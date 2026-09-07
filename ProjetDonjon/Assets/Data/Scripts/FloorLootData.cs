using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorLootData", menuName = "Scriptable Objects/FloorLootData")]
public class FloorLootData : ScriptableObject
{
    [Header("Normal Chests")]
    [Range(0, 100)] public int[] probaPerTierChest = new int[4];
    public int minChestCoins;
    public int maxChestCoins;
    public int minLootAmountPerChest;
    public int maxLootAmountPerChest;

    [Header("Battle Ends")]
    [Range(0, 100)] public int[] probaPerTierBattle = new int[4];
    public int minBattleCoins;
    public int maxBattleCoins;
    public int minLootAmountPerBattle;
    public int maxLootAmountPerBattle;

    [Header("Challenge")]
    [Range(0, 100)] public int[] probaPerTierChallenge = new int[4];
    public int minChallengeCoins;
    public int maxChallengeCoins;
    public int minLootAmountPerChallenge;
    public int maxLootAmountPerChallenge;

    [Header("Trial")]
    [Range(0, 100)] public int[] probaPerTierTrial = new int[4];
    public int minTrialCoins;
    public int maxTrialCoins;
    public int minLootAmountPerTrial;
    public int maxLootAmountPerTrial;
}

[Serializable]
public class PossibleLootData
{
    [Range(0, 100)] public int probability;
    public LootData loot;
}
