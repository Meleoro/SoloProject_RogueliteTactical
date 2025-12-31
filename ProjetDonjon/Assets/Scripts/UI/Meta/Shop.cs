using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;



public class ShopMenu : MonoBehaviour, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private Loot lootPrefab;

    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private int currentLevel;

    [Header("References")]
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _middlePosRectTr;
    [SerializeField] private RectTransform _leftPosRectTr;
    [SerializeField] private ChestMenu _chestMenu;
    [SerializeField] private ShopRow[] _shopRows;
    [SerializeField] private UpgradePanel _upgradePanel;


    private void Start()
    {
        SaveManager.Instance.AddSaveableObject(this);
        _upgradePanel.OnUpgrade += UpgradeShop;

        InitialiseShop();
    }

    private void InitialiseShop()
    {
        for(int i = 0; i < _shopRows.Length; i++)
        {
            _shopRows[i].InitialiseRow(i > currentLevel);

            for(int j = 0; j < _shopRows[i].RowItems.Length; j++)
            {
                _shopRows[i].RowItems[j].OnValidClick += TryToBuyItem;
            }
        }

        _upgradePanel.DisplayLevel(currentLevel);
    }


    public void TryToBuyItem(ShopItem baseItem, LootData data)
    {
        // If we do not have enough room in the chest
        if (!_chestMenu.ChestInventory.VerifyCanPlaceItem(data))
        {

            return;
        }

        Loot newLoot = Instantiate(lootPrefab);
        newLoot.Initialise(data);

        newLoot.BecomeInventoryItem(false);

        _chestMenu.ChestInventory.AutoPlaceItem(newLoot);

        InventoriesManager.Instance.RemoveCoins(data.value);
    }


    #region Camp Level

    private void ActualiseLevel()
    {
        for (int i = 0; i < _shopRows.Length; i++)
        {
            _shopRows[i].InitialiseRow(i > currentLevel);
        }

        _upgradePanel.DisplayLevel(currentLevel);
    }

    private void UpgradeShop()
    {
        currentLevel++;

        ActualiseLevel();
    }

    #endregion


    #region Show / Hide

    public void Show()
    {
        OnStartTransition.Invoke();
        OnShow.Invoke();

        UIManager.Instance.CoinUI.Show();

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        _mainRectTr.DOMove(_middlePosRectTr.position, 0.3f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    public void Hide()
    {
        OnStartTransition.Invoke();
        OnHide.Invoke();

        UIManager.Instance.CoinUI.Hide();
        _mainMetaMenu.Show(false);

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.DOMove(_leftPosRectTr.position, 0.3f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    #endregion


    #region Saves

    public void LoadGame(GameData data)
    {
        currentLevel = data.shopLevel;

        ActualiseLevel();
    }

    public void SaveGame(ref GameData data)
    {
        data.shopLevel = currentLevel;
    }

    #endregion
}
