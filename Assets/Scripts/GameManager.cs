using Serialization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private const string SaveFolder = "Saves";
    private const string SaveFile = "Game1";

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/GameManager");
                var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                _instance = obj.GetComponent<GameManager>();
            }
            return _instance;
        }
    }

    public bool SaveGameAvailable => File.Exists(Path.Combine(SaveFolder, SaveFile));

    public SerializationInfo LoadedSaveGame { get; private set; }

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void New()
    {
        LoadedSaveGame = null;
        SceneManager.LoadScene("game", LoadSceneMode.Single);
        SceneManager.LoadScene("mission1", LoadSceneMode.Additive);
    }

    public void Save(MissionManager missionManager)
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }
        using (var stream = new FileStream(Path.Combine(SaveFolder, SaveFile), FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
        {
            var serializationInfo = new SerializationInfo();
            serializationInfo.SetValue("Scene", SceneManager.GetActiveScene().name);
            serializationInfo.SetValue("Mission", missionManager);
            serializationInfo.Write(writer);
        }
        missionManager.LoadButton.interactable = true;
    }

    public void Load()
    {
        using (var stream = new FileStream(Path.Combine(SaveFolder, SaveFile), FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
        {
            LoadedSaveGame = new SerializationInfo(reader);
            SceneManager.LoadScene(LoadedSaveGame.GetString("Scene"));
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("mainMenu");
    }
}