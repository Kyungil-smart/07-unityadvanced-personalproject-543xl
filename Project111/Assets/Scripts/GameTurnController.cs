using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTurnController : MonoBehaviour
{
    [Header("UI")]
    public Button nextTurnButton;
    public TMP_Text turnText;

    [Header("Systems")]
    public SimulationSystem simulation;
    public PipeVisualController pipeVisual;
    public DistrictWorldLabelUpdater labelUpdater;
    public WaterEventSystem eventSystem;
    public PipeAutoLinker autoLinker;
    public UIEventDisplay eventUI;
    
    [Header("Turn")]
    public int turn = 0;

    private void Start()
    {
        if (nextTurnButton != null)
            nextTurnButton.onClick.AddListener(NextTurn);

        RefreshUI();
    }

    public void NextTurn()
    {
        turn++;

        if (autoLinker != null) autoLinker.RebuildPipes(); // 네 AutoLinker 함수명에 맞게

        if (eventSystem != null)
        {
            eventSystem.OnTurnStart(); // ✅ 이벤트 적용
            eventUI.Refresh();
        }

        if (simulation != null) simulation.SimulateTurn();   // ✅ 시뮬

        if (labelUpdater != null) labelUpdater.RefreshAllDistrictLabels();
        if (pipeVisual != null) pipeVisual.Refresh(simulation);

        if (eventSystem != null)
        {
            eventSystem.OnTurnEnd(); // ✅ 지속시간 감소/종료
            eventUI.Refresh();
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (turnText != null) turnText.text = $"Turn: {turn}";
    }
}