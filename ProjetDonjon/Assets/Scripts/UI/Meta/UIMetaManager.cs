using UnityEngine;
using Utilities;

public enum CurrentMetaMenu
{
    Main,
    Expeditions,
    Chest,
    Collection,
    Shop,
    Smith
}

public class UIMetaManager : GenericSingletonClass<UIMetaManager>
{
    [Header("Private Infos")]
    private bool isInTransition;
    private bool isActive;
    private CurrentMetaMenu currentMetaMenu;

    [Header("Public Infos")]
    public bool IsInTransition { get { return isInTransition; } }
    public CollectionMenu CollectionMenu { get { return _collectionMenu; } }

    [Header("References")]
    [SerializeField] private Transform _globalParent;
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private ExpeditionsMenu _expeditionsMenu;
    [SerializeField] private ChestMenu _chestMenu;
    [SerializeField] private ShopMenu _shopMenu;
    [SerializeField] private CollectionMenu _collectionMenu;
    

    public override void Awake()
    {
        base.Awake();
        _globalParent.gameObject.SetActive(false);
    }

    private void Start()
    {
        _expeditionsMenu.OnStartTransition += StartTransition;
        _expeditionsMenu.OnEndTransition += EndTransition;
        _expeditionsMenu.OnShow += () => currentMetaMenu = CurrentMetaMenu.Expeditions;
        _expeditionsMenu.OnHide += () => currentMetaMenu = CurrentMetaMenu.Main;

        _chestMenu.OnStartTransition += StartTransition;
        _chestMenu.OnEndTransition += EndTransition;
        _chestMenu.OnShow += () => currentMetaMenu = CurrentMetaMenu.Chest;
        _chestMenu.OnHide += () => currentMetaMenu = CurrentMetaMenu.Main;

        _shopMenu.OnStartTransition += StartTransition;
        _shopMenu.OnEndTransition += EndTransition;
        _shopMenu.OnShow += () => currentMetaMenu = CurrentMetaMenu.Shop;
        _shopMenu.OnHide += () => currentMetaMenu = CurrentMetaMenu.Main;
    }


    private void Update()
    {
        if (isInTransition || !isActive) return;
        if (InputManager.wantsToReturn)
        {
            switch (currentMetaMenu)
            {
                case CurrentMetaMenu.Expeditions:
                    _expeditionsMenu.Hide(false);
                    break;

                case CurrentMetaMenu.Chest:
                    _chestMenu.Hide();
                    break;

                case CurrentMetaMenu.Collection:
                    _collectionMenu.Hide();
                    break;

                case CurrentMetaMenu.Shop:
                    _shopMenu.Hide();
                    break;
            }
        }
    }


    #region Show / Hide Meta Menu

    public void EnterMetaMenu()
    {
        currentMetaMenu = CurrentMetaMenu.Main;

        _globalParent.gameObject.SetActive(true);
        isActive = true;

        _collectionMenu.OnStartTransition += StartTransition;
        _collectionMenu.OnEndTransition += EndTransition;
        _collectionMenu.OnShow += () => currentMetaMenu = CurrentMetaMenu.Collection;
        _collectionMenu.OnHide += () => currentMetaMenu = CurrentMetaMenu.Main;

        _mainMetaMenu.ActualiseCampProgress();
        _mainMetaMenu.Show(true);

        _expeditionsMenu.Hide(true);

    }

    public void QuitMetaMenu()
    {
        _globalParent.gameObject.SetActive(false);
        isActive = false;

        _collectionMenu.OnStartTransition -= StartTransition;
        _collectionMenu.OnEndTransition -= EndTransition;
        _collectionMenu.OnShow -= () => currentMetaMenu = CurrentMetaMenu.Collection;
        _collectionMenu.OnHide -= () => currentMetaMenu = CurrentMetaMenu.Main;
    }

    #endregion


    #region Others

    public void StartTransition()
    {
        isInTransition = true;
    }

    public void EndTransition()
    {
        isInTransition = false;
    }

    #endregion
}
