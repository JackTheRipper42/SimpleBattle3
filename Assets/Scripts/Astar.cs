using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

public class Astar
{
    public IList<GridPosition> Calculate(GridPosition start, GridPosition goal, ICollection<GridPosition> obstacles)
    {
        var startNode = new Node(start);
        var goalNode = new Node(goal);
        var closed = new HashSet<GridPosition>(obstacles);
        var open = new HashSet<Node> {startNode};
        var cameFrom = new Dictionary<Node, Node>();

        startNode.Gscore = 0;
        startNode.Fscore = startNode.Gscore + HeuristicCostEstimate(startNode, goalNode);

        while (open.Any())
        {
            Node currentNode = null;
            var minFScore = float.MaxValue;
            foreach (var node in open)
            {
                if (node.Fscore < minFScore)
                {
                    minFScore = node.Fscore;
                    currentNode = node;
                }
            }

            if (currentNode == null)
            {
                return null;
            }

            if (currentNode.Position == goal)
            {
                return ReconstructPath(cameFrom, goalNode);
            }
            open.Remove(currentNode);
            closed.Add(currentNode.Position);

            foreach (var neighborNode in GetNeighborNodes(currentNode.Position, closed))
            {
                var tentativeGScore = currentNode.Gscore + GetCost(currentNode.Position, neighborNode.Position);

                if (!open.Contains(neighborNode) || tentativeGScore < neighborNode.Gscore)
                {
                    if (cameFrom.ContainsKey(neighborNode))
                    {
                        cameFrom[neighborNode] = currentNode;
                    }
                    else
                    {
                        cameFrom.Add(neighborNode, currentNode);
                    }

                    neighborNode.Gscore = tentativeGScore;
                    neighborNode.Fscore = neighborNode.Gscore + HeuristicCostEstimate(neighborNode, goalNode);

                    if (!open.Contains(neighborNode))
                    {
                        open.Add(neighborNode);
                    }
                }
            }
        }
        return null;
    }

    public IList<GridPosition> GetDestinations(
        GridPosition position,
        HashSet<GridPosition> obstacles,
        HashSet<GridPosition> entities,
        int range)
    {
        var destinations = new List<GridPosition>();

        if (range < 0)
        {
            return destinations;
        }

        if (!entities.Contains(position))
        {
            destinations.Add(position);
        }

        foreach (var neighbor in position.Neighbors)
        {
            if (!obstacles.Contains(neighbor))
            {
                destinations.AddRange(GetDestinations(neighbor, obstacles, entities, range - 1));
            }
        }

        return destinations.Distinct().ToList();
    }

    private static float HeuristicCostEstimate([NotNull] Node start, [NotNull] Node goal)
    {
        return GridPosition.Distance(start.Position, goal.Position);
    }

    private static float GetCost(GridPosition start, GridPosition goal)
    {
        return GridPosition.Distance(start, goal);
    }

    private static IList<GridPosition> ReconstructPath([NotNull] IDictionary<Node, Node> cameFrom, [NotNull] Node current)
    {
        var path = new List<GridPosition> {current.Position};
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current.Position);
        }
        return path;
    }

    private static IEnumerable<Node> GetNeighborNodes(GridPosition position, [NotNull] ICollection<GridPosition> closed)
    {
        var neighbors = new List<Node>(6);

        foreach (var neighbor in position.Neighbors)
        {
            if (!closed.Contains(neighbor))
            {
                neighbors.Add(new Node(neighbor));
            }
        }

        return neighbors;
    }
}