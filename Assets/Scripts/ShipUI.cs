using System;
using UnityEngine;
using UnityEngine.UI;

public class ShipUI : MonoBehaviour
{
    public GameObject TargetMarker;
    public GameObject CanMoveMarker;
    public GameObject CanFireMarker;
    public GameObject Health;

    private Slider _slider;

    protected virtual void Awake()
    {
        _slider = Health.GetComponentInChildren<Slider>();
        DisableTargetMarker();
        DisableCanFireMarker();
        DisableCanMoveMarker();
        DisableHealth();
    }

    public void EnableTargetMarker()
    {
        TargetMarker.SetActive(true);
    }

    public void DisableTargetMarker()
    {
        TargetMarker.SetActive(false);
    }

    public void EnableCanMoveMarker()
    {
        CanMoveMarker.SetActive(true);
    }

    public void DisableCanMoveMarker()
    {
        CanMoveMarker.SetActive(false);
    }

    public void EnableCanFireMarker()
    {
        CanFireMarker.SetActive(true);
    }

    public void DisableCanFireMarker()
    {
        CanFireMarker.SetActive(false);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        const int minValue = 0;
        const int maxValue = 10000;
        var value = Mathf.RoundToInt(currentHealth / maxHealth * maxValue);
        if (value > minValue && value < maxValue)
        {
            EnableHealth();
            _slider.maxValue = maxValue;
            _slider.minValue = minValue;
            _slider.value = value;
        }
        else
        {
            DisableHealth();
        }
    }

    private void EnableHealth()
    {
        Health.SetActive(true);
    }

    private void DisableHealth()
    {
        Health.SetActive(false);
    }
}