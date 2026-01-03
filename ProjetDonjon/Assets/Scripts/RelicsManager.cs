using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using static CampLevelData;
using Random = UnityEngine.Random;

public class RelicsManager : GenericSingletonClass<RelicsManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private RelicData[] allRelics;
    [SerializeField] private Relic relicPrefab;
    [SerializeField] private RelicData debugRelicData;
    [SerializeField] private CampLevelData[] campLevels;
    [SerializeField] private int relicsPerArea;

    [Header("Debug")]
    [SerializeField] private bool unlockAllDebug;

    [Header("Actions")]
    public Action<RelicData, int> OnRelicObtained;
    public Action OnLevelUp;

    [Header("Private Infos")]
    private List<RelicData> currentAvailableRelics;
    private bool[] possessedRelicIndexes;
    private int currentCampRelicCount;
    private int currentCampLevel;
    private int currentPossessedRelicCount;

    [Header("Public Infos")]
    public bool[] PossessedRelicIndexes { get { return possessedRelicIndexes; } }
    public RelicData[] AllRelics { get { return allRelics; } }
    public int CurrentCampLevel { get { return currentCampLevel; } }
    public int CurrentCampRelicCount { get { return currentCampRelicCount; } } 
    public CampLevelData[] CampLevels { get { return campLevels; } }


    // For Debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            int index = Random.Range(0, currentAvailableRelics.Count);
            Relic newRelic = Instantiate(relicPrefab, HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].transform.position, 
                Quaternion.Euler(0, 0, 0));
            newRelic.Initialise(currentAvailableRelics[index]);
        }
    }


    #region Main Functions

    public void StartExploration(int enviroIndex)
    {
        int startIndex = enviroIndex * relicsPerArea;
        currentAvailableRelics = new List<RelicData>();

        for (int i = 0; i < relicsPerArea; i++)
        {
            if (i + startIndex >= allRelics.Length) return;
            if (PossessedRelicIndexes[i + startIndex]) continue;

            currentAvailableRelics.Add(allRelics[i]);
        }
    }

    public void ObtainNewRelic(RelicData data)
    {
        for(int i = 0; i < allRelics.Length; i++)
        {
            if (allRelics[i] != data) continue;

            OnRelicObtained.Invoke(data, i);
            PossessedRelicIndexes[i] = true;
            currentAvailableRelics.Remove(allRelics[i]);

            break;
        }
    }

    public int GetAreaAllRelicCount(int areaIndex)
    {
        return Mathf.Clamp(allRelics.Length - relicsPerArea * areaIndex, 0, relicsPerArea);
    }

    public int GetAreaCurrentRelicCount(int areaIndex)
    {
        int result = 0;

        for(int i = 0; i < relicsPerArea; i++)
        {
            int realIndex = relicsPerArea * areaIndex + i;
            if (allRelics.Length < realIndex) continue;

            if(possessedRelicIndexes[realIndex]) result++;
        }

        return result;  
    }

    #endregion


    #region Verify Relic Spawn 

    public RelicData TryRelicSpawn(RelicSpawnType eventType, int floorIndex, float probaModificator)
    {
        if (TutoManager.Instance.IsInTuto)
        {
            if (eventType != RelicSpawnType.BattleEndSpawn) return null;

            StartCoroutine(TutoManager.Instance.DisplayTutorialWithDelayCoroutine(9, 0.8f));
            return allRelics[0];
        }

        RelicData[] possibeRelics = GetAllRelicsOfSpawnType(eventType);

        return GetSpawnedRelic(possibeRelics, floorIndex);
    }

    private RelicData[] GetAllRelicsOfSpawnType(RelicSpawnType spawnType)
    {
        List<RelicData> result = new List<RelicData>();

        for(int i = 0; i < currentAvailableRelics.Count; i++)
        {
            if (currentAvailableRelics[i].spawnType != spawnType && currentAvailableRelics[i].spawnType != RelicSpawnType.Everywhere) continue;

            result.Add(currentAvailableRelics[i]);
        }

        return result.ToArray();
    }

    private RelicData GetSpawnedRelic(RelicData[] possibleRelics, int floorIndex)
    {
        for(int i = 0;i < possibleRelics.Length; i++)
        {
            int pickedProba = Random.Range(0, 100);
            if (possibleRelics[i].spawnProbaPerFloor[floorIndex] > pickedProba)
            {
                return possibleRelics[i];
            }
        }

        return null;
    }

    #endregion


    #region Save Interface

    public void LoadGame(GameData data)
    {
        possessedRelicIndexes = data.possessedRelicsIndexes;
        currentCampLevel = data.campLevel;

        if (unlockAllDebug)
        {
            currentCampLevel = campLevels.Length - 1;
        }

        ActualiseCampLevel();

        UIMetaManager.Instance.MainMetaMenu.LoadCampProgress();
        HeroesManager.Instance.Initialise();
    }

    public void SaveGame(ref GameData data)
    {
        data.possessedRelicsIndexes = possessedRelicIndexes;
        data.campLevel = currentCampLevel;
    }

    #endregion


    #region Camp Levels

    public void ActualiseCampLevel()
    {
        currentCampLevel = 0;
        currentCampRelicCount = 0;
        currentPossessedRelicCount = 0;

        for (int i = 0; i < PossessedRelicIndexes.Length; i++)
        {
            if (!PossessedRelicIndexes[i] && !unlockAllDebug) continue;
            currentPossessedRelicCount++;
            currentCampRelicCount++;
        }

        for (int i = 0; i < campLevels.Length; i++)
        {
            if (campLevels[i].neededRelicCount > currentPossessedRelicCount && !unlockAllDebug) continue;
            currentCampLevel = i + 1;
        }
    }

    public void CampLevelUp()
    {
        OnLevelUp?.Invoke();
    }

    public CampLevelData[] GetUnlockedCampLevels()
    {
        CampLevelData[] unlockedLevels = new CampLevelData[currentCampLevel];
        for (int i = 0; i < currentCampLevel; i++)
        {
            unlockedLevels[i] = campLevels[i];
        }

        return unlockedLevels;
    }

    public bool VerifyHasCampUpgrade(CampUnlockType unlockType, int additionalIndex = 0)
    {
        if (unlockAllDebug) return true;

        for(int i = 0; i < currentCampLevel; i++)
        {
            if (campLevels[i].unlockType != unlockType) continue;

            switch (unlockType)
            {
                case CampUnlockType.NewHero:
                    if(additionalIndex == campLevels[i].unlockedHeroIndex) return true;
                    break;

                case CampUnlockType.NewExpedition:
                    if (additionalIndex == campLevels[i].unlockedExpeditionIndex) return true;
                    break;

                case CampUnlockType.Shop:
                    return true;

                case CampUnlockType.Blacksmith:
                    return true;
            }
        }

        return false;
    }

    #endregion
}
