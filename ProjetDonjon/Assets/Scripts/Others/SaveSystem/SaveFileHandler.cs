using System.IO;
using UnityEngine;

public class SaveFileHandler 
{
    private string dataDirPath = "";
    private string[] dataFileNames = new string[3];

    public SaveFileHandler(string dirPath, string[] fileName)
    {
        dataDirPath = dirPath;
        dataFileNames = fileName;
    }

    public GameData Load(int index)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileNames[index]);
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

    public void Save(GameData data, int index)
    {
        string fullPath =  Path.Combine(dataDirPath, dataFileNames[index]);

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
