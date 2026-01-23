using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public enum UIState
{
    Nothing,
    Inventories,
    HeroesInfos,
    SkillTrees,
    Skills,
    Collection
}

public class UIManager : GenericSingletonClass<UIManager>
{
    [Header("Public Infos")]
    public Loot DraggedLoot { 
        get { return draggedLoot; } 
        set 
        { 
            draggedLoot = value;
            if (draggedLoot == null) OnStopDrag?.Invoke();
            else OnStartDrag?.Invoke();
        } 
    }
    public CoinUI CoinUI { get { return _coinUI; } }
    public Minimap Minimap { get { return _minimap; } }
    public FloorTransition FloorTransition { get { return _floorTransition; } }
    public HeroInfosScreen HeroInfosScreen { get { return _heroInfosScreen; } }
    public PlayerActionsMenu PlayerActionsMenu { get { return _playerActionsMenu; } }
    public HUDExploration HUDExploration { get { return _hudExploration; } }
    public UIState CurrentUIState { get { return currentState; } }

    [Header("Actions")]
    public Action OnStartDrag;
    public Action OnStopDrag;
    public Action OnExplorationStart;
    public Action OnExplorationEnd;

    [Header("Private Infos")]
    private UIState currentState;
    private Loot draggedLoot;
    private HeroInfoPanel[] currentHeroInfoPanels;
    private Animator currentHeroInfoPanelsAnimator;

    [Header("References")]
    [SerializeField] private InventoriesManager _inventoriesManager;
    [SerializeField] private HeroInfosScreen _heroInfosScreen;
    [SerializeField] private SkillTreeManager _skillTreesManager;
    [SerializeField] private SkillsMenu _skillsMenu;
    [SerializeField] private CollectionMenu _collectionMenu;
    [SerializeField] private CoinUI _coinUI;
    [SerializeField] private Minimap _minimap;
    [SerializeField] private FloorTransition _floorTransition;
    [SerializeField] private PlayerActionsMenu _playerActionsMenu;
    [SerializeField] private HUDExploration _hudExploration;

    [Header("References Hero Infos")]
    [SerializeField] private HeroInfoPanel[] _heroInfoPanels1H;
    [SerializeField] private Animator _heroInfoPanelsAnimator1H;
    [SerializeField] private HeroInfoPanel[] _heroInfoPanels2H;
    [SerializeField] private Animator _heroInfoPanelsAnimator2H;
    [SerializeField] private HeroInfoPanel[] _heroInfoPanels3H;
    [SerializeField] private Animator _heroInfoPanelsAnimator3H;



    private void Update()
    {
        if (BattleManager.Instance.IsInBattle || !GameManager.Instance.IsInExplo) return;

        switch (currentState)
        {
            case UIState.Nothing:
                if (InputManager.wantsToHeroInfo)
                {
                    if (!_heroInfosScreen.VerifyCanOpenOrCloseHeroInfos()) return;
                    StartCoroutine(_heroInfosScreen.OpenInfosScreenCoroutine());
                    currentState = UIState.HeroesInfos;
                }

                if (InputManager.wantsToInventory)
                {
                    if (!_inventoriesManager.VerifyCanOpenCloseInventory()) return;
                    _inventoriesManager.OpenInventories();
                }

                if (InputManager.wantsToSkillTree)
                {
                    _skillTreesManager.Show();
                }

                if (InputManager.wantsToSkills)
                {
                    _skillsMenu.Show(); 
                }
                break;


            case UIState.Inventories:
                if (InputManager.wantsToInventory || InputManager.wantsToReturn)
                {
                    if (!_inventoriesManager.VerifyCanOpenCloseInventory(false)) return;
                    _inventoriesManager.CloseInventories();
                    currentState = UIState.Nothing;
                }
                break;


            case UIState.HeroesInfos:

                if (InputManager.wantsToHeroInfo || InputManager.wantsToReturn)
                {
                    if (!_heroInfosScreen.VerifyCanOpenOrCloseHeroInfos()) return;
                    StartCoroutine(_heroInfosScreen.CloseInfosScreenCoroutine());
                    currentState = UIState.Nothing;
                }
                break;

            case UIState.SkillTrees:
                if (InputManager.wantsToSkillTree || InputManager.wantsToReturn) _skillTreesManager.Hide();
                
                break;

            case UIState.Skills:
                if (InputManager.wantsToSkills || InputManager.wantsToReturn) _skillsMenu.Hide();
                
                break;

            case UIState.Collection:
                if(InputManager.wantsToReturn) _collectionMenu.Hide();
                break;
        }
    }


