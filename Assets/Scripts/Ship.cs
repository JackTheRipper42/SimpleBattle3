public class Ship : InteractiveEntity
{
    public int MovementRange = 4;
    public int FireRange = 1;

    public new void Move(GridPosition position)
    {
        base.Move(position);
    }

    public new void Kill()
    {
        base.Kill();
    }
}