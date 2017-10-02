using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

public class Astar
{

    public IList<GridPosition> Calculate(GridPosition start, GridPosition goal, ICollection<GridPosition> obstacles)
    {
        var startNode = new Node(start);
        var goalNode = new Node(goal);
        var closedset = new HashSet<GridPosition>(obstacles);
        var openset = new HashSet<Node> {startNode};
        var cameFrom = new Dictionary<Node, Node>();

        startNode.Gscore = 0;
        startNode.Fscore = startNode.Gscore + HeuristicCostEstimate(startNode, goalNode);

        while (openset.Any())
        {
            Node currentNode = null;
            var minFscore = float.MaxValue;
            foreach (var node in openset)
            {
                if (node.Fscore < minFscore)
                {
                    minFscore = node.Fscore;
                    currentNode = node;
                }
            }

            if (currentNode == null)
            {
                return null;
            }

            if (currentNode.Position == goal)
            {
                return ReconstuctPath(cameFrom, goalNode);
            }
            openset.Remove(currentNode);
            closedset.Add(currentNode.Position);

            foreach (var neighborNode in GetNeighborNodes(currentNode.Position, closedset))
            {
                var tentativeGscore = currentNode.Gscore + GetCost(currentNode.Position, neighborNode.Position);

                if (!openset.Contains(neighborNode) || tentativeGscore < neighborNode.Gscore)
                {
                    if (cameFrom.ContainsKey(neighborNode))
                    {
                        cameFrom[neighborNode] = currentNode;
                    }
                    else
                    {
                        cameFrom.Add(neighborNode, currentNode);
                    }

                    neighborNode.Gscore = tentativeGscore;
                    neighborNode.Fscore = neighborNode.Gscore + HeuristicCostEstimate(neighborNode, goalNode);

                    if (!openset.Contains(neighborNode))
                    {
                        openset.Add(neighborNode);
                    }
                }
            }
        }
        return null;
    }

    private static float HeuristicCostEstimate([NotNull] Node start, [NotNull] Node goal)
    {
        return GridPosition.Distance(start.Position, goal.Position);
    }

    private static float GetCost(GridPosition start, GridPosition goal)
    {
        return GridPosition.Distance(start, goal);
    }

    private static IList<GridPosition> ReconstuctPath([NotNull] IDictionary<Node, Node> cameFrom, [NotNull] Node current)
    {
        var totalpath = new List<GridPosition> {current.Position};
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalpath.Insert(0, current.Position);
        }
        return totalpath;
    }

    private static IEnumerable<Node> GetNeighborNodes(GridPosition position, [NotNull] ICollection<GridPosition> closedset)
    {
        var neighbors = new List<Node>(6);

        foreach (var neighbor in position.Neighbors)
        {
            if (!closedset.Contains(neighbor))
            {
                neighbors.Add(new Node(neighbor));
            }
        }

        return neighbors;
    }
}