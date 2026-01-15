using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveData", menuName = "Scriptable Objects/PassiveData")]
public class PassiveData : ScriptableObject
{
    [Header("Main")]
    public string passiveName;
    [TextArea] public string passiveDescription;
    public Sprite passiveIcon;
    public Sprite passiveHighlightIcon;
    public AdditionalTooltipData[] additionalTooltipDatas;

    [Header("Effect")]
    public PassiveTriggerType passiveTriggerType;
    public AlterationData neededAlteration;
    [Range(0, 100)] public float passiveTriggerProba;
    public PassiveEffect[] passiveEffects;
}

[Serializable]
public struct PassiveEffect
{
    public PassiveEffectType passiveEffectType;

    public AlterationData appliedAlteration;
    public float additivePower;
    public float multipliedPower;
}

public enum PassiveEffectType
{
    ApplyOnSelfAlteration,
    ApplyOnOtherAlteration,
    UpgradeAlteration,
    ImmuneAlteration,
    UpgradeCritDamages,
    UpgradeCritChances,
    MaxSkillPoints,
    GainSkillPoint,
    UpgradeSummonHealth,
    UpgradeSummonDamages,
    UpgradeDamages
}

public enum PassiveTriggerType
{
    Always,
    OnDamageReceived,
    OnAlterationGained,
    OnAlterationApplied,
    OnAttackUnitWithAlt,
    OnAttack,
    OnAttacked,
    OnCrit,
    OnKill,
    OnSummon,
    OnBattleStarted
}
