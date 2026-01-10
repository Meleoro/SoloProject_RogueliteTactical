using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorLootData", menuName = "Scriptable Objects/FloorLootData")]
public class FloorLootData : ScriptableObject
{
    [Header("Normal Chests")]
    [Range(0, 100)] public int[] probaPerTierChest = new int[4];
    public PossibleLootData[] chestPossibleLoots;
    public int minChestCoins;
    public int maxChestCoins;

    [Header("Battle Ends")]
    [Range(0, 100)] public int[] probaPerTierBattle = new int[4];
    public PossibleLootData[] battleEndPossibleLoots;
    public int minBattleCoins;
    public int maxBattleCoins;

    [Header("Challenge")]
    [Range(0, 100)] public int[] probaPerTierChallenge = new int[4];
    public PossibleLootData[] challengeEndPossibleLoots;
    public int minChallengeCoins;
    public int maxChallengeCoins;
}

[Serializable]
public class PossibleLootData
{
    [Range(0, 100)] public int probability;
    public LootData loot;
}
