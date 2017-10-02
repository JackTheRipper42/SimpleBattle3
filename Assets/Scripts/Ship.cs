﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : Entity
{
#pragma warning disable 649
    // ReSharper disable ConvertToConstant.Local
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [SerializeField] private int _movementRange = 4;
    [SerializeField] private int _fireRange = 1;
    [SerializeField] private Side _side;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _weaponDamage = 10f;
    [SerializeField] private float _health;
    [SerializeField] private AudioSource _fireAudio;
    [SerializeField] private AudioSource _thrusterAudio ;
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private GameObject[] _explosionPrefabs;
    [SerializeField] private GameObject[] _hitPrefabs;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    // ReSharper restore ConvertToConstant.Local
#pragma warning restore 649

    private ShipUI _shipUI;

    public bool CanMove { get; private set; }

    public bool CanFire { get; private set; }

    public int MovementRange => _movementRange;

    public int FireRange => _fireRange;

    public Side Side => _side;

    protected virtual void Awake()
    {
        var uiObject = Instantiate(_uiPrefab, transform);
        _shipUI = uiObject.GetComponent<ShipUI>();
    }

    protected override void Start()
    {
        base.Start();
        _health = _maxHealth;
        StartTurn();
    }

    public IEnumerator Attack(Ship target)
    {
        CanFire = false;
        _shipUI.DisableCanFireMarker();

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
        _shipUI.DisableCanMoveMarker();
        _thrusterAudio.Play();
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
        _thrusterAudio.Stop();
    }

    public void EnableTargetMarker()
    {
        _shipUI.EnableTargetMarker();
    }

    public void DisableTargetMarker()
    {
        _shipUI.DisableTargetMarker();
    }

    public void StartTurn()
    {
        CanFire = true;
        CanMove = true;
        if (GameManager.PlayerSide == _side)
        {
            _shipUI.EnableCanFireMarker();
            _shipUI.EnableCanMoveMarker();
        }
    }

    public override bool IsObstacle(Side side)
    {
        return side != _side;
    }

    private static IEnumerator Attack(Ship first, Ship second)
    {
        const float flightTime = 0.35f;
        var range = GridPosition.Distance(first.Position, second.Position);

        if (range <= first.FireRange)
        {
            yield return first.PlayFireAnimation();
            yield return new WaitForSeconds(flightTime);
            second._health -= first._weaponDamage;
            if (second._health <= 0)
            {
                yield return second.PlayShipExplosion();
                second.Kill();
            }
            else
            {
                yield return second.PlayHit();
            }
        }
        if (second._health > 0 && range <= second.FireRange)
        {
            yield return new WaitForSeconds(0.15f);
            yield return second.PlayFireAnimation();
            yield return new WaitForSeconds(flightTime);
            first._health -= second._weaponDamage;
            if (first._health < 0)
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
        _fireAudio.Play();
        while (_fireAudio.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator PlayShipExplosion()
    {
        var prefab = _explosionPrefabs[Random.Range(0, _explosionPrefabs.Length)];
        return PlayExplosionAnimation(prefab);
    }

    private IEnumerator PlayHit()
    {
        var prefab = _hitPrefabs[Random.Range(0, _hitPrefabs.Length)];
        return PlayExplosionAnimation(prefab);
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