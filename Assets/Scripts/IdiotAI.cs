using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class IdiotAI
{
    public IEnumerator CalculateTurn(List<Entity> entities, Astar astar, Side side, float movementSpeed, float salvoFlightTime)
    {
        var ownEntities = entities.OfType<LivingEntity>().Where(entity => entity.Side == side).ToList();
        var enemyShips = entities.OfType<LivingEntity>().Where(entity => entity.Side != side).ToList();

        foreach (var own in ownEntities)
        {
            if (!own.CanAttack)
            {
                continue;
            }

            var blackList = new List<LivingEntity>();
            bool selectNewEnemy;
            do
            {
                selectNewEnemy = false;
                var enemy = GetNearestEnemy(own, enemyShips.Except(blackList));

                if (GridPosition.Distance(own.Position, enemy.Position) <= own.Weapon.Range)
                {
                    yield return own.Attack(enemy, salvoFlightTime);
                }
                else
                {
                    var obstacles = entities
                        .Except(ownEntities)
                        .Where(entity => entity != enemy)
                        .Select(entity => entity.Position)
                        .ToList();

                    var destinations = GetDestinations(enemy.Position, own.Weapon.Range)
                        .Except(entities.Select(entity => entity.Position))
                        .Distinct();

                    IList<GridPosition> shortestPath = null;
                    foreach (var destination in destinations)
                    {
                        var path = astar.Calculate(own.Position, destination, obstacles);
                        if (shortestPath == null || path.Count < shortestPath.Count)
                        {
                            shortestPath = path;
                        }
                    }

                    if (shortestPath != null)
                    {
                        var path = shortestPath.Count > own.MovementRange + 2
                            ? shortestPath.Take(own.MovementRange + 1).ToList()
                            : shortestPath;
                        yield return own.Move(path, movementSpeed);

                        if (GridPosition.Distance(own.Position, enemy.Position) <= own.Weapon.Range)
                        {
                            yield return own.Attack(enemy, salvoFlightTime);
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

    private static LivingEntity GetNearestEnemy(Entity self, IEnumerable<LivingEntity> enemies)
    {
        int minDistance = int.MaxValue;
        LivingEntity closest = null;

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