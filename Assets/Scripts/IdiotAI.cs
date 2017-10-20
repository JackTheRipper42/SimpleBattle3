using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class IdiotAI
{
    public IEnumerator CalculateTurn(List<Entity> entities, Astar astar, Side side, float speed)
    {
        var ownShips = entities.OfType<Ship>().Where(ship => ship.Side == side).ToList();
        var enemyShips = entities.OfType<Ship>().Where(ship => ship.Side != side).ToList();

        foreach (var ship in ownShips)
        {
            var blackList = new List<Ship>();
            bool selectNewEnemy;
            do
            {
                selectNewEnemy = false;
                var enemy = GetNearestEnemy(ship, enemyShips.Except(blackList));

                if (GridPosition.Distance(ship.Position, enemy.Position) <= ship.FireRange)
                {
                    yield return ship.Attack(enemy);
                }
                else
                {
                    var obstacles = entities
                        .Except(ownShips)
                        .Where(entity => entity != enemy)
                        .Select(entity => entity.Position)
                        .ToList();

                    var destinations = GetDestinations(enemy.Position, ship.FireRange)
                        .Except(entities.Select(entity => entity.Position))
                        .Distinct();

                    IList<GridPosition> shortestPath = null;
                    foreach (var destination in destinations)
                    {
                        var path = astar.Calculate(ship.Position, destination, obstacles);
                        if (shortestPath == null || path.Count < shortestPath.Count)
                        {
                            shortestPath = path;
                        }
                    }

                    if (shortestPath != null)
                    {
                        var path = shortestPath.Count > ship.MovementRange + 2
                            ? shortestPath.Take(ship.MovementRange + 1).ToList()
                            : shortestPath;
                        yield return ship.Move(path, speed);

                        if (GridPosition.Distance(ship.Position, enemy.Position) <= ship.FireRange)
                        {
                            yield return ship.Attack(enemy);
                        }

                    }
                    else
                    {
                        blackList.Add(enemy);
                        selectNewEnemy = true;
                    }
                }
            } while (selectNewEnemy && blackList.Count < enemyShips.Count);
        }
    }

    private static Ship GetNearestEnemy(Entity self, IEnumerable<Ship> enemies)
    {
        int minDistance = int.MaxValue;
        Ship closest = null;

        foreach (var enemy in enemies)
        {
            var distance = GridPosition.Distance(self.Position, enemy.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private static List<GridPosition> GetDestinations(GridPosition position, int range)
    {
        var destinations = new List<GridPosition>();

        if (range <= 0)
        {
            return destinations;
        }

        foreach (var neighbor in position.Neighbors)
        {
            destinations.Add(neighbor);
            destinations.AddRange(GetDestinations(neighbor, range - 1));

        }

        return destinations;
    }

}