using System.Collections;
using System.Collections.Generic;
using Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ship : Entity
{
    public int MovementRange = 4;
    public Side Side;
    public float SalvoFlightTime = 0.35f;
    public Weapon Weapon;
    public Shield Shield;
    public Structure Structure;
    public AudioSource FireAudio;
    public AudioSource ThrusterAudio;
    public GameObject UIPrefab;
    public GameObject[] ExplosionPrefabs;
    public GameObject[] HitPrefabs;
    public GameObject ShipModel;

    private bool _isLoading;

    public ShipUI UI { get; private set; }

    public bool CanMove { get; private set; }

    public bool CanFire { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        var uiObject = Instantiate(UIPrefab, transform);
        UI = uiObject.GetComponent<ShipUI>();
    }

    protected override void Start()
    {
        base.Start();
        if (_isLoading)
        {
            if (GameManager.PlayerSide == Side)
            {
                if (CanFire)
                {
                    UI.EnableCanFireMarker();
                }

                if (CanMove)
                {
                    UI.EnableCanMoveMarker();
                }
            }
            else
            {
                UI.DisableCanFireMarker();
                UI.DisableCanMoveMarker();

            }
            UI.UpdateStructure(Structure);
            UI.UpdateShield(Shield);

            _isLoading = false;
        }
        else
        {
            Structure.HitPoints = Structure.MaxHitPoints;
            Shield.HitPoints = Shield.MaxHitPoints;
            StartTurn();
        }
    }

    public IEnumerator Attack(Ship target)
    {
        CanFire = false;
        UI.DisableCanFireMarker();

        if (Random.Range(0, 10) < 7)
        {
            yield return Attack(this, target);
        }
        else
        {
            yield return Attack(target, this);
        }
    }

    public IEnumerator Move(IList<GridPosition> path, float speed)
    {
        CanMove = false;
        UI.DisableCanMoveMarker();
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

    public void StartTurn()
    {
        CanFire = true;
        CanMove = true;
        if (GameManager.PlayerSide == Side)
        {
            UI.EnableCanFireMarker();
            UI.EnableCanMoveMarker();
        }
    }

    public override bool IsObstacle(Side side)
    {
        return side != Side;
    }

    public override void Serialize(SerializationInfo serializationInfo)
    {
        base.Serialize(serializationInfo);

        serializationInfo.SetValue(ShipSerializationNames.MovementRange, MovementRange);
        serializationInfo.SetValue(ShipSerializationNames.Side, (int) Side);
        serializationInfo.SetValue(ShipSerializationNames.CanMove, CanMove);
        serializationInfo.SetValue(ShipSerializationNames.CanFire, CanFire);
        serializationInfo.SetValue(ShipSerializationNames.Structure, Structure);
        serializationInfo.SetValue(ShipSerializationNames.Shield, Shield);
        serializationInfo.SetValue(ShipSerializationNames.Weapon, Weapon);
    }

    public override void Deserialize(SerializationInfo serializationInfo)
    {
        _isLoading = true;

        base.Deserialize(serializationInfo);

        MovementRange = serializationInfo.GetInt32(ShipSerializationNames.MovementRange);
        Side = (Side)serializationInfo.GetInt32(ShipSerializationNames.Side);
        CanMove = serializationInfo.GetBoolean(ShipSerializationNames.CanMove);
        CanFire = serializationInfo.GetBoolean(ShipSerializationNames.CanFire);
        Structure = serializationInfo.GetValue<Structure>(ShipSerializationNames.Structure);
        Shield = serializationInfo.GetValue<Shield>(ShipSerializationNames.Shield);
        Weapon = serializationInfo.GetValue<Weapon>(ShipSerializationNames.Weapon);
    }

    private IEnumerator Attack(Ship first, Ship second)
    {
        var range = GridPosition.Distance(first.Position, second.Position);

        if (range <= first.Weapon.Range)
        {
            yield return FireSalvo(first, second);
        }
        if (second.Structure.HitPoints > 0 && range <= second.Weapon.Range)
        {
            yield return new WaitForSeconds(0.15f);
            yield return FireSalvo(second, first);
        }
    }

    private IEnumerator FireSalvo(Ship attacker, Ship target)
    {
        yield return attacker.PlayFireAnimation();
        yield return new WaitForSeconds(SalvoFlightTime);
        if (Random.Range(0f, 1f) > attacker.Weapon.Accuracy)
        {
            Debug.Log("miss");
            yield break;
        }
        var shieldDamage = Mathf.Min(target.Shield.HitPoints, attacker.Weapon.Damage * target.Shield.Absorption);
        target.Shield.HitPoints -= shieldDamage;
        target.Structure.HitPoints -= attacker.Weapon.Damage - shieldDamage;
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
        ShipModel.SetActive(false);
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

}