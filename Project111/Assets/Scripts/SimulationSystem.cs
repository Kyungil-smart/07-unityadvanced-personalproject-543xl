using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationSystem : MonoBehaviour
{
    public PipeAutoLinker autoLinker;
    public bool rebuildPipesBeforeSim = true;

    // 파이프 색/과부하 표시용: 이번 턴 파이프 유량(손실/용량 적용 후)
    public Dictionary<PipePiece, float> lastPipeFlowAfterLoss = new();

    [ContextMenu("Simulate Turn (Pipe Graph)")]
    public void SimulateTurn()
    {
        if (rebuildPipesBeforeSim && autoLinker != null)
            autoLinker.RebuildPipes();

        lastPipeFlowAfterLoss.Clear();

        var nodes = FindObjectsOfType<WaterNode>().ToList();
        foreach (var n in nodes) n.ClearRuntime();

        var sources = nodes.Where(n => n.kind == NodeKind.Source).ToList();
        if (sources.Count != 1)
        {
            Debug.LogError($"[PipeSim] Source는 1개여야 합니다. 현재 {sources.Count}개");
            return;
        }

        var src = sources[0];
        if (src.sourcePipes == null || src.sourcePipes.Length == 0 || src.sourcePipes.Any(p => p == null))
        {
            Debug.LogError("[PipeSim] Source의 sourcePipes(주입 파이프)를 Inspector에서 지정하세요.");
            return;
        }

        // District 파이프 매핑: pipe -> districts
        var pipeToDistricts = new Dictionary<PipePiece, List<WaterNode>>();
        foreach (var d in nodes.Where(n => n.kind == NodeKind.District))
        {
            if (d.districtPipe == null)
            {
                Debug.LogWarning($"[PipeSim] District '{d.name}' districtPipe가 비어있음");
                continue;
            }

            if (!pipeToDistricts.TryGetValue(d.districtPipe, out var list))
            {
                list = new List<WaterNode>();
                pipeToDistricts[d.districtPipe] = list;
            }
            list.Add(d);
        }

        // 1) 멀티루트 BFS 트리 생성 (루프 제거)
        var parent = new Dictionary<PipePiece, PipePiece>();
        var children = new Dictionary<PipePiece, List<PipePiece>>();
        var order = BuildMultiRootBfsTree(src.sourcePipes, parent, children);

        // 2) 하위 수요(트리 기준) 계산 (✅ priority 반영)
        var subtreeDemand = ComputeSubtreeDemand(order, children, pipeToDistricts);

        // 3) 유량 전파
        var pipeFlowIn = new Dictionary<PipePiece, float>();
        foreach (var p in order) pipeFlowIn[p] = 0f;

        float perRoot = Mathf.Max(0f, src.production) / src.sourcePipes.Length;
        foreach (var rp in src.sourcePipes) pipeFlowIn[rp] += perRoot;

        foreach (var p in order)
        {
            float incoming = pipeFlowIn[p];
            if (incoming <= 0f) continue;

            // 파손이면 막힘
            if (p.IsBroken)
            {
                lastPipeFlowAfterLoss[p] = 0f;
                continue;
            }

            // 용량 + 손실
            float capped = Mathf.Min(incoming, p.capacity);
            float afterLoss = capped * (1f - p.TotalLossRate);
            afterLoss = Mathf.Max(0f, afterLoss);

            lastPipeFlowAfterLoss[p] = afterLoss;

            float remaining = afterLoss;

            // (A) 이 파이프에 붙은 District 수요 채우기 (✅ priority 반영 + used 버그 수정)
            if (pipeToDistricts.TryGetValue(p, out var districtsHere) && remaining > 0f)
            {
                remaining = DistributeToDistrictsWithPriority(remaining, districtsHere);
            }

            // (B) 자식 파이프로 분배(하위 수요 가중치) (✅ subtreeDemand가 이미 priority 반영)
            if (children.TryGetValue(p, out var childList) && childList.Count > 0 && remaining > 0f)
            {
                float totalChildDemand = childList.Sum(c => subtreeDemand.GetValueOrDefault(c, 0f));

                if (totalChildDemand <= 0.0001f)
                {
                    float each = remaining / childList.Count;
                    foreach (var c in childList) pipeFlowIn[c] += each;
                }
                else
                {
                    foreach (var c in childList)
                    {
                        float w = subtreeDemand.GetValueOrDefault(c, 0f) / totalChildDemand;
                        pipeFlowIn[c] += remaining * w;
                    }
                }
            }
        }

        // 4) coverage 계산
        foreach (var d in nodes.Where(n => n.kind == NodeKind.District))
        {
            float dem = Mathf.Max(0.0001f, d.demand);
            d.coverage = Mathf.Clamp01(d.delivered / dem);
            Debug.Log($"[District] {d.name} delivered={d.delivered:0.#}/{d.demand:0.#} ({d.coverage * 100f:0.#}%)");
        }

        foreach (var node in nodes)
            node.UpdateColor();
    }

    /// <summary>
    /// remaining 물을 districtsHere에 분배.
    /// - 가중치: demand * priority
    /// - 미충족(unmet) 있는 대상만 참여
    /// - 어떤 애가 cap에 걸려 남는 물이 생기면 재분배(몇 회 반복)
    /// </summary>
    private float DistributeToDistrictsWithPriority(float remaining, List<WaterNode> districtsHere)
    {
        const float EPS = 0.0001f;

        // 안전 반복(대상이 적으니 5회면 충분)
        for (int iter = 0; iter < 5 && remaining > EPS; iter++)
        {
            // 미충족만
            var candidates = districtsHere
                .Where(d => d != null)
                .Select(d => new
                {
                    d,
                    unmet = Mathf.Max(0f, d.demand - d.delivered),
                    weight = Mathf.Max(0f, d.demand) * Mathf.Max(0.5f, d.priority) // priority 최소 0.5 가정
                })
                .Where(x => x.unmet > EPS && x.weight > EPS)
                .ToList();

            if (candidates.Count == 0) break;

            float totalW = candidates.Sum(x => x.weight);
            if (totalW <= EPS) break;

            float usedThisIter = 0f;

            foreach (var x in candidates)
            {
                float share = remaining * (x.weight / totalW);
                float give = Mathf.Min(share, x.unmet);
                if (give <= 0f) continue;

                x.d.delivered += give;
                usedThisIter += give;
            }

            remaining = Mathf.Max(0f, remaining - usedThisIter);

            // 이번 반복에서 거의 못 줬으면 종료
            if (usedThisIter <= EPS) break;
        }

        return remaining;
    }

    private List<PipePiece> BuildMultiRootBfsTree(
        PipePiece[] roots,
        Dictionary<PipePiece, PipePiece> parent,
        Dictionary<PipePiece, List<PipePiece>> children)
    {
        parent.Clear();
        children.Clear();

        var visited = new HashSet<PipePiece>();
        var q = new Queue<PipePiece>();
        var order = new List<PipePiece>();

        foreach (var r in roots)
        {
            if (r == null) continue;
            if (visited.Add(r)) q.Enqueue(r);
        }

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            order.Add(cur);

            if (!children.ContainsKey(cur))
                children[cur] = new List<PipePiece>();

            foreach (var nb in cur.neighbors)
            {
                if (nb == null) continue;
                if (visited.Contains(nb)) continue;

                visited.Add(nb);
                parent[nb] = cur;
                children[cur].Add(nb);
                q.Enqueue(nb);
            }
        }

        return order;
    }

    private Dictionary<PipePiece, float> ComputeSubtreeDemand(
        List<PipePiece> bfsOrder,
        Dictionary<PipePiece, List<PipePiece>> children,
        Dictionary<PipePiece, List<WaterNode>> pipeToDistricts)
    {
        var demand = new Dictionary<PipePiece, float>();

        // ✅ local demand = sum(demand * priority)
        foreach (var p in bfsOrder)
        {
            float local = 0f;
            if (pipeToDistricts.TryGetValue(p, out var ds))
            {
                local = ds.Sum(d =>
                {
                    float dem = Mathf.Max(0f, d.demand);
                    float pri = Mathf.Max(0.5f, d.priority);
                    return dem * pri;
                });
            }
            demand[p] = local;
        }

        // bottom-up 누적
        for (int i = bfsOrder.Count - 1; i >= 0; i--)
        {
            var p = bfsOrder[i];
            if (!children.TryGetValue(p, out var childList)) continue;
            demand[p] += childList.Sum(c => demand.GetValueOrDefault(c, 0f));
        }

        return demand;
    }
}