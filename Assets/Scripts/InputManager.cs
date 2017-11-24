using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public LayerMask GridLayerMask;
    public float ScrollSpeed = 1;
    public Button LoadButton;
    public GameObject UI;

    private MissionManager _missionManager;
    private Rect[] _uiRectangles;

    protected virtual void Start()
    {
        _missionManager = FindObjectOfType<MissionManager>();
        LoadButton.interactable = GameManager.Instance.SaveGameAvailable;

        var allRectangles = UI
            .GetComponentsInChildren<UIBehaviour>()
            .Select(ui => (RectTransform) ui.transform)
            .Select(rect =>
            {
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                return new Rect(corners[0], corners[2] - corners[0]);
            })
            .ToArray();

        var redundant = new List<Rect>();
        for (var i = 0; i < allRectangles.Length; i++)
        {
            for (var j = 0; j < allRectangles.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }
                if (allRectangles[i].Contains(allRectangles[j].min) && allRectangles[i].Contains(allRectangles[j].max))
                {
                    redundant.Add(allRectangles[j]);
                }
            }
        }

        _uiRectangles = allRectangles.Except(redundant).ToArray();
    }

    protected virtual void Update()
    {
        var inputScroll = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical"));

        _missionManager.MoveCamera(inputScroll * ScrollSpeed * Time.deltaTime);

        if (_missionManager.BlockUI || IsMouseOverUI())
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

    private bool IsMouseOverUI()
    {
        return _uiRectangles.Any(rect => rect.Contains(Input.mousePosition));
    }
}