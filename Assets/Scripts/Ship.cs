﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ship : Entity
{
    public int MovementRange = 4;
    public int FireRange = 1;
    public Side Side;
    public float MaxHealth = 100f;
    public float WeaponDamage = 10f;
    public float Health;
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
            UI.UpdateHealth(Health, MaxHealth);

            _isLoading = false;
        }
        else
        {
            Health = MaxHealth;
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

        serializationInfo.SetValue(ShipSerializationNames.MovementRange,MovementRange);
        serializationInfo.SetValue(ShipSerializationNames.FireRange, FireRange);
        serializationInfo.SetValue(ShipSerializationNames.Side, (int)Side);
        serializationInfo.SetValue(ShipSerializationNames.MaxHealth, MaxHealth);
        serializationInfo.SetValue(ShipSerializationNames.WeaponDamage, WeaponDamage);
        serializationInfo.SetValue(ShipSerializationNames.Health, Health);
        serializationInfo.SetValue(ShipSerializationNames.CanMove, CanMove);
        serializationInfo.SetValue(ShipSerializationNames.CanFire, CanFire);
    }

    public override void Deserialize(SerializationInfo serializationInfo)
    {
        _isLoading = true;

        base.Deserialize(serializationInfo);

        MovementRange = serializationInfo.GetInt32(ShipSerializationNames.MovementRange);
        FireRange = serializationInfo.GetInt32(ShipSerializationNames.FireRange);
        Side = (Side)serializationInfo.GetInt32(ShipSerializationNames.Side);
        MaxHealth = serializationInfo.GetSingle(ShipSerializationNames.MaxHealth);
        WeaponDamage = serializationInfo.GetSingle(ShipSerializationNames.WeaponDamage);
        Health = serializationInfo.GetSingle(ShipSerializationNames.Health);
        CanMove = serializationInfo.GetBoolean(ShipSerializationNames.CanMove);
        CanFire = serializationInfo.GetBoolean(ShipSerializationNames.CanFire);
    }

    private static IEnumerator Attack(Ship first, Ship second)
    {
        const float flightTime = 0.35f;
        var range = GridPosition.Distance(first.Position, second.Position);

        if (range <= first.FireRange)
        {
            yield return first.PlayFireAnimation();
            yield return new WaitForSeconds(flightTime);
            second.Health -= first.WeaponDamage;
            if (second.Health <= 0)
            {
                yield return second.PlayShipExplosion();
                second.Kill();
            }
            else
            {
                yield return second.PlayHit();
            }
        }
        if (second.Health > 0 && range <= second.FireRange)
        {
            yield return new WaitForSeconds(0.15f);
            yield return second.PlayFireAnimation();
            yield return new WaitForSeconds(flightTime);
            first.Health -= second.WeaponDamage;
            if (first.Health <= 0)
            {
                yield return first.PlayShipExplosion();
                first.Kill();
            }
            else
            {
                yield return first.PlayHit();
            }
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
        UI.UpdateHealth(Health, MaxHealth);
    }

    private IEnumerator PlayExplosionAnimation(GameObject prefab)
    {
        var explosion = Instantiate(prefab, transform);
        var audioSources = explosion.GetComponentsInChildren<AudioSource>();
        var particleSystems = explosion.GetComponentsInChildren<ParticleSystem>();
        var lights = explosion.GetComponentsInChildren<Light>();

        foreach (var explisionAudio in audioSources)
        {
            explisionAudio.Play();
        }
        foreach (var explosionParticleSystem in particleSystems)
        {
            explosionParticleSystem.Play();
        }

        yield return new WaitForEndOfFrame();

        foreach (var explisionAudio in audioSources)
        {
            while (explisionAudio.isPlaying)
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