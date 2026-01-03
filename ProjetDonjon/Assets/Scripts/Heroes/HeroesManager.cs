using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class HeroesManager : GenericSingletonClass<HeroesManager>, ISaveable
{
    [Header("Debug")]
    [SerializeField] private Hero[] debugHeroesPrefabs;
    [SerializeField] private Hero[] allHeroesPrefabs;
    [SerializeField] private Transform debugSpawnPos;
    public bool unlockAllSkills;

    [Header("Parameters")]
    [SerializeField] private XP xpPrefab;

    [Header("Public Infos")]
    public Hero[] Heroes { get { return heroes; } }
    public Hero[] AllHeroes { get { return allHeroes; } }
    public int CurrentHeroIndex { get { return currentHeroIndex; } }
    public InteractionManager InteractionManager { get { return _interactionManager; } }

    [Header("Private Infos")]
    private Hero[] heroes = new Hero[0];
    private Hero[] allHeroes = new Hero[4];
    private int currentHeroIndex;
    private int aliveHeroCount;
    private bool isInitialised;
    private bool isInvincible;
    private bool noControl;

    [Header("Public Saveable Infos")]
    public int[] HeroesLevel { get; private set; }
    public bool[] HeroesUnlockedNodes { get; private set; }
    public int[] HeroesCurrentSkillTreePoint { get; private set; }
    public string[] HeroesEquippedSkillNames { get; private set; }
    public string[] HeroesEquippedPassiveNames { get; private set; }

    [Header("References")]
    [SerializeField] private InteractionManager _interactionManager;
    [SerializeField] private InventoriesManager _inventoryManager;
    [SerializeField] private ProceduralGenerationManager _genProScript;
    [SerializeField] private SpriteLayererManager _spriteLayererManager;


    private void Start()
    {
        RelicsManager.Instance.OnLevelUp += ActualiseUnlockedHeroes;

        //if (!isInitialised) Initialise();
    }


    public void EndExploration()
    {
        SaveManager.Instance.SaveGame();

        StopControl();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !noControl)
        {
            SwitchHero();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeStairs();
        }
    }



    #region Control / Display

    public void RestartControl()
    {
        noControl = false;
        heroes[currentHeroIndex].Controller.RestartControl();
    }

    public void StopControl()
    {
        noControl = true;
        heroes[currentHeroIndex].Controller.StopControl();
    }

    private void SwitchHero()
    {
        currentHeroIndex = ++currentHeroIndex % heroes.Length;

        _spriteLayererManager.InitialiseAll();
        ActualiseDisplayedHero();
    }

    private void ActualiseDisplayedHero()
    {
        for (int i = 0; i < heroes.Length; i++)
        {
            heroes[i].ActualiseCurrentDisplayedHero(heroes[currentHeroIndex].transform);

            if (currentHeroIndex == i)
            {
                heroes[i].ShowHero(true);
            }
            else
            {
                heroes[i].HideHero();
            }
        }
    }

    #endregion


    #region Initialisation / Environment Related

    public void Initialise()
    {
        if(HeroesEquippedSkillNames is null)   // When we want to initialise the heroes, but the save still haven't been loaded
        {
            StartCoroutine(InitialiseWithDelayCoroutine());
            return;
        }
        if (isInitialised) 
        {
            ActualiseUnlockedHeroes();
            return;
        }
        isInitialised = true;

        allHeroes = new Hero[allHeroesPrefabs.Length];
        for (int i = 0; i < allHeroes.Length; i++)
        {
            allHeroes[i] = Instantiate(allHeroesPrefabs[i], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), transform);
            allHeroes[i].SetupInventory(_inventoryManager.InitialiseInventory(allHeroes[i].HeroData.heroInventoryPrefab, i, allHeroes[i]));
            allHeroes[i].LoadEquippedInfos(HeroesEquippedSkillNames, HeroesEquippedPassiveNames);

            allHeroes[i].OnDead += HeroDies;
            allHeroes[i].OnRevive += HeroRevives;
        }

        List<Hero> heroesList = new List<Hero>();
        for (int i = 0; i < allHeroes.Length; i++)
        {
            if (i > 3) return;
            if (i != 0 && !RelicsManager.Instance.VerifyHasCampUpgrade(CampLevelData.CampUnlockType.NewHero, i)) continue;

            _inventoryManager.EnableInventory(i);

            heroesList.Add(GetHeroObject(allHeroes[i]));
            heroesList[i].transform.position = Vector3.zero;
        }

        heroes = heroesList.ToArray();
    }

    public IEnumerator InitialiseWithDelayCoroutine()
    {
        yield return new WaitForSeconds(0.01f);

        Initialise();
    }

    public void ActualiseUnlockedHeroes()
    {
        List<Hero> heroesList = new List<Hero>();
        for (int i = 0; i < allHeroes.Length; i++)
        {
            if (i > 3) return;
            if (i != 0 && !RelicsManager.Instance.VerifyHasCampUpgrade(CampLevelData.CampUnlockType.NewHero, i)) continue;

            _inventoryManager.EnableInventory(i);

            heroesList.Add(GetHeroObject(allHeroes[i]));
            heroesList[i].transform.position = Vector3.zero;
        }

        heroes = heroesList.ToArray();
    }


    public void StartExploration(Vector2 spawnPos)
    {
        SaveManager.Instance.AddSaveableObject(this);

        if(!isInitialised) Initialise();  

        aliveHeroCount = heroes.Length;
        currentHeroIndex = 0;

        _interactionManager.ActualiseCurrentHeroTransform(heroes[0].transform);
        CameraManager.Instance.Initialise(heroes[0].transform);

        RestartControl();
        ActualiseDisplayedHero();

        UIManager.Instance.SetupHeroInfosPanel(heroes);

        for(int i = 0; i < aliveHeroCount; i++)
        {
            heroes[i].StartExploration();
        }
    }

    private Hero GetHeroObject(Hero wantedHero)
    {
        for(int i = 0; i < allHeroes.Length; i++)
        {
            if (!allHeroes[i]) continue;
            if (allHeroes[i].HeroData.unitName == wantedHero.HeroData.unitName) return allHeroes[i];
        }

        return null;
    }

    public void Teleport(Vector3 position)
    {
        if (!isInitialised) return;

        heroes[currentHeroIndex].transform.position = position;
    }

    public void TakeStairs()
    {
        StartCoroutine(TakeStairsCoroutine());
    }

    private IEnumerator TakeStairsCoroutine()
    {
        heroes[currentHeroIndex].Controller.AutoMove(heroes[currentHeroIndex].transform.position + Vector3.up * 2f);

        UIManager.Instance.FloorTransition.StartTransition(_genProScript.EnviroData, _genProScript.CurrentFloor + 1);

        yield return new WaitForSeconds(1);

        _genProScript.GenerateNextFloor();

        heroes[currentHeroIndex].Controller.StopAutoMove();
    }

    #endregion


    #region Battle Functions

    // Places the 3 heroes on te nearest tiles and initialises there battle behavior
    public void EnterBattle(List<BattleTile> possibleTiles)
    {
        for(int i = 0; i < heroes.Length; i++)
        {
            BattleTile pickedTile = possibleTiles[0];
            float bestDist = Mathf.Infinity;

            for (int j = 0; j < possibleTiles.Count; j++)
            {
                if (possibleTiles[j].UnitOnTile is not null) continue;
                if (possibleTiles[j].IsHole) continue;

                float currentDist = Vector2.Distance(possibleTiles[j].transform.position, heroes[i].transform.position);
                if (currentDist < bestDist)
                {
                    pickedTile = possibleTiles[j];
                    bestDist = currentDist;
                }
            }

            pickedTile.UnitEnterTile(heroes[i], false);
            heroes[i].ShowHero();
            heroes[i].EnterBattle(pickedTile);
        }
    }

    public void ExitBattle()
    {
        while (heroes[currentHeroIndex].CurrentHealth <= 0)
        {
            currentHeroIndex = (currentHeroIndex + 1) % heroes.Length;
        }

        for (int i = 0; i < heroes.Length; i++)
        {
            heroes[i].ExitBattle(heroes[currentHeroIndex]);
        }
    }


    public void SpawnXP(int quantity, Vector3 origin)
    {
        for(int i = 0; i < heroes.Length; i++)
        {
            for(int j = 0; j < quantity; j++)
            {
                XP xP = Instantiate(xpPrefab, origin, Quaternion.Euler(0, 0, 0));
                xP.Initialise(heroes[i], 1);
            }
        }
    }

    #endregion


    #region Damage / Death 


    public void TakeDamage(int damagesAmount)
    {
        if (isInvincible) return;

        heroes[currentHeroIndex].TakeDamage(damagesAmount, null);

        StartCoroutine(DamageInvincibilityCoroutine());
    }

    private IEnumerator DamageInvincibilityCoroutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(1);

        isInvincible = false;
    }

    private void HeroDies()
    {
        aliveHeroCount--;

        if(aliveHeroCount >= 0)
            SwitchDeadHero();
    }

    private void SwitchDeadHero()
    {
        while (heroes[currentHeroIndex].CurrentHealth <= 0)
        {
            SwitchHero();
        }
    }

    private void HeroRevives()
    {
        aliveHeroCount++;
    }

    #endregion


    #region Saves

    public void LoadGame(GameData data)
    {
        HeroesLevel = data.heroesLevel;
        HeroesUnlockedNodes = data.heroesUnlockedNodes;
        HeroesCurrentSkillTreePoint = data.heroesCurrentSkillPoint;
        HeroesEquippedSkillNames = data.heroesEquippedSkillIndexes;
        HeroesEquippedPassiveNames = data.heroesEquippedPassiveIndexes;

        if (data.needEquippedInitialisation)
        {
            data.needEquippedInitialisation = false;

            for(int i = 0; i < allHeroesPrefabs.Length; i++)
            {
                HeroesEquippedSkillNames[i * 6] = allHeroesPrefabs[i].HeroData.heroBaseSkills[0].skillName;
                HeroesEquippedSkillNames[i * 6 + 1] = allHeroesPrefabs[i].HeroData.heroBaseSkills[1].skillName;
            }
        }
    }

    public void SaveGame(ref GameData data)
    {
        for(int i = 0; i < heroes.Length; i++)
        {
            HeroesLevel[heroes[i].HeroIndex] = heroes[i].CurrentLevel;
            HeroesCurrentSkillTreePoint[heroes[i].HeroIndex] = heroes[i].CurrentSkillTreePoints;

            for (int j = 0; j < heroes[i].SkillTreeUnlockedNodes.Length; j++)
            {
                HeroesUnlockedNodes[heroes[i].HeroIndex * 15 + j] = heroes[i].SkillTreeUnlockedNodes[j];
            }

            for (int j = 0; j < heroes[i].EquippedSkills.Length; j++)
            {
                if (heroes[i].EquippedSkills[j] is null) HeroesEquippedSkillNames[heroes[i].HeroIndex * 6 + j] = "";
                else HeroesEquippedSkillNames[heroes[i].HeroIndex * 6 + j] = heroes[i].EquippedSkills[j].skillName;
            }

            for (int j = 0; j < heroes[i].EquippedPassives.Length; j++)
            {
                if (heroes[i].EquippedPassives[j] is null) HeroesEquippedPassiveNames[heroes[i].HeroIndex * 3 + j] = "";
                else HeroesEquippedPassiveNames[heroes[i].HeroIndex * 3 + j] = heroes[i].EquippedPassives[j].passiveName;
            }
        }

        data.heroesLevel = HeroesLevel;
        data.heroesUnlockedNodes = HeroesUnlockedNodes;
        data.heroesCurrentSkillPoint = HeroesCurrentSkillTreePoint;
        data.heroesEquippedSkillIndexes = HeroesEquippedSkillNames;
        data.heroesEquippedPassiveIndexes = HeroesEquippedPassiveNames;
    }

    #endregion
}
