using UnityEngine;

public class ShipUI : MonoBehaviour
{
    public GameObject TargetMarker;
    public GameObject CanMoveMarker;
    public GameObject CanFireMarker;

    protected virtual void Awake()
    {
        DisableTargetMarker();
        DisableCanFireMarker();
        DisableCanMoveMarker();
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
}