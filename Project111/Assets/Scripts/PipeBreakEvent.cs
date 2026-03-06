using UnityEngine;

public class PipeBreakEvent : BaseGameEvent
{
    public override string Name => "관 파손";
    public override string Description => "랜덤 관로 손실 +20%p (3턴)";

    private readonly float addLoss;
    private PipePiece target;
    private float originalLoss;

    public PipeBreakEvent(float addLoss = 0.20f, int duration = 3) : base(duration)
    {
        this.addLoss = addLoss;
    }

    public override void Apply(GameState state)
    {
        if (state.pipes == null || state.pipes.Count == 0) return;

        target = state.pipes[Random.Range(0, state.pipes.Count)];
        if (target == null) return;

        originalLoss = target.baseLossRate;
        target.baseLossRate = Mathf.Clamp01(target.baseLossRate + addLoss);
    }

    public override void Revert(GameState state)
    {
        if (target == null) return;
        target.baseLossRate = originalLoss;
    }
}