using UnityEngine;
using UnityEngine.UI;

public class EntityUI : MonoBehaviour
{
    public GameObject TargetMarker;
    public GameObject BoardMarker;
    public GameObject CanMoveMarker;
    public GameObject CanFireMarker;
    public GameObject CanBoardMarker;
    public GameObject Health;
    public GameObject Shield;

    private Slider _healthSlider;
    private Slider _shieldSlider;

    protected virtual void Awake()
    {
        _healthSlider = Health.GetComponentInChildren<Slider>();
        _shieldSlider = Shield.GetComponentInChildren<Slider>();
        EnableTargetMarker(false);
        EnableBoardMarker(false);
        EnableCanFireMarker(false);
        EnableCanMoveMarker(false);
        EnableCanBoardMarker(false);
        DisableHealth();
        DisableShield();
    }

    public void EnableTargetMarker(bool active)
    {
        TargetMarker.SetActive(active);
    }

    public void EnableBoardMarker(bool active)
    {
        BoardMarker.SetActive(active);
    }

    public void EnableCanMoveMarker(bool active)
    {
        CanMoveMarker.SetActive(active);
    }

    public void EnableCanFireMarker(bool active)
    {
        CanFireMarker.SetActive(active);
    }

    public void EnableCanBoardMarker(bool active)
    {
        CanBoardMarker.SetActive(active);
    }

    public void UpdateStructure(Structure structure)
    {
        const int minValue = 0;
        const int maxValue = 10000;
        var value = Mathf.RoundToInt(structure.HitPoints / structure.MaxHitPoints * maxValue);
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

    public void UpdateShield(Shield shield)
    {
        const int minValue = 0;
        const int maxValue = 10000;
        var value = Mathf.RoundToInt(shield.HitPoints / shield.MaxHitPoints * maxValue);
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