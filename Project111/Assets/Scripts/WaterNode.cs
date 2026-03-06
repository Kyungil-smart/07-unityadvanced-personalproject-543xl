using UnityEngine;

public enum NodeKind { Source, District }

public class WaterNode : MonoBehaviour
{
    public NodeKind kind = NodeKind.District;

    [Header("Source")]
    public float production = 120f;
    [Tooltip("소스가 물을 주입하는 시작 파이프들(수동 지정)")]
    public PipePiece[] sourcePipes;

    [Header("District")]
    public float demand = 60f;
    [Tooltip("구역이 물을 받는 파이프(수동 지정)")]
    public PipePiece districtPipe;

    [Header("Visual")]
    public Renderer nodeRenderer;
    
    [Header("Control")]
    [Range(0.5f, 2.0f)]
    public float priority = 1f;
    
    [Header("Runtime")]
    public float delivered;
    public float coverage;

    public void ClearRuntime()
    {
        delivered = 0f;
        coverage = 0f;
    }
    
    public void UpdateColor()
    {
        if (nodeRenderer == null) return;

        Color c = Color.white;

        if (coverage >= 0.95f)
            c = Color.green;
        else if (coverage >= 0.80f)
            c = Color.yellow;
        else
            c = Color.red;

        nodeRenderer.material.color = c;
    }
}