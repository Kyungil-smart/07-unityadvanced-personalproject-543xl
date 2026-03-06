using System.Linq;
using UnityEngine;

public class PipeVisualController : MonoBehaviour
{
    [Header("Thresholds")]
    [Range(0f, 2f)] public float overloadRatio = 0.90f;  // 90% 넘으면 과부하 표시
    [Range(0f, 100f)] public float brokenHealth = 20f;   // health <= 20이면 파손 표시

    [Header("Colors")]
    public Color normalColor = new Color(0.15f, 0.45f, 1f);
    public Color overloadColor = new Color(1f, 0.55f, 0.1f);
    public Color brokenColor = new Color(1f, 0.2f, 0.2f);

    public void Refresh(SimulationSystem sim)
    {
        if (sim == null) return;

        var pipes = FindObjectsOfType<PipePiece>(true).ToList();

        foreach (var p in pipes)
        {
            if (p == null) continue;

            var renderer = p.GetComponentInChildren<MeshRenderer>();
            if (renderer == null) continue;

            // 파손 우선
            if (p.health <= brokenHealth)
            {
                SetColor(renderer, brokenColor);
                continue;
            }

            // 이번 턴 흐름 기반 과부하
            float flow = 0f;
            if (sim.lastPipeFlowAfterLoss != null && sim.lastPipeFlowAfterLoss.TryGetValue(p, out var f))
                flow = f;

            float ratio = (p.capacity <= 0.0001f) ? 0f : flow / p.capacity;

            if (ratio >= overloadRatio) SetColor(renderer, overloadColor);
            else SetColor(renderer, normalColor);
        }
    }

    private void SetColor(MeshRenderer r, Color c)
    {
        // 머티리얼 인스턴스 공유 이슈 피하려면 material 사용
        if (r != null) r.material.color = c;
    }
}