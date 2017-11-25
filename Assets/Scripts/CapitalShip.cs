public class CapitalShip : LivingEntity
{
    public Weapon MainWeapon;

    public override Weapon Weapon => MainWeapon;

    protected override void ResetAbilities()
    {
        CanMove = true;
        CanAttack = true;
        CanBoard = false;
        CanBeBoarded = true;
    }

    protected override void Serialize(SerializationInfo serializationInfo)
    {
        base.Serialize(serializationInfo);

        serializationInfo.SetValue(CapitalShipSerializationNames.MainWeapon, MainWeapon);
    }

    protected override void Deserialize(SerializationInfo serializationInfo)
    {
        base.Deserialize(serializationInfo);

        MainWeapon = serializationInfo.GetValue<Weapon>(CapitalShipSerializationNames.MainWeapon);
    }

    protected class CapitalShipSerializationNames : LivingEntitySerializationNames
    {
        public const string MainWeapon = "MainWeapon";
    }
}