using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PassivesManager 
{
    #region Main Functions

    public List<PassiveData> GetTriggeredPassives(PassiveTriggerType triggerType, Unit unit)
    {
        List<PassiveData> returnedList = new List<PassiveData>();

        if (!unit) return returnedList;

        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if (unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] == null) continue;
            if (passivesToCheck[i].passiveTriggerType != triggerType) continue;

            int pickedProba = Random.Range(0, 100);
            if (pickedProba >= passivesToCheck[i].passiveTriggerProba) continue;

            returnedList.Add(passivesToCheck[i]);
        }
        
        return returnedList;
    }

    public void ApplyPassives(PassiveData[] passivesToApply, Unit mainUnit, Unit secondaryUnit) 
    {
        for(int i = 0; i < passivesToApply.Length; i++)
        {
            ApplyPassive(passivesToApply[i].passiveEffects, mainUnit, secondaryUnit);
        }
    }

    private void ApplyPassive(PassiveEffect[] passivesEffects, Unit mainUnit, Unit secondaryUnit)
    {
        for(int i = 0; i < passivesEffects.Length; i++)
        {
            switch (passivesEffects[i].passiveEffectType)
            {
                case PassiveEffectType.ApplyOnSelfAlteration:
                    mainUnit.AddAlteration(passivesEffects[i].appliedAlteration, mainUnit);
                    break;

                case PassiveEffectType.ApplyOnOtherAlteration:
                    secondaryUnit.AddAlteration(passivesEffects[i].appliedAlteration, mainUnit);
                    break;

                case PassiveEffectType.UpgradeAlteration:
                    mainUnit.AddAlterationStrength(passivesEffects[i].appliedAlteration.alterationType,
                        passivesEffects[i].appliedAlteration.strength);
                    break;

                case PassiveEffectType.GainSkillPoint:
                    (mainUnit as Hero).AddSkillPoints(1);
                    break;

                case PassiveEffectType.MaxSkillPoints:
                    mainUnit.AddStatsModificators(0, 0, 0, 0, 0, (int)passivesEffects[i].additivePower);
                    break;

                case PassiveEffectType.UpgradeSummonHealth:
                    secondaryUnit.AddStatsModificators(secondaryUnit.CurrentMaxHealth + (int)passivesEffects[i].additivePower, secondaryUnit.CurrentStrength,
                        secondaryUnit.CurrentSpeed, secondaryUnit.CurrentLuck, 0, 0);
                    break;

                case PassiveEffectType.UpgradeSummonDamages:
                    secondaryUnit.ActualiseUnitInfos(secondaryUnit.CurrentMaxHealth, secondaryUnit.CurrentStrength + (int)passivesEffects[i].additivePower,
                        secondaryUnit.CurrentSpeed, secondaryUnit.CurrentLuck, 0, 0);
                    break;
            }
        }
    }

    #endregion


    #region Specific Cases

    public bool VerifyAlterationImmunities(AlterationType alterationType, Unit unit)
    {
        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if(unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();   

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] is null) continue;
            if (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.Always) continue;

            int pickedProba = Random.Range(0, 100);
            if (pickedProba >= passivesToCheck[i].passiveTriggerProba) continue;

            if (passivesToCheck[i].passiveEffects[0].passiveEffectType != PassiveEffectType.ImmuneAlteration) continue;
            if (passivesToCheck[i].passiveEffects[0].appliedAlteration.alterationType != alterationType) continue;

            return true;
        }

        return false;
    }

    public int GetPassiveMaxSkillPointUpgrade(Unit unit)
    {
        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if (unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] is null) continue;
            if (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.Always) continue;

            if (passivesToCheck[i].passiveEffects[0].passiveEffectType != PassiveEffectType.MaxSkillPoints) continue;

            return (int)passivesToCheck[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetPassiveCritChanceUpgrade(Unit unit, Unit attackedUnit)
    {
        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if (unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] is null) continue;
            if (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.OnAttack &&
                (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.OnAttackUnitWithAlt ||
                !attackedUnit.VerifyHasAlteration(passivesToCheck[i].neededAlteration.alterationType))) continue;

            if (passivesToCheck[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeCritChances) continue;

            return (int)passivesToCheck[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetGivePassiveAlterationUpgrade(Unit originUnit, AlterationData appliedAlteration)
    {
        PassiveData[] passivesToCheck = originUnit.EquippedPassives;
        // If hero, we add the equipment passives
        if (originUnit.GetType() == typeof(Hero)) passivesToCheck = originUnit.EquippedPassives.Concat(((Hero)originUnit).EquippedLootPassives).ToArray();

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] is null) continue;
            if (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.OnAlterationApplied) continue;

            if (passivesToCheck[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeAlteration ||
                appliedAlteration.alterationType != passivesToCheck[i].neededAlteration.alterationType) continue;

            return (int)passivesToCheck[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetReceivePassiveAlterationUpgrade(Unit unit, AlterationData appliedAlteration)
    {
        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if (unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] is null) continue;
            if (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.OnAlterationGained) continue;

            if (passivesToCheck[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeAlteration ||
                appliedAlteration.alterationType != passivesToCheck[i].neededAlteration.alterationType) continue;

            return (int)passivesToCheck[i].passiveEffects[0].additivePower;
        }

        return 0;
    }

    public int GetPassiveCritDamagesUpgrade(Unit unit, Unit attackedUnit)
    {
        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if (unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();

        return 0;
    }

    public int GetBonusDamages(Unit unit, Unit attackedUnit)
    {
        if (!unit) return 0;
        PassiveData[] passivesToCheck = unit.EquippedPassives;
        // If hero, we add the equipment passives
        if (unit.GetType() == typeof(Hero)) passivesToCheck = unit.EquippedPassives.Concat(((Hero)unit).EquippedLootPassives).ToArray();

        int bonus = 0;

        for (int i = 0; i < passivesToCheck.Length; i++)
        {
            if (passivesToCheck[i] is null) continue;
            if (passivesToCheck[i].passiveTriggerType != PassiveTriggerType.OnAttack) continue;

            if (passivesToCheck[i].passiveEffects[0].passiveEffectType != PassiveEffectType.UpgradeDamages ||
                !unit.VerifyHasAlteration(passivesToCheck[i].neededAlteration.alterationType)) continue;

            bonus += (int)passivesToCheck[i].passiveEffects[0].additivePower;
        }

        return bonus;
    }

    #endregion
}
