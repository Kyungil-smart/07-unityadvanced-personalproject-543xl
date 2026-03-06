using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PipePiece : MonoBehaviour
{
    [Header("Pipe Params")]
    [Range(0f, 0.99f)] public float baseLossRate = 0.08f;
    public float capacity = 120f;
    [Range(0f, 100f)] public float health = 100f;
    public float brokenHealth = 20f;
    
    [Header("Collider")]
    public Collider col;

    // ✅ 런타임 전용(직렬화 금지)
    [System.NonSerialized] public List<PipePiece> neighbors = new();

    private void Reset()
    {
        col = GetComponent<Collider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
    }

    private void OnValidate()
    {
        if (col == null) col = GetComponent<Collider>();
    }

    public bool IsBroken => health <= brokenHealth;

    public float TotalLossRate
    {
        get
        {
            float extra = 0f;
            if (health < 40f)
            {
                float t = Mathf.InverseLerp(40f, 0f, health);
                extra = Mathf.Lerp(0f, 0.25f, t);
            }
            return Mathf.Clamp01(baseLossRate + extra);
        }
    }
}