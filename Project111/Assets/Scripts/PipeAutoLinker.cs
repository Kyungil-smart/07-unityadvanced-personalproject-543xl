using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeAutoLinker : MonoBehaviour
{
    public float pipeLinkDistance = 0.25f;
    public bool rebuildOnStart = true;

    private List<PipePiece> _pipes = new();

    private void Start()
    {
        if (rebuildOnStart) RebuildPipes();
    }

    [ContextMenu("Rebuild Pipes Only")]
    public void RebuildPipes()
    {
        _pipes = FindObjectsOfType<PipePiece>().ToList();

        foreach (var p in _pipes)
            p.neighbors.Clear();

        for (int i = 0; i < _pipes.Count; i++)
        {
            var a = _pipes[i];
            if (a == null || a.col == null) continue;

            for (int j = i + 1; j < _pipes.Count; j++)
            {
                var b = _pipes[j];
                if (b == null || b.col == null) continue;

                if (AreClose(a.col, b.col, pipeLinkDistance))
                {
                    a.neighbors.Add(b);
                    b.neighbors.Add(a);
                }
            }
        }

        Debug.Log($"[PipeAutoLinker] pipes={_pipes.Count}");
    }

    private bool AreClose(Collider a, Collider b, float maxDist)
    {
        Vector3 pa = a.ClosestPoint(b.bounds.center);
        Vector3 pb = b.ClosestPoint(a.bounds.center);
        return Vector3.Distance(pa, pb) <= maxDist;
    }
}