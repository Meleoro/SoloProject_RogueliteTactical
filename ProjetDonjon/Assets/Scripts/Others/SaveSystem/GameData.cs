using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    [Serializable]
    public struct ItemSaveStruct
    {
        public string itemID;
        public int inventoryID;
        public Vector2Int coord;
        public int angle;
        public bool isEquipped;

        public ItemSaveStruct(string itemID, int inventoryID, Vector2Int coord, int angle, bool isEquipped)
        {
            this.itemID = itemID;
            this.inventoryID = inventoryID;
            this.coord = coord;
            this.angle = angle;
            this.isEquipped = isEquipped;
        }
    }

    public bool needEquippedInitialisation;

    // HEROES
    public int[] heroesLevel;
    public bool[] heroesUnlockedNodes;
    public int[] heroesCurrentSkillPoint;
    public string[] heroesEquippedSkillIndexes;
    public string[] heroesEquippedPassiveIndexes;

    // EQUIPMENT / RELICS / CAMP LEVEL
    public bool[] possessedRelicsIndexes;
    public int campLevel;
    public int shopLevel;
    public int chestLevel;

    // INVENTORIES
    public List<ItemSaveStruct> savedInventoryItems;
    public int gold;
    public int[] inventoryLevels;

    // Others
    public bool[] finishedTutorialSteps;
    public bool[] finishedAdditionalTutorialSteps;
    public bool launchedTutorial;



    public GameData()
    {
        heroesLevel = new int[10];    // 6 In case of updates for save compatibility
        heroesUnlockedNodes = new bool[10 * 20];
        heroesCurrentSkillPoint = new int[10];
        heroesEquippedSkillIndexes = new string[10 * 6];
        heroesEquippedPassiveIndexes = new string[10 * 3];

        possessedRelicsIndexes = new bool[12 * 10];

        needEquippedInitialisation = true;
        campLevel = 0;
        shopLevel = 0;
        chestLevel = 0;

        savedInventoryItems = new List<ItemSaveStruct>();
        gold = 0;
        inventoryLevels = new int[10];

        finishedTutorialSteps = new bool[20];
        finishedAdditionalTutorialSteps = new bool[30];
    }
}
