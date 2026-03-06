using TMPro;
using UnityEngine;

public class DistrictWorldLabel : MonoBehaviour
{
    public WaterNode node;

    [Header("Assign in Inspector")]
    [Tooltip("Canvas에 미리 만들어둔 TMP 텍스트를 드래그해서 연결")]
    public TMP_Text label;

    private void Reset()
    {
        node = GetComponent<WaterNode>();
    }

    /// <summary>
    /// Turn 이후(시뮬레이션 끝난 뒤) 호출해서 UI 텍스트 갱신
    /// </summary>
    public void Refresh()
    {
        if (node == null) node = GetComponent<WaterNode>();
        if (node == null || label == null) return;

        // District만 표시(원하면 Source도 표시 가능)
        if (node.kind != NodeKind.District)
        {
            label.text = "";
            return;
        }

        float pct = node.coverage * 100f;
        float lack = Mathf.Max(0f, node.demand - node.delivered);

        label.text = $"{pct:0.#}%";
    }

    /// <summary>
    /// 라벨을 연결했는지 빠르게 확인용
    /// </summary>
    [ContextMenu("DEBUG: Refresh Now")]
    private void DebugRefreshNow() => Refresh();
}