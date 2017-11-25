public class CivilShip : LivingEntity
{
    public override Weapon Weapon => null;

    protected override void ResetAbilities()
    {
        CanMove = true;
        CanAttack = false;
        CanBoard = false;
        CanBeBoarded = true;
    }

    protected class CivilShipSerializationNames : LivingEntitySerializationNames
    {

    }
}