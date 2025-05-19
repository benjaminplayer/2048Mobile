using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void StartGame() 
    {
        SceneManager.LoadScene(1);
    }

    public void LoadLeaderboards()
    {
        SceneManager.LoadScene(2);
    }

    public void ExitApplication()
    { 
        Application.Quit();
    }

    public void ClearData()
    {
        string path = Application.persistentDataPath;
        
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                    Debug.Log($"Deleted: {file}");
                }
                catch (IOException e)
                {
                    Debug.LogError($"Could not delete file: {file} - {e}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Persistent data path does not exist.");
        }
    }

}
