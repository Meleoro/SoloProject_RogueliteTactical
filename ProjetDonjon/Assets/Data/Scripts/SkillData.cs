using System;
using System.Collections.Generic;
using UnityEngine;


public enum SkillType
{
    Attack,
    HeavyAttack,
    Heal,
    Buff,
    Debuff,
    AttackBuff,
    AttackDebuff,
    Summon
}


public enum SkillPaternType
{
    Self,
    AOEPaternTiles,
    SkillArea,
    AdjacentTiles
}

public enum SkillEffectTargetType
{
    Self,
    Allies,
    Enemies,
    Empty
}

public enum SkillEffectType
{
    None,
    Damage,
    Heal,
    AddEnergy,
    Push,
    Summon,
    HealDebuffs,
    DestroySelf
}

[Serializable]
public struct SkillEffect
{
    public SkillEffectType skillEffectType;
    public SkillEffectTargetType skillEffectTargetType;
    public AlterationData appliedAlteration;

    public int additivePower;
    public float multipliedPower;
    public bool onlyOnCrit;

    public AIUnit summonPrefab;
}


[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Main")]
    public string skillName;
    [TextArea] public string skillDescription;
    public string animName;
    public int skillPointCost;
    public bool mustTargetAlly;
    public Sprite skillIcon;
    public Sprite skillHighlightIcon;
    public Sprite skillIntentIcon;     // For enemies intent icon in battles
    public AdditionalTooltipData[] additionalTooltipDatas;

    [Header("Effects")]
    public SkillType skillType;
    public SkillPaternType skillPaternType;
    public SkillEffect[] skillEffects;
    [Min(1)] public int attackCount;

    [Header("Visuals")]
    public bool onTargetVFX;
    public bool oneVFXPerTile;
    public bool rotateVFX;
    public bool mirrorHorizontalVFX;
    public bool mirrorVerticalVFX;
    public GameObject[] VFXs;
    public GameObject throwedObject;
    public GameObject leftVFX;
    public GameObject rightVFX;
    public GameObject upVFX;
    public GameObject downVFX;

    [Header("Paterns")]
    public bool useOrientatedAOE;
    public bool[] skillPatern = new bool[15 * 15];
    public bool[] skillAOEPatern = new bool[9 * 9];
    public bool[] skillAOEPaternLeft = new bool[9 * 9];
    public bool[] skillAOEPaternRight = new bool[9 * 9];
    public bool[] skillAOEPaternUp = new bool[9 * 9];
    public bool[] skillAOEPaternDown = new bool[9 * 9];
}
