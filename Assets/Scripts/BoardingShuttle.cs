public class BoardingShuttle : LivingEntity
{
    public Weapon DefenseWeapon;

    public override Weapon Weapon => DefenseWeapon;

    protected override void ResetAbilities()
    {
        CanMove = true;
        CanAttack = false;
        CanBoard = true;
        CanBeBoarded = false;
    }

    protected override void Serialize(SerializationInfo serializationInfo)
    {
        base.Serialize(serializationInfo);

        serializationInfo.SetValue(BoardingShuffleSerializationNames.DefenseWeapon, DefenseWeapon);
    }

    protected override void Deserialize(SerializationInfo serializationInfo)
    {
        base.Deserialize(serializationInfo);

        DefenseWeapon = serializationInfo.GetValue<Weapon>(BoardingShuffleSerializationNames.DefenseWeapon);
    }

    protected class BoardingShuffleSerializationNames : LivingEntitySerializationNames
    {
        public const string DefenseWeapon = "DefenseWeapon";
    }
}