using Unity.VisualScripting;
using UnityEngine;
using Utilities;

public class LootManager : GenericSingletonClass<LootManager>
{
    [Header("Parameters")]
    [SerializeField] private Loot lootPrefab;

    [Header("Private Infos")]
    private EnviroData currentEnviroData;
    private FloorLootData currentFloorLootData;


    #region Public Functions

    public void ActualiseInfos(EnviroData enviroData, FloorLootData floorData)
    {
        currentEnviroData = enviroData;
        currentFloorLootData = floorData;
    }


    public void SpawnLootChest(Vector2 spawnPosition)
    {
        SpawnLoot(currentFloorLootData.probaPerTierChest, spawnPosition);
    }

    public void SpawnLootBattleEnd(Vector2 spawnPosition)
    {
        SpawnLoot(currentFloorLootData.probaPerTierBattle, spawnPosition);
    }

    public void SpawnLootChallengeEnd(Vector2 spawnPosition)
    {
        SpawnLoot(currentFloorLootData.probaPerTierChallenge, spawnPosition);
    }

    #endregion


    #region Private Functions

    private void SpawnLoot(int[] probabilityPerRarity, Vector2 spawnPosition)
    {

        // First we get the loot pool according to the rarity spawned
        PossibleLootData[] possibleLoots = new PossibleLootData[0];

        int pickedRarity = GetSpawnedRarity(probabilityPerRarity);
        switch (pickedRarity)
        {
            case 0:
                possibleLoots = currentEnviroData.possibleCommonLoot;
                break;

            case 1:
                possibleLoots = currentEnviroData.possibleRareLoot;
                break;

            case 2:
                possibleLoots = currentEnviroData.possibleEpicLoot;
                break;

            case 3:
                possibleLoots = currentEnviroData.possibleMysticLoot;
                break;
        }

        // Next we get the loot spawned into that pool
        int pickedPercentage = Random.Range(0, 100);
        int currentSum = 0;
        for (int i = 0; i < possibleLoots.Length; i++)
        {
            currentSum += possibleLoots[i].probability;

            if (currentSum > pickedPercentage)
            {
                if (possibleLoots[i].loot is null) break;

                Loot newLoot = Instantiate(lootPrefab, spawnPosition, Quaternion.Euler(0, 0, 0));
                newLoot.Initialise(possibleLoots[i].loot);

                break;
            }
        }
    }

    private int GetSpawnedRarity(int[] probabilityPerRarity)
    {
        int pickedPercentage = Random.Range(0, 100);
        int currentSum = 0;

        for (int i = 0; i < probabilityPerRarity.Length; i++)
        {
            currentSum += probabilityPerRarity[i];

            if (currentSum > pickedPercentage)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion 
}
