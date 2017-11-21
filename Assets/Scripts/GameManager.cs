using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private const string SaveFolder = "Saves";
    private const string SaveFile = "Game1";

    private int _level;

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
        _level = 1;
    }

    public void New()
    {
        _level = 1;
        LoadLevel();
    }

    public void Save()
    {
        var missionSerializer = FindObjectOfType<MissionSerializer>();
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }
        using (var fileStream = new FileStream(Path.Combine(SaveFolder, SaveFile), FileMode.Create, FileAccess.Write))
        using (var memoryStream = new MemoryStream())
        {
            var serializationInfo = new SerializationInfo();
            serializationInfo.SetValue(GameManagerSerializationNames.Scene, SceneManager.GetActiveScene().name);
            serializationInfo.SetValue(GameManagerSerializationNames.MissionData, missionSerializer);
            serializationInfo.SetValue(GameManagerSerializationNames.Level, _level);

            SerializationInfo.Serializer.Write(memoryStream, serializationInfo);

            memoryStream.Position = 0;
            memoryStream.CopyTo(fileStream);
        }
    }

    public void Load()
    {
        using (var stream = new FileStream(Path.Combine(SaveFolder, SaveFile), FileMode.Open, FileAccess.Read))
        {
            var serializationInfo = SerializationInfo.Serializer.Read(stream);
            _level = serializationInfo.GetInt32(GameManagerSerializationNames.Level);
            StartCoroutine(Load(serializationInfo));
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

    public void Success()
    {
        _level++;
        LoadLevel();
    }

    public void GameOver()
    {
        _level = 1;
        MainMenu();
    }

    private void LoadLevel()
    {
        SceneManager.LoadScene("game", LoadSceneMode.Single);
        SceneManager.LoadScene($"mission{_level}", LoadSceneMode.Additive);
    }

    private static IEnumerator Load(SerializationInfo serializationInfo)
    {
        var missionManager = FindObjectOfType<MissionManager>();
        if (missionManager != null)
        {
            missionManager.ResetState();
            Destroy(missionManager.gameObject);
        }
        
        SceneManager.LoadSceneAsync(serializationInfo.GetString(GameManagerSerializationNames.Scene));
        MissionSerializer missionSerializer;
        do
        {
            yield return new WaitForEndOfFrame();
            missionSerializer = FindObjectOfType<MissionSerializer>();
        } while (missionSerializer == null);

        serializationInfo.GetValue(GameManagerSerializationNames.MissionData, info => missionSerializer);
    }

    protected class GameManagerSerializationNames
    {
        public const string Scene = "Scene";
        public const string MissionData = "MissionData";
        public const string Level = "Level";
    }
}