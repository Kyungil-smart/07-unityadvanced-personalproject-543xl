using System.Collections.Generic;
using System.Linq;

public class GameState
{
    public List<WaterNode> nodes;
    public List<PipePiece> pipes;

    public GameState(List<WaterNode> nodes, List<PipePiece> pipes)
    {
        this.nodes = nodes;
        this.pipes = pipes;
    }

    public WaterNode Source => nodes.FirstOrDefault(n => n != null && n.kind == NodeKind.Source);

    public List<WaterNode> Districts =>
        nodes.Where(n => n != null && n.kind == NodeKind.District).ToList();
}