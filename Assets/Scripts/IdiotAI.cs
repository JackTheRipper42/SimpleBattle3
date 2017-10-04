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
            var enemy = GetCloestEnemy(ship, enemyShips);

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

                var fullPath = astar.Calculate(ship.Position, enemy.Position, obstacles);
                var pathToFirePosition = fullPath.Take(fullPath.Count - ship.FireRange).ToList();

                var path = pathToFirePosition.Count > ship.MovementRange + 2
                    ? pathToFirePosition.Take(ship.MovementRange + 1).ToList()
                    : pathToFirePosition;
                yield return ship.Move(path, speed);

                if (GridPosition.Distance(ship.Position, enemy.Position) <= ship.FireRange)
                {
                    yield return ship.Attack(enemy);
                }
            }
        }

    }

    private static Ship GetCloestEnemy(Entity self, IEnumerable<Ship> enemies)
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
}