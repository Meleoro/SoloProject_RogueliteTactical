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
    [SerializeField] public int neededXPLevelZero;
    [SerializeField] public float neededXPMultiplicator;

    [Header("Actions")]
    public Action OnClickUnit;
    public Action OnDead;
    public Action OnRevive;

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
    public UnitUI UI { get { return _ui; } }
    public int CurrentXP { get { return currentXP; } set { currentXP = value; OnHeroInfosChange?.Invoke(); } }
    public int CurrentXPToReach { get { return XPToReach; } set { XPToReach = value; OnHeroInfosChange?.Invoke(); } }


    [Header("Private Infos")]
    private Loot[] equippedLoot = new Loot[6];
    private PassiveData[] equippedLootPassives = new PassiveData[6];
    private SkillData[] equippedSkills = new SkillData[6];
    private Inventory inventory;
    private bool isHidden;
    private bool isDead;
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

        skillTreeUnlockedNodes = new bool[15];
        for(int i = 0; i < 15; i++)
        {
            if(HeroesManager.Instance.unlockAllSkills) skillTreeUnlockedNodes[i] = true;
            else skillTreeUnlockedNodes[i] = HeroesManager.Instance.HeroesUnlockedNodes[heroIndex * 15 + i];
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
        EndTurnOutline();

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

    public override void TakeDamage(int damageAmount, Unit originUnit)
    {
        if (isDead) return;

        base.TakeDamage(damageAmount, originUnit);
        
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
        _animator.SetBool("IsDead", true);

        AudioManager.Instance.PlaySoundOneShot(2, 9);
        StartCoroutine(DisappearCoroutine(1.0f, false));

        if (BattleManager.Instance.IsInBattle)
        {
            currentTile.UnitLeaveTile();
            BattleManager.Instance.RemoveUnit(this);
        }

        OnDead.Invoke();
        isDead = true;
    }

    public override void Heal(int healedAmount)
    {
        base.Heal(healedAmount);

        if (isDead)
        {
            OnRevive.Invoke();

            isDead = false;
            BattleManager.Instance.AddUnit(this);
        }
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
        equippedLoot[equipSlotIndex] = loot;
        equippedLootPassives[equipSlotIndex] = loot.LootData.appliedPassive;
    }

    public void RemoveEquipment(Loot removedLoot, int unequipSlotIndex)
    {
        equippedLoot[unequipSlotIndex] = null;
        equippedLootPassives[unequipSlotIndex] = null;
    }

    #endregion


    #region Level Functions

    public void GainXP(int quantity)
    {
        CurrentXP += quantity;
        _ui.GainXP((float)currentXP / XPToReach);

        if (currentXP >= XPToReach) StartCoroutine(GainLevelCoroutine(0.9f));
    }
    
    private IEnumerator GainLevelCoroutine(float delay)
    {
        GainLevel();

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

        currentXP = currentXP - XPToReach;
        XPToReach = (int)(XPToReach * neededXPMultiplicator);
    }

    #endregion


    #region Others

    public void StartExploration()
    {
        CurrentHealth = currentMaxHealth;
        _spriteRenderer.material.ULerpMaterialFloat(0.05f, 3.0f, "_DitherProgress");

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

        this.currentMovePoints = currentMovePoints;
        this.currentMaxSkillPoints = currentMaxSP;

        base.ActualiseUnitInfos(currentHealth, currentStrength, currentSpeed, currentLuck, addedMovePoints, addedSP);

        UIManager.Instance.HeroInfosScreen.ActualiseInfoScreen(this, true);
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
