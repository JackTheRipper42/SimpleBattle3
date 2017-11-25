using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class LivingEntity : Entity
{
    public Side Side;
    public int MovementRange = 4;
    public Shield Shield;
    public Structure Structure;

    public GameObject Model;
    public AudioSource FireAudio;
    public AudioSource ThrusterAudio;

    public GameObject[] ExplosionPrefabs;
    public GameObject[] HitPrefabs;
    public GameObject UIPrefab;

    public bool IsAlive => Structure.HitPoints > 0;

    public EntityUI UI { get; private set; }

    public bool CanMove { get; protected set; }

    public bool CanAttack { get; protected set; }

    public bool CanBoard { get; protected set; }

    public bool CanBeBoarded { get; protected set; }

    public abstract Weapon Weapon { get; }

    protected override void Awake()
    {
        base.Awake();
        var uiObject = Instantiate(UIPrefab, transform);
        UI = uiObject.GetComponent<EntityUI>();
    }

    protected override void Start()
    {
        base.Start();
        if (IsLoading)
        {
            if (MissionManager.PlayerSide == Side)
            {
                UI.EnableCanFireMarker(CanAttack);
                UI.EnableCanMoveMarker(CanMove);
                UI.EnableCanBoardMarker(CanBoard);
            }
            else
            {
                UI.EnableCanFireMarker(false);
                UI.EnableCanMoveMarker(false);
                UI.EnableCanBoardMarker(false);

            }
            UI.UpdateStructure(Structure);
            UI.UpdateShield(Shield);
        }
        else
        {
            Structure.HitPoints = Structure.MaxHitPoints;
            Shield.HitPoints = Shield.MaxHitPoints;
            StartTurn();
        }
    }

    public void StartTurn()
    {
        ResetAbilities();
        if (MissionManager.PlayerSide == Side)
        {
            UI.EnableCanFireMarker(CanAttack);
            UI.EnableCanMoveMarker(CanMove);
            UI.EnableCanBoardMarker(CanBoard);
        }
    }

    protected abstract void ResetAbilities();

    public override bool IsObstacle(Side side)
    {
        return side != Side;
    }

    public IEnumerator Attack(LivingEntity target, float salvoFlightTime)
    {
        var range = GridPosition.Distance(Position, target.Position);
        if (!CanAttack)
        {
            throw new InvalidOperationException($"The entity '{this}' cannot attack.");
        }
        if (range > Weapon.Range)
        {
            throw new InvalidOperationException($"The target '{target}' is out of range.");
        }

        CanAttack = false;
        UI.EnableCanFireMarker(CanAttack);

        if (target.Weapon != null &&
            range <= target.Weapon.Range &&
            Random.Range(0, 10) >= 7)
        {
            yield return FireSalvo(target, this, salvoFlightTime);
            if (IsAlive)
            {
                yield return FireSalvo(this, target, salvoFlightTime);
            }
        }
        else
        {
            yield return FireSalvo(this, target, salvoFlightTime);
            if (target.IsAlive &&
                target.Weapon != null &&
                range <= target.Weapon.Range)
            {
                yield return FireSalvo(target, this, salvoFlightTime);
            }
        }
    }

    public IEnumerator Move(IList<GridPosition> path, float speed)
    {
        CanMove = false;
        UI.EnableCanMoveMarker(CanMove);
        ThrusterAudio.Play();
        for (var index = 1; index < path.Count; index++)
        {
            var end = GridPosition.ToVector3(path[index]);

            while ((transform.position - end).sqrMagnitude > 0.01)
            {
                var step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, end, step);
                yield return new WaitForEndOfFrame();
            }
            Move(path[index]);
        }
        ThrusterAudio.Stop();
    }

    protected override void Serialize(SerializationInfo serializationInfo)
    {
        base.Serialize(serializationInfo);

        serializationInfo.SetValue(LivingEntitySerializationNames.MovementRange, MovementRange);
        serializationInfo.SetValue(LivingEntitySerializationNames.Side, (int)Side);
        serializationInfo.SetValue(LivingEntitySerializationNames.Structure, Structure);
        serializationInfo.SetValue(LivingEntitySerializationNames.Shield, Shield);
        serializationInfo.SetValue(LivingEntitySerializationNames.CanMove, CanMove);
        serializationInfo.SetValue(LivingEntitySerializationNames.CanFire, CanAttack);
        serializationInfo.SetValue(LivingEntitySerializationNames.CanBoard, CanBoard);
        serializationInfo.SetValue(LivingEntitySerializationNames.CanBeBoarded, CanBeBoarded);
    }

    protected override void Deserialize(SerializationInfo serializationInfo)
    {
        base.Deserialize(serializationInfo);

        MovementRange = serializationInfo.GetInt32(LivingEntitySerializationNames.MovementRange);
        Side = (Side)serializationInfo.GetInt32(LivingEntitySerializationNames.Side);
        Structure = serializationInfo.GetValue<Structure>(LivingEntitySerializationNames.Structure);
        Shield = serializationInfo.GetValue<Shield>(LivingEntitySerializationNames.Shield);
        CanMove = serializationInfo.GetBoolean(LivingEntitySerializationNames.CanMove);
        CanAttack = serializationInfo.GetBoolean(LivingEntitySerializationNames.CanFire);
        CanBoard = serializationInfo.GetBoolean(LivingEntitySerializationNames.CanBoard);
        CanBeBoarded = serializationInfo.GetBoolean(LivingEntitySerializationNames.CanBeBoarded);
    }

    private static IEnumerator FireSalvo(LivingEntity attacker, LivingEntity target, float salvoFlightTime)
    {
        yield return attacker.PlayFireAnimation();
        yield return new WaitForSeconds(salvoFlightTime);
        var hit = false;
        for (var i = 0; i < attacker.Weapon.SalveRounds; i++)
        {
            if (!(Random.Range(0f, 1f) <= attacker.Weapon.Accuracy))
            {
                continue;
            }
            var shieldDamage = Mathf.Min(
                target.Shield.HitPoints,
                attacker.Weapon.Damage * target.Shield.Absorption);
            target.Shield.HitPoints -= shieldDamage;
            target.Structure.HitPoints -= attacker.Weapon.Damage - shieldDamage;
            hit = true;
        }
        if (!hit)
        {
            yield break;
        }
        if (target.Structure.HitPoints <= 0)
        {
            yield return target.PlayShipExplosion();
            target.Kill();
        }
        else
        {
            yield return target.PlayHit();
        }
    }

    private IEnumerator PlayFireAnimation()
    {
        FireAudio.Play();
        while (FireAudio.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator PlayShipExplosion()
    {
        var prefab = ExplosionPrefabs[Random.Range(0, ExplosionPrefabs.Length)];
        Model.SetActive(false);
        UI.gameObject.SetActive(false);
        yield return PlayExplosionAnimation(prefab);
    }

    private IEnumerator PlayHit()
    {
        var prefab = HitPrefabs[Random.Range(0, HitPrefabs.Length)];
        yield return PlayExplosionAnimation(prefab);
        UI.UpdateStructure(Structure);
        UI.UpdateShield(Shield);
    }

    private IEnumerator PlayExplosionAnimation(GameObject prefab)
    {
        var explosion = Instantiate(prefab, transform);
        var audioSources = explosion.GetComponentsInChildren<AudioSource>();
        var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>();
        var lights = explosion.GetComponentsInChildren<Light>();

        foreach (var explosionAudio in audioSources)
        {
            explosionAudio.Play();
        }
        foreach (var explosionParticleSystem in particleSystems)
        {
            explosionParticleSystem.Play();
        }

        yield return new WaitForEndOfFrame();

        foreach (var explosionAudio in audioSources)
        {
            while (explosionAudio.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        foreach (var explosionParticleSystem in particleSystems)
        {
            while (explosionParticleSystem.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        foreach (var explosionLight in lights)
        {
            while (explosionLight.intensity > 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        Destroy(explosion);
    }

    protected class LivingEntitySerializationNames : EntitySerializationNames
    {
        public const string MovementRange = "MovementRange";
        public const string Side = "Side";
        public const string Shield = "Shield";
        public const string Structure = "Structure";
        public const string CanMove = "CanMove";
        public const string CanFire = "CanFire";
        public const string CanBoard = "CanBoard";
        public const string CanBeBoarded = "CanBeBoarded";
    }
}