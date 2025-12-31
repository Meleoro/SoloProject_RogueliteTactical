using System.IO;
using UnityEngine;

public class SaveFileHandler 
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public SaveFileHandler(string dirPath, string fileName)
    {
        dataDirPath = dirPath;
        dataFileName = fileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;

        if(File.Exists(fullPath))
        {
            string dataToLoad = "";

            using(FileStream stream = new FileStream(fullPath, FileMode.Open))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    dataToLoad = reader.ReadToEnd();
                }
            }

            loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
        }

        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath =  Path.Combine(dataDirPath, dataFileName);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        string dataToStore = JsonUtility.ToJson(data, true);

        using(FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            using(StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(dataToStore);
            }
        }
    }
}
