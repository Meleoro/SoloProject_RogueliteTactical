using UnityEngine;

public class GameData 
{
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

    // Others
    public bool[] finishedTutorialSteps;
    public bool launchedTutorial;



    public GameData()
    {
        heroesLevel = new int[4];
        heroesUnlockedNodes = new bool[4 * 15];
        heroesCurrentSkillPoint = new int[4];
        heroesEquippedSkillIndexes = new string[4 * 6];
        heroesEquippedPassiveIndexes = new string[4 * 3];

        possessedRelicsIndexes = new bool[12 * 4];

        needEquippedInitialisation = true;
        campLevel = 0;

        finishedTutorialSteps = new bool[30];
    }
}