    public void StartExploration(EnviroData enviroData, bool isTuto)
    {
        if (OnExplorationStart == null)  // If what need to be bind isn't
        { 
            StartCoroutine(DelayStartExploration(enviroData, isTuto));
            return;
        }

        OnExplorationStart.Invoke();

        _collectionMenu.OnShow += () => currentState = UIState.Collection;
        _collectionMenu.OnHide += () => currentState = UIState.Nothing;

        _inventoriesManager.OnInventoryOpen += OpenInventory;

        _heroInfosScreen.OnShow += () => currentState = UIState.HeroesInfos;
        _heroInfosScreen.OnHide += () => currentState = UIState.Nothing;

        _skillTreesManager.OnShow += () => currentState = UIState.SkillTrees;
        _skillTreesManager.OnHide += () => currentState = UIState.Nothing;

        _skillsMenu.OnShow += () => currentState = UIState.Skills;
        _skillsMenu.OnHide += () => currentState = UIState.Nothing;

        _floorTransition.StartTransition(enviroData, 0, isTuto);
    }

    private IEnumerator DelayStartExploration(EnviroData enviroData, bool isTuto)
    {
        yield return new WaitForSeconds(0.01f);

        StartExploration(enviroData, isTuto);
    }

    public void EndExploration()
    {
        OnExplorationEnd?.Invoke();

        _collectionMenu.OnShow -= () => currentState = UIState.Collection;
        _collectionMenu.OnHide -= () => currentState = UIState.Nothing;

        _inventoriesManager.OnInventoryOpen -= OpenInventory;

        _heroInfosScreen.OnShow -= () => currentState = UIState.HeroesInfos;
        _heroInfosScreen.OnHide -= () => currentState = UIState.Nothing;

        _skillTreesManager.OnShow -= () => currentState = UIState.SkillTrees;
        _skillTreesManager.OnHide -= () => currentState = UIState.Nothing;

        _skillsMenu.OnShow -= () => currentState = UIState.Skills;
        _skillsMenu.OnHide -= () => currentState = UIState.Nothing;
    }


    public void OpenInventory()
    {
        currentState = UIState.Inventories;
    }


    #region Battle UI

    public void SetupHeroInfosPanel(Hero[] heroes)
    {
        switch (HeroesManager.Instance.Heroes.Length)
        {
            case 1:
                currentHeroInfoPanels = _heroInfoPanels1H;
                currentHeroInfoPanelsAnimator = _heroInfoPanelsAnimator1H;
                break;

            case 2:
                currentHeroInfoPanels = _heroInfoPanels2H;
                currentHeroInfoPanelsAnimator = _heroInfoPanelsAnimator2H;
                break;

            case 3:
                currentHeroInfoPanels = _heroInfoPanels3H;
                currentHeroInfoPanelsAnimator = _heroInfoPanelsAnimator3H;
                break;
        }

        for (int i = 0; i < heroes.Length; i++)
        {
            currentHeroInfoPanels[i].InitialisePanel(heroes[i]);
            heroes[i].SetupHeroInfosPanel(currentHeroInfoPanels[i]);
        }
    }

    public void ShowHeroInfosPanels()
    {
        currentHeroInfoPanelsAnimator.SetBool("IsOpened", true);
    }

    public void HideHeroInfosPanels()
    {
        currentHeroInfoPanelsAnimator.SetBool("IsOpened", false);
    }

    #endregion
}
