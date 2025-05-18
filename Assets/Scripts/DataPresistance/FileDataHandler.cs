using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
public class FileDataHandler
{
    private string dataDirPath = "";

    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        Debug.Log("Load function called");
        //Path.combine -> ker imajo razlicni os razlicne path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        GameData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream fs = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    { 
                        dataToLoad = sr.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            }
            catch (Exception e)
            { 
                Debug.LogError("Error while trying to read from the file: "+ fullPath +"\n"+ e);
            }

        }
        return loadedData;
    }

    public void Save(GameData data)
    { 
        //Path.combine -> ker imajo razlicni os razlicne path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            //createDirPath
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //serialize C# game data object v json

            string dataToStore = JsonUtility.ToJson(data, true);

            //write the file
            // raba using -> po koncu statementa using zapre tok -> just in case da ne pride do memory leaks
            using (FileStream stream = new FileStream(fullPath, FileMode.Create)) 
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.Write(dataToStore);
                }
            }

        }
        catch (Exception e)
        { 
            Debug.LogException(e);
        }
    }

}
