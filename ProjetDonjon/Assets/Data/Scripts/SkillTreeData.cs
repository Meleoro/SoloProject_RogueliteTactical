using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeData", menuName = "Scriptable Objects/SkillTreeData")]
public class SkillTreeData : ScriptableObject
{
    public SkillTreeRow[] skillTreeRows;
}

public enum StatType
{
    None,
    Health,
    Strength,
    Speed,
    Luck,
    MovePoints,
    SkillPoints
}


[Serializable]
public struct SkillTreeRow
{
    public SkillTreeNodeData[] rowNodes;
}

[Serializable]
public struct SkillTreeNodeData
{
    public SkillData skillData;
    public PassiveData passiveData;
    public Sprite icon;

    public StatType statUpgradeType;
    public int upgradeStrength;

    public int[] connectedNextNodeIndexes;
}
