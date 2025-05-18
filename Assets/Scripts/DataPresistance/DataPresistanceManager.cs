using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DataPresistanceManager : MonoBehaviour
{

    [Header("File Storage Config")]
    [SerializeField] private string filename;


    public static DataPresistanceManager Instance { get; private set; } // instance lahko dobis publicly, nastavis pa samo privately

    private GameData gamedata;
    
    private List<IDataPresistance> dataPresistanceObjects;
    private FileDataHandler fileDataHandler;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Presistance Manager in the scene");
        }
        Instance = this;

        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, filename);
        this.dataPresistanceObjects = FindAllDataPresistanceObjects();
    }

    private void Start()
    {

        //LoadGame();
    }


    /*private void OnApplicationQuit()
    {
        SaveGame();
    }*/

    public void newGame() 
    {
        this.gamedata = new GameData();  
    }

    public void LoadGame()
    {

        this.gamedata = fileDataHandler.Load();

        if (gamedata == null)
        {
            Debug.Log("No data found, init defaults");
            newGame();
        }

        foreach (IDataPresistance dataPresistanceObj in dataPresistanceObjects)
        {
            dataPresistanceObj.LoadData(gamedata);
        }

       //Debug.Log("Loaded hi score: "+gamedata.score);

    }

    public void SaveGame()
    {
        foreach (IDataPresistance dataPresistanceObj in dataPresistanceObjects)
        {
            dataPresistanceObj.SaveData(ref gamedata);
            //Debug.Log("Saved hi score: " + gamedata.score);
        }

        fileDataHandler.Save(gamedata);

    }

    private List<IDataPresistance> FindAllDataPresistanceObjects()
    { 
        IEnumerable<IDataPresistance> dataPersistanceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPresistance>();

        return new List<IDataPresistance>(dataPersistanceObjects);
    }

}
