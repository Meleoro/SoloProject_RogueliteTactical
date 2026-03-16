using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithMenu : MonoBehaviour
{
    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private Inventory currentInventory;
    private Loot currentLoot;

    [Header("References")]
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _inventoryParent;
    [SerializeField] private RectTransform _middlePosRectTr;
    [SerializeField] private RectTransform _bottomPosRectTr;
    [SerializeField] private Image _upgradedEquipmentImage;
    [SerializeField] private UpgradePanel _upgradeInventory;
    [SerializeField] private UpgradePanel _upgradeEquipment;
    [SerializeField] private DetailsPanel _detailsBase;
    [SerializeField] private DetailsPanel _detailsUpgrade;
    

    private void Start()
    {
        _upgradeEquipment.OnUpgrade += UpgradeEquipment;
        _upgradeInventory.OnUpgrade += UpgradeInventory;
    }


    #region Show / Hide

    public void Show()
    {
        OnStartTransition.Invoke();
        OnShow.Invoke();

        InventoriesManager.Instance.OnQuickChange += LoadInventory;
        UIManager.Instance.CoinUI.Show();

        LoadInventory(0);
        InventoriesManager.Instance.DisplayQuickChangeButtons(0);
        InventoriesManager.Instance.EnterBlacksmith();

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

        InventoriesManager.Instance.OnQuickChange -= LoadInventory;
        UIManager.Instance.CoinUI.Hide();

        _mainMetaMenu.Show(false);
        InventoriesManager.Instance.HideQuickChangeButtons();
        InventoriesManager.Instance.ExitBlacksmith();

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.DOMove(_bottomPosRectTr.position, 0.3f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    #endregion


    #region Equipment

    public void ClickEquipment(Loot clickedLoot)
    {
        if (clickedLoot.LootData.lootType != LootType.Equipment) return;

        currentLoot = clickedLoot;

        _upgradedEquipmentImage.enabled = true;
        _upgradedEquipmentImage.sprite = currentLoot.LootData.equipmentSprite;
        _upgradedEquipmentImage.SetNativeSize(); 

        _upgradeEquipment.SetUpgradeCost(clickedLoot.LootData.upgradeCost);
    }

    private void UpgradeEquipment()
    {
        if (!currentLoot) return;

        currentLoot.UpgradeLoot();
        _upgradeEquipment.SetUpgradeCost(currentLoot.LootData.upgradeCost);
    }

    #endregion


    #region Inventory

    private void LoadInventory(int index)
    {
        if (currentInventory is not null)
            currentInventory.RebootPosition();

        currentInventory = InventoriesManager.Instance.CurrentHeroesInventories[index];

        currentInventory.RectTransform.SetParent(_inventoryParent, true);
        currentInventory.RectTransform.localPosition = Vector3.zero;

        currentInventory.LootParent.SetParent(_inventoryParent, true);
        currentInventory.LootParent.localPosition = Vector3.zero;

        //currentInventory.RectTransform.localScale = Vector3.one;
        //currentInventory.LootParent.localScale = Vector3.one;

        _upgradeInventory.SetUpgradeCost(currentInventory.UpgradeCosts[currentInventory.CurrentLevel]);
    }

    private void UpgradeInventory()
    {
        currentInventory.UpgradeInventory();
    }

    #endregion
}
