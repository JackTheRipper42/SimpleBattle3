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

    private SerializationInfo _loadedSaveGame;

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

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void New()
    {
        _loadedSaveGame = null;
        SceneManager.LoadScene("game", LoadSceneMode.Single);
        SceneManager.LoadScene("mission1", LoadSceneMode.Additive);
    }

    public void Save()
    {
        var missionSerializer = FindObjectOfType<MissionSerializer>();
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }
        using (var stream = new FileStream(Path.Combine(SaveFolder, SaveFile), FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
        {
            var serializationInfo = new SerializationInfo();
            serializationInfo.SetValue(GameManagerSerializationNames.Scene, SceneManager.GetActiveScene().name);
            serializationInfo.SetValue(GameManagerSerializationNames.Mission, missionSerializer);
            serializationInfo.Write(writer);
        }
    }

    public void Load()
    {
        using (var stream = new FileStream(Path.Combine(SaveFolder, SaveFile), FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
        {
            _loadedSaveGame = new SerializationInfo(reader);
            SceneManager.LoadScene(_loadedSaveGame.GetString(GameManagerSerializationNames.Scene));
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

    public void DeserializeMission(MissionSerializer missionSerializer)
    {
        if (_loadedSaveGame != null)
        {
            _loadedSaveGame.GetValue(GameManagerSerializationNames.Mission, serializationInfo => missionSerializer);
        }
    }
}