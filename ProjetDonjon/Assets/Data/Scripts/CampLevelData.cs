using UnityEngine;


[CreateAssetMenu(fileName = "CampLevelData", menuName = "Scriptable Objects/CampLevelData")]
public class CampLevelData : ScriptableObject
{
    public enum CampUnlockType
    {
        NewHero,
        NewExpedition,
        Shop,
        Blacksmith
    };

    [Header("Main Parameters")]
    public int neededRelicCount;
    public CampUnlockType unlockType;
    public string unlockName;
    [TextArea] public string unlockDescription;

    [Header("Hero Unlock")]
    public int unlockedHeroIndex;
    [TextArea] public string unlockAdditionalDescription;  // Because the text is in two parts
    public Sprite[] heroAnim;


    [Header("Expedition Unlock")]
    public int unlockedExpeditionIndex;
}
