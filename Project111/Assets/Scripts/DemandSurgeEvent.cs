using UnityEngine;

public class DemandSurgeEvent : BaseGameEvent
{
    public override string Name => "수요 급증";
    public override string Description => "랜덤 구역 수요 +40% (2턴)";

    private readonly float multiplier;
    private WaterNode target;
    private float originalDemand;

    public DemandSurgeEvent(float multiplier = 1.4f, int duration = 2) : base(duration)
    {
        this.multiplier = multiplier;
    }

    public override void Apply(GameState state)
    {
        var districts = state.Districts;
        if (districts.Count == 0) return;

        target = districts[Random.Range(0, districts.Count)];
        if (target == null) return;

        originalDemand = target.demand;
        target.demand *= multiplier;
    }

    public override void Revert(GameState state)
    {
        if (target == null) return;
        target.demand = originalDemand;
    }
}