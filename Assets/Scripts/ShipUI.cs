using UnityEngine;
using UnityEngine.UI;

public class ShipUI : MonoBehaviour
{
    public GameObject TargetMarker;
    public GameObject CanMoveMarker;
    public GameObject CanFireMarker;
    public GameObject Health;
    public GameObject Shield;

    private Slider _healthSlider;
    private Slider _shieldSlider;

    protected virtual void Awake()
    {
        _healthSlider = Health.GetComponentInChildren<Slider>();
        _shieldSlider = Shield.GetComponentInChildren<Slider>();
        DisableTargetMarker();
        DisableCanFireMarker();
        DisableCanMoveMarker();
        DisableHealth();
        DisableShield();
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
            _healthSlider.maxValue = maxValue;
            _healthSlider.minValue = minValue;
            _healthSlider.value = value;
        }
        else
        {
            DisableHealth();
        }
    }

    public void UpdateShield(float currentShield, float maxShield)
    {
        const int minValue = 0;
        const int maxValue = 10000;
        var value = Mathf.RoundToInt(currentShield / maxShield * maxValue);
        if (value > minValue && value < maxValue)
        {
            EnableShield();
            _shieldSlider.maxValue = maxValue;
            _shieldSlider.minValue = minValue;
            _shieldSlider.value = value;
        }
        else
        {
            DisableShield();
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

    private void EnableShield()
    {
        Shield.SetActive(true);
    }

    private void DisableShield()
    {
        Shield.SetActive(false);
    }
}