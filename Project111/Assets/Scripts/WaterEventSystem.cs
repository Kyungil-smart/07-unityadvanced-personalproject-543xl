using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterEventSystem : MonoBehaviour
{
    [Range(0f, 1f)] public float eventChancePerTurn = 0.4f;

    public GameState state;
    [SerializeField] private List<BaseGameEvent> activeEvents = new();

    public IReadOnlyList<BaseGameEvent> ActiveEvents => activeEvents;

    private void Awake()
    {
        RefreshState();
    }

    public void RefreshState()
    {
        state = new GameState(
            FindObjectsOfType<WaterNode>().ToList(),
            FindObjectsOfType<PipePiece>().ToList()
        );
    }

    public void OnTurnStart()
    {
        RefreshState();

        if (state.Source == null)
        {
            Debug.LogError("[WaterEventSystem] kind=Source 인 WaterNode가 없습니다.");
            return;
        }

        if (Random.value <= eventChancePerTurn)
        {
            var ev = CreateRandomEvent();
            if (ev != null)
            {
                ev.Apply(state);
                activeEvents.Add(ev);
                Debug.Log($"[Event] 발생: {ev.Name} - {ev.Description}");
            }
        }
    }

    public void OnTurnEnd()
    {
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            var ev = activeEvents[i];
            ev.Tick();

            if (ev.RemainingTurns <= 0)
            {
                ev.Revert(state);
                Debug.Log($"[Event] 종료: {ev.Name}");
                activeEvents.RemoveAt(i);
            }
        }
    }

    public string GetActiveEventText()
    {
        if (activeEvents.Count == 0) return "이벤트 없음";
        return string.Join("\n", activeEvents.Select(e => $"{e.Name} ({e.RemainingTurns}턴)"));
    }

    private BaseGameEvent CreateRandomEvent()
    {
        int r = Random.Range(0, 4);
        return r switch
        {
            0 => new DroughtEvent(),
            1 => new PipeBreakEvent(),
            _ => new DemandSurgeEvent(),
        };
    }
}