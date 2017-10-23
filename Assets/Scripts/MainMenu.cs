using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button LoadButton;

    protected virtual void Start()
    {
        LoadButton.interactable = GameManager.Instance.SaveGameAvailable;
    }

    public void New()
    {
        GameManager.Instance.New();
    }

    public void Load()
    {
        GameManager.Instance.Load();
    }

    public void Exit()
    {
        GameManager.Instance.Exit();
    }
}