using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public LayerMask GridLayerMask;
    public float ScrollSpeed = 1;
    public Button LoadButton;

    private MissionManager _missionManager;

    protected virtual void Start()
    {
        _missionManager = FindObjectOfType<MissionManager>();
        LoadButton.interactable = GameManager.Instance.SaveGameAvailable;
    }

    protected virtual void Update()
    {
        var inputScroll = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical"));

        _missionManager.MoveCamera(inputScroll * ScrollSpeed * Time.deltaTime);

        if (_missionManager.BlockUI)
        {
            return;
        }

        if (Input.mousePresent && Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
            {
                var gridPosition = GridPosition.FromVector3(hit.point);

                _missionManager.Select(gridPosition);
            }
        }
        if (Input.mousePresent && Input.GetMouseButtonUp(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
            {
                var gridPosition = GridPosition.FromVector3(hit.point);

                _missionManager.Interact(gridPosition);
            }
        }
    }

    public void EndTurn()
    {
        if (_missionManager.BlockUI)
        {
            return;
        }

        _missionManager.EndTurn();
    }

    public void Save()
    {
        if (_missionManager.BlockUI)
        {
            return;
        }

        GameManager.Instance.Save();
        LoadButton.interactable = true;
    }

    public void Load()
    {
        if (_missionManager.BlockUI)
        {
            return;
        }

        GameManager.Instance.Load();
    }

    public void MainMenu()
    {
        if (_missionManager.BlockUI)
        {
            return;
        }

        GameManager.Instance.MainMenu();
    }
}