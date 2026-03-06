public interface IGameEvent
{
    string Name { get; }
    string Description { get; }
    int RemainingTurns { get; }

    void Apply(GameState state);
    void Revert(GameState state);
    void Tick();
}