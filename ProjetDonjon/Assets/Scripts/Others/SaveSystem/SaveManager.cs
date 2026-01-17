using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class SaveManager : GenericSingletonClass<SaveManager>
{
    [SerializeField] private string fileName;
    [SerializeField] private bool newGameOnStart;
    private SaveFileHandler handler;
    private GameData gameData;
    private List<ISaveable> saveableObjects;

    private void Start()
    {
        handler = new SaveFileHandler(Application.persistentDataPath, fileName);
        SetupSaveableObjects();

        if (newGameOnStart) NewGame();

        LoadGame();
    }

    [ContextMenu("New Game")]
    public void NewGame()
    {
        gameData = new GameData();

        handler.Save(gameData);
    }

    public void SaveGame()
    {
        for (int i = 0; i < saveableObjects.Count; i++)
        {
            saveableObjects[i].SaveGame(ref gameData);
        }

        handler.Save(gameData);
    }

    public void LoadGame()
    {
        gameData = handler.Load();

        if(gameData == null)
        {
            NewGame();
        }

        for(int i= 0; i < saveableObjects.Count; i++)
        {
            saveableObjects[i].LoadGame(gameData);
        }
    }



    private void SetupSaveableObjects()
    {
        saveableObjects = FindObjectsByType<MonoBehaviour>(0).OfType<ISaveable>().ToList();    
    }

    public void AddSaveableObject(ISaveable obj)
    {
        if (saveableObjects.Contains(obj)) return;
        saveableObjects.Add(obj);

        obj.LoadGame(gameData);
    }


    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
