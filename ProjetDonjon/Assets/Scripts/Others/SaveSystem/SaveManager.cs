using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class SaveManager : GenericSingletonClass<SaveManager>
{
    [Header("Paramaters")]
    [SerializeField] private int fileCount;
    [SerializeField] private string[] fileNames;
    [SerializeField] private bool newGameOnStart;

    [Header("Public Infos")]
    public bool[] HasSaveFile { private set; get; }
    public int CurrentFileIndex { private set; get; }
    public GameData[] GameDatas { private set; get; }

    [Header("Private Infos")]
    private SaveFileHandler handler;
    private List<ISaveable> saveableObjects;


    #region Start Setup

    public override void Awake()
    {
        base.Awake();

        CurrentFileIndex = -1;

        handler = new SaveFileHandler(Application.persistentDataPath, fileNames);
        saveableObjects = new List<ISaveable>();

        //LoadGame();
        LoadSaves();
    }

    private void Start()
    {
        SetupSaveableObjects();

        if (newGameOnStart) NewGame(0);

        //LoadGame();
    }

    public void LoadSaves()
    {
        GameDatas = new GameData[fileCount];
        HasSaveFile = new bool[fileCount];

        for (int i = 0; i < fileCount; i++)
        {
            GameDatas[i] = handler.Load(i);
            HasSaveFile[i] = (GameDatas[i] != null);
        }
    }

    #endregion


    #region Load / Save / New

    public void NewGame(int index)
    {
        GameDatas[index] = new GameData();

        handler.Save(GameDatas[index], index);
    }

    public void SaveGame(int index)
    {
        if (index == -1) return;

        for (int i = 0; i < saveableObjects.Count; i++)
        {
            saveableObjects[i].SaveGame(ref GameDatas[index]);
        }

        handler.Save(GameDatas[index], index);
    }

    public void LoadGame(int index)
    {
        GameDatas[index] = handler.Load(index);

        if (GameDatas[index] == null)
        {
            NewGame(index);
        }

        for (int i = 0; i < saveableObjects.Count; i++)
        {
            saveableObjects[i].LoadGame(GameDatas[index]);
        }
    }

    #endregion


    #region Others 

    private void SetupSaveableObjects()
    {
        saveableObjects = FindObjectsByType<MonoBehaviour>().OfType<ISaveable>().ToList();
    }

    public void AddSaveableObject(ISaveable obj)
    {
        if (saveableObjects.Contains(obj)) return;
        saveableObjects.Add(obj);

        if (CurrentFileIndex == -1) return;
        
        obj.LoadGame(GameDatas[CurrentFileIndex]);
    }

    private void OnApplicationQuit()
    {
        SaveGame(CurrentFileIndex);
    }

    #endregion
}
