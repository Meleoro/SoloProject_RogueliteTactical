using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class Hero : Unit
{
    [Header("Hero Parameters")]
    [SerializeField] private HeroData heroData;
    [SerializeField] private SkillTreeData skillTreeData;
    [SerializeField] private int heroIndex;        // ID To access heroes save
    public LootData[] startLoot;
    [SerializeField] private int maxHeroLevel;
    [SerializeField] private int neededXPLevelZero;
    [SerializeField] private float neededXPMultiplicator;

    [Header("Actions")]
    public Action OnClickUnit;
    public Action OnDead;
    public Action OnRevive;
    public Action OnLevelUp;

    [Header("Public Infos")]
    public Loot[] EquippedLoot { get { return equippedLoot; } }
    public PassiveData[] EquippedLootPassives { get { return equippedLootPassives; } }
    public SkillData[] EquippedSkills { get { return equippedSkills; } }
    public Inventory Inventory { get { return inventory; } }
    public HeroData HeroData { get { return heroData; } }
    public SkillTreeData SkillTreeData { get { return skillTreeData; } }
    public HeroController Controller { get { return _controller; } }
    public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; OnHeroInfosChange?.Invoke(); } }  
    public int CurrentSkillPoints { get { return currentSkillPoints; } set { currentSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentMaxSkillPoints { get { return currentMaxSkillPoints; } set { currentMaxSkillPoints = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentActionPoints { get { return currentActionPoints; } }
    public int CurrentSkillTreePoints { get { return currentSkillTreePoints; } }
    public bool[] SkillTreeUnlockedNodes { get { return skillTreeUnlockedNodes; } }
    public int HeroIndex { get { return heroIndex; } }
    public bool IsDying { get { return isDying; } }
    public UnitUI UI { get { return _ui; } }
    public int CurrentXP { get { return currentXP; } set { currentXP = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentXPToReach { get { return XPToReach; } set { XPToReach = value; OnHeroInfosChange?.Invoke(); } }

    [Header("Private Infos")]
    private Loot[] equippedLoot = new Loot[6];
    private PassiveData[] equippedLootPassives = new PassiveData[6];
    private SkillData[] equippedSkills = new SkillData[6];
    private Inventory inventory;
    private bool isHidden;
    private bool isDying;
    private Transform currentDisplayedHeroTr;
    private int currentSkillPoints;
    private int currentMaxSkillPoints;
    private int currentActionPoints;
    private int currentXP;
    private int XPToReach;
    private int currentLevel;
    private int currentSkillTreePoints;
    private bool[] skillTreeUnlockedNodes;

    [Header("Hero References")]
    [SerializeField] private HeroController _controller;
    [SerializeField] private Transform _spriteRendererParent;
    [SerializeField] private Transform _shadowTransform;
    [SerializeField] private HeroInfoPanel _heroInfoPanel;
    [SerializeField] private Collider2D[] _colliders;


    public void Start()
    {
        unitData = heroData;
        _controller.EndAutoMoveAction += HideHero;

        CurrentLevel = HeroesManager.Instance.HeroesLevel[heroIndex];
        CurrentXPToReach = (int)(neededXPLevelZero * ((currentLevel + 1) * neededXPMultiplicator));

        skillTreeUnlockedNodes = new bool[maxHeroLevel];
        for(int i = 0; i < maxHeroLevel; i++)
        {
            if(HeroesManager.Instance.unlockAllSkills) skillTreeUnlockedNodes[i] = true;
            else skillTreeUnlockedNodes[i] = HeroesManager.Instance.HeroesUnlockedNodes[heroIndex * maxHeroLevel + i];
        }
        currentSkillTreePoints = HeroesManager.Instance.HeroesCurrentSkillTreePoint[heroIndex];

        InitialiseUnitInfos(heroData.baseHealth, heroData.baseStrength, heroData.baseSpeed, heroData.baseLuck, heroData.baseMovePoints);
        CurrentSkillPoints = HeroData.startSkillPoints;
        CurrentMaxSkillPoints = HeroData.maxSkillPoints;
    }

    private void Update()
    {
        if (isHidden)
        {
            transform.position = currentDisplayedHeroTr.transform.position;
        }
        else if (!isDead)
        {
            _controller.UpdateController();
        }

        // For Debug
        if (Input.GetKeyDown(KeyCode.R))
        {
            GainLevel();
        }
    }


    #region Hide / Show Functions

    public void ShowHero(bool isFromHeroSwitch = false)
    {
        isHidden = false;

        _spriteRenderer.transform.localPosition = Vector3.zero;
        _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);

        _shadowTransform.localScale = Vector3.one;

        _spriteRendererParent.gameObject.SetActive(true);
        _shadowTransform.gameObject.SetActive(true);

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i].isTrigger || isFromHeroSwitch)
                _colliders[i].enabled = true;
        }
    }

    public void HideHero()
    {
        isHidden = true;

        _spriteRenderer.transform.localPosition = Vector3.zero;
        _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);

        _spriteRendererParent.gameObject.SetActive(false);
        _shadowTransform.gameObject.SetActive(false);

        for (int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i].enabled = false;
        }
    }

    public void ActualiseCurrentDisplayedHero(Transform heroTr)
    {
        currentDisplayedHeroTr = heroTr;
    }

    #endregion


    #region Battle Functions

    public override void EnterBattle(BattleTile startTile)
    {
        base.EnterBattle(startTile);
        _controller.EnterBattle();
        _controller.AutoMove(startTile.transform.position);

        CurrentSkillPoints = heroData.startSkillPoints;

        if(CurrentHealth <= 0)
        {
            _animator.SetBool("IsDead", true);
        }
    }

    public override void ExitBattle(Hero currentHero)
    {
        base.ExitBattle(currentHero);
        _controller.ExitBattle();
        EndTurnOutline(false);

        currentShield = 0;

        RemoveAllAlterations();

        if (currentHero == this) return;
        StartCoroutine(_controller.AutoMoveCoroutineEndBattle(currentHero.transform));
    }

    public override void StartTurn()
    {
        base.StartTurn();

        if (currentSkillPoints < currentMaxSkillPoints)
        {
            CurrentSkillPoints += 1;
        }
        currentActionPoints = 2;
    }

    public void DoAction()
    {
        currentActionPoints--;
    }

    public override void TakeDamage(int damageAmount, Unit originUnit, bool isCrit)
    {
        if (isDead) return;

        base.TakeDamage(damageAmount, originUnit, isCrit);
        
        // For no damages when the hero explores in the tuto
        if (!BattleManager.Instance.IsInBattle && TutoManager.Instance.IsInTuto)
        {
            AudioManager.Instance.PlaySoundOneShot(2, 0);
            return;
        }

        OnHeroInfosChange?.Invoke();

        AudioManager.Instance.PlaySoundOneShot(2, 0);
    }

    protected override void Die()
    {
        isDead = true;
        _animator.SetBool("IsDead", true);
        _controller.StopControl();

        UnHoverUnit();
        HideOutline();

        if (BattleManager.Instance.IsInBattle)
        {
            _ui.HideUnitUI();
        }

        StartCoroutine(DieCoroutine());

        if (BattleManager.Instance.IsInBattle)
        {
            BattleManager.Instance.AddBattleEventToQueue(BattleEventType.HeroDeath);
        }
        AudioManager.Instance.PlaySoundOneShot(2, 9);
    }

    private IEnumerator DieCoroutine()
    {
        Time.timeScale = 0.4f;
        transform.DOShakePosition(1.0f, 0.75f, 45);
        isDying = true;

        yield return new WaitForSecondsRealtime(1);

        Time.timeScale = 1f;

        if (BattleManager.Instance.IsInBattle)
        {
            currentTile.UnitLeaveTile();
            BattleManager.Instance.RemoveUnit(this);
        }

        _shadowTransform.DOScale(Vector3.zero, 1.0f).SetEase(Ease.InBack);
        StartCoroutine(DisappearCoroutine(1.0f, false));
        OnDead.Invoke();

        isDying = false; 
    }

    public override void Heal(int healedAmount)
    {
        if(BattleManager.Instance.IsInBattle && isDead)
        {
            return;
        }

        base.Heal(healedAmount);

        if (isDead)
        {
            Revive();
        }
    }

    private void Revive()
    {
        OnRevive.Invoke();
        isDead = false;

        if (BattleManager.Instance.IsInBattle)
        {
            BattleManager.Instance.AddUnit(this);
        }

        _shadowTransform.localScale = Vector3.one;
        _spriteRenderer.material.SetFloat("_DitherProgress", 2.5f);

        HideHero();
    }

    protected override void ClickUnit()
    {
        base.ClickUnit();

        OnClickUnit?.Invoke();
    }

    public override void EndTurn(float delay)
    {
        base.EndTurn(delay);
    }

    #endregion


    #region Inventory Functions

    public void SetupInventory(Inventory inventory)
    {
        this.inventory = inventory;
    }

    public void AddEquipment(Loot loot, int equipSlotIndex)
    {
        if (equippedLoot[equipSlotIndex] != null)
        {
            equippedLoot[equipSlotIndex].Unequip();
        }

        equippedLoot[equipSlotIndex] = loot;
        equippedLootPassives[equipSlotIndex] = loot.LootData.appliedPassive;
    }

    public void RemoveEquipment(Loot removedLoot, int unequipSlotIndex)
    {
        if (equippedLoot[unequipSlotIndex] != null)
        {
            equippedLoot[unequipSlotIndex].Unequip();
        }

        equippedLoot[unequipSlotIndex] = null;
        equippedLootPassives[unequipSlotIndex] = null;
    }

    #endregion


    #region Level Functions

    public void GainXP(int quantity)
    {
        if(isDead)
        {
            CurrentXP += quantity;
            if (currentXP >= XPToReach) StartCoroutine(GainLevelCoroutine(0.9f));

            return;
        }

        CurrentXP += quantity;
        _ui.GainXP((float)currentXP / XPToReach);

        if (currentXP >= XPToReach) StartCoroutine(GainLevelCoroutine(0.9f));
        else AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 2, 20);   // To avoid overlap with the level up sound
    }
    
    private IEnumerator GainLevelCoroutine(float delay)
    {
        GainLevel();

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 2, 18);

        yield return new WaitForSeconds(delay);

        StartCoroutine(_ui.DoLevelUpEffectCoroutine(1f));

        _ui.ResetXPProgress();

        yield return new WaitForSeconds(0.25f);

        _ui.GainXP((float)currentXP / XPToReach);
    }

    private void GainLevel()
    {
        currentLevel++;
        currentSkillTreePoints++;

        OnLevelUp?.Invoke();

        currentXP = currentXP - XPToReach;
        XPToReach = (int)(XPToReach * neededXPMultiplicator);
    }

    #endregion


    #region Others

    public void StartExploration()
    {
        CurrentHealth = currentMaxHealth;
        isDead = false;

        _spriteRenderer.material.ULerpMaterialFloat(0.05f, 3.0f, "_DitherProgress");

        _controller.ExitBattle();

        _animator.SetBool("IsDead", false);
    }

    public override void ActualiseUnitInfos(int addedMaxHealth, int addedStrength, int addedSpeed, int addedLuck, int addedMovePoints, int addedSP)
    {
        int currentHealth = addedMaxHealth;
        int currentStrength = addedStrength;
        int currentSpeed = addedSpeed;
        int currentLuck = addedLuck;
        int currentMovePoints = heroData.baseMovePoints + addedMovePoints;
        int currentMaxSP = heroData.maxSkillPoints + addedSP;

        // EQUIPMENT
        for (int i = 0; i < EquippedLoot.Length; i++)
        {
            if (EquippedLoot[i] == null) continue;

            currentHealth += EquippedLoot[i].LootData.healthUpgrade;
            currentStrength += EquippedLoot[i].LootData.strengthUpgrade;
            currentSpeed += EquippedLoot[i].LootData.speedUpgrade;
            currentLuck += EquippedLoot[i].LootData.luckUpgrade;
            currentMovePoints += EquippedLoot[i].LootData.mpUpgrade;
            currentMaxSP += EquippedLoot[i].LootData.spUpgrade;
        }

        // SKILL TREE UPGRADES
        foreach(SkillTreeNodeData treeNode in GetAllPossessedNodes(skillTreeData, skillTreeUnlockedNodes)) 
        {
            if (treeNode.skillData is not null || treeNode.passiveData is not null) continue;

            switch (treeNode.statUpgradeType)
            {
                case StatType.Health:
                    currentHealth += treeNode.upgradeStrength;
                    break;

                case StatType.Strength:
                    currentStrength += treeNode.upgradeStrength;
                    break;

                case StatType.Speed:
                    currentSpeed += treeNode.upgradeStrength;
                    break;

                case StatType.Luck:
                    currentLuck += treeNode.upgradeStrength;
                    break;

                case StatType.MovePoints:
                    currentMovePoints += treeNode.upgradeStrength;
                    break;

                case StatType.SkillPoints:
                    currentMaxSP += treeNode.upgradeStrength;
                    break;
            }
        }

        this.currentMovePoints = currentMovePoints;
        this.currentMaxSkillPoints = currentMaxSP;

        base.ActualiseUnitInfos(currentHealth, currentStrength, currentSpeed, currentLuck, addedMovePoints, addedSP);

        UIManager.Instance.HeroInfosScreen.ActualiseInfoScreen(this, true);
        OnHeroInfosChange?.Invoke();
    }

    private SkillTreeNodeData[] GetAllPossessedNodes(SkillTreeData skillTreeData, bool[] unlockedNodes)
    {
        SkillTreeNodeData[] validNodes = new SkillTreeNodeData[unlockedNodes.Length];   // "2 +" because a hero starts with 2 skills at level 0 
        int i = 0;

        for (int x = 0; x < skillTreeData.skillTreeRows.Length; x++)
        {
            for (int y = 0; y < skillTreeData.skillTreeRows[x].rowNodes.Length; y++)
            {
                if (unlockedNodes[i])
                    validNodes[i] = skillTreeData.skillTreeRows[x].rowNodes[y];

                i++;
            }
        }

        return validNodes;
    }

    public override void AddStatsModificators(int addedMaxHealth, int addedStrength, int addedSpeed, int addedLuck, int addedMovePoints, int addedSP)
    {
        currentMovePoints += addedMovePoints;
        currentMaxSkillPoints += addedSP;

        base.AddStatsModificators(addedMaxHealth, addedStrength, addedSpeed, addedLuck, addedMovePoints, addedSP);
    }

    public void AddSkillPoints(int quantity)
    {
        if (quantity != -1)
            CurrentSkillPoints = Mathf.Clamp(CurrentSkillPoints + quantity, 0, currentMaxSkillPoints);
        else
            CurrentSkillPoints = currentMaxSkillPoints;
    }


    public void UseSkillPoints(int quantity)
    {
        int bonusCount = BattleManager.Instance.PassivesManager.GetConsumedSkillPointsBonus(this, null);
        quantity = Mathf.Clamp(quantity - bonusCount, 0, int.MaxValue);

        CurrentSkillPoints = Mathf.Clamp(CurrentSkillPoints - quantity, 0, currentMaxSkillPoints);
    }


    public void AddActionPoints(int quantity)
    {
        currentActionPoints += quantity;
    }

    public void SetupHeroInfosPanel(HeroInfoPanel heroInfoPanel)
    {
        _heroInfoPanel = heroInfoPanel;
        _heroInfoPanel.InitialisePanel(this);

        OnHeroInfosChange += ActualiseHeroInfoPanel;
    }

    private void ActualiseHeroInfoPanel()
    {
        _heroInfoPanel.ActualisePanel(this);
    }

    // Called when we buy a new skill to actualise hero infos
    public void ActualiseSkillTreeUnlockedNodes(bool[] unlockedNodes)
    {
        skillTreeUnlockedNodes = unlockedNodes;

        currentSkillTreePoints--;

        ActualiseUnitInfos(0, 0, 0, 0, 0, 0);
    }

    // Called when we modify the equipped skills
    public void ActualiseEquippedSkills(SkillData[] equippedSkills)
    {
        this.equippedSkills = equippedSkills;
    }

    // Called when we modify the equipped passives
    public void ActualiseEquippedPassives(PassiveData[] equippedPassives)
    {
        this.equippedPassives = equippedPassives;
        ActualiseUnitInfos(0, 0, 0, 0, 0, 0);
    }


    public void LoadEquippedInfos(string[] allSavedSkills, string[] allSavedPasssives)
    {
        string[] heroSavedSkills = new string[6];
        string[] heroSavedPassives = new string[3];

        for(int i = heroIndex * 6;  i < heroIndex * 6 + 6; i++)
        {
            heroSavedSkills[i - heroIndex * 6] = allSavedSkills[i];
        }

        for (int i = heroIndex * 3; i < heroIndex * 3 + 3; i++)
        {
            heroSavedPassives[i - heroIndex * 3] = allSavedPasssives[i];
        }

        LoadEquippedSkills(heroSavedSkills);
        LoadEquippedPassives(heroSavedPassives);
    }


    private void LoadEquippedSkills(string[] skillsNames)
    {
        for(int i = 0; i < skillsNames.Length; i++)
        {
            for(int j = 0; j < heroData.heroSkillPool.Length; j++)
            {
                if (heroData.heroSkillPool[j].skillName != skillsNames[i]) continue;

                equippedSkills[i] = heroData.heroSkillPool[j];  
                break;
            }
        }
    }

    private void LoadEquippedPassives(string[] passivesNames)
    {
        for (int i = 0; i < passivesNames.Length; i++)
        {
            for (int j = 0; j < heroData.heroPassivePool.Length; j++)
            {
                if (heroData.heroPassivePool[j].passiveName != passivesNames[i]) continue;

                equippedPassives[i] = heroData.heroPassivePool[j];
                break;
            }
        }
    }

    #endregion
}
