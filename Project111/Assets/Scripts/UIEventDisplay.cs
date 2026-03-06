using TMPro;
using UnityEngine;

public class UIEventDisplay : MonoBehaviour
{
    [Header("Assign")]
    public WaterEventSystem eventSystem;
    public TMP_Text eventText;

    private void Awake()
    {
        if (eventSystem == null)
            eventSystem = FindObjectOfType<WaterEventSystem>();
    }

    /// <summary>
    /// 턴 시작/끝/이벤트 발생 직후 호출해서 UI 갱신
    /// </summary>
    public void Refresh()
    {
        if (eventText == null || eventSystem == null) return;

        // 이름 + 남은 턴 + 설명까지 표시
        var list = eventSystem.ActiveEvents;
        if (list == null || list.Count == 0)
        {
            eventText.text = "이벤트 없음";
            return;
        }

        // 여러 이벤트가 동시에 있을 수 있으니 줄바꿈
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var e in list)
        {
            sb.AppendLine($"[{e.Name}] (남은 턴 : {e.RemainingTurns}턴)");
            sb.AppendLine($"{e.Description}");
            sb.AppendLine(); // 한 줄 띄우기
        }

        eventText.text = sb.ToString().TrimEnd();
    }
}