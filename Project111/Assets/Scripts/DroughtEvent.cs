using UnityEngine;

public class DroughtEvent : BaseGameEvent
{
    public override string Name => "가뭄";
    public override string Description => "소스 생산량 -30% (2턴)";

    private readonly float multiplier;
    private float originalProduction;
    private WaterNode target;

    public DroughtEvent(float multiplier = 0.7f, int duration = 2) : base(duration)
    {
        this.multiplier = multiplier;
    }

    public override void Apply(GameState state)
    {
        target = state.Source;
        if (target == null) return;

        originalProduction = target.production;
        target.production *= multiplier;
    }

    public override void Revert(GameState state)
    {
        if (target == null) return;
        target.production = originalProduction;
    }
}