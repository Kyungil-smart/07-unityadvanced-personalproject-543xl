public abstract class BaseGameEvent : IGameEvent
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public int RemainingTurns { get; protected set; }

    protected BaseGameEvent(int duration)
    {
        RemainingTurns = duration;
    }

    public abstract void Apply(GameState state);
    public abstract void Revert(GameState state);

    public void Tick() => RemainingTurns--;
}