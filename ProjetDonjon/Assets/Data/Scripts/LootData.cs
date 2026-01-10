using System;
using UnityEngine;

public enum LootType
{
    Treasure,
    Equipment,
    Consumable,
    Relic,
    Material
}

public enum EquipmentType
{
    Weapon,
    Headset,
    Breast,
    Legs,
    Ring
}

public enum ConsumableType
{
    Heal,
    Focus,
    Action,
    Strength,
    Move
}

public enum SpecialEquipmentEffectType
{
    None,
    Alteration,
    HitAlteration,
    AttackAlteration,
    CritUpgrade
}


[CreateAssetMenu(fileName = "LootData", menuName = "Scriptable Objects/LootData")]
public class LootData : ScriptableObject
{
    [Header("Main Infos")]
    public int lootID;
    public string lootName;
    public string lootDescription;
    public LootType lootType;
    public RarityType rarityType;
    public Sprite sprite;
    public int value;
    public SpaceTakenRow[] spaceTaken;

    [Header("Equipment Infos")]
    public EquipmentType equipmentType;
    public Sprite equipmentSprite;
    public int healthUpgrade;
    public int strengthUpgrade;
    public int speedUpgrade;
    public int luckUpgrade;
    public int mpUpgrade;
    public int spUpgrade;

    [Header("Equipment Special Effect Infos")]
    /*public SpecialEquipmentEffectType equipmentEffectType;
    public AlterationData equipmentEffectAlteration;
    public float equipmentEffectPower;*/
    public PassiveData appliedPassive;

    [Header("Consumable Infos")]
    public ConsumableType consumableType;
    public int consumablePower;
    public bool usableOutsideBattle;

    [Header("Others")]
    public AdditionalTooltipData[] additionalTooltipDatas;
}

[Serializable]
public struct SpaceTakenRow
{
    public bool[] row;
}
