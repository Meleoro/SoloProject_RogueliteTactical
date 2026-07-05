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
    private bool isDoingUpgradeEffect;

    [Header("References")]
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _inventoryParent;
    [SerializeField] private RectTransform _middlePosRectTr;
    [SerializeField] private RectTransform _bottomPosRectTr;
    [SerializeField] private Image _upgradedEquipmentImage;
    [SerializeField] private Image _upgradedEquipmentImageFaded;
    [SerializeField] private UpgradePanel _upgradeInventory;
    [SerializeField] private UpgradePanel _upgradeEquipment;
    [SerializeField] private GenericDetailsPanel _detailsBase;
    [SerializeField] private GenericDetailsPanel _detailsUpgrade;
    [SerializeField] private RectTransform _detailsArrow;
    

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
        if (isDoingUpgradeEffect) return;

        OnStartTransition.Invoke();
        OnHide.Invoke();

        InventoriesManager.Instance.OnQuickChange -= LoadInventory;
        UIManager.Instance.CoinUI.Hide();

        _mainMetaMenu.Show(false);
        InventoriesManager.Instance.HideQuickChangeButtons();
        InventoriesManager.Instance.ExitBlacksmith();

        ClickEquipment(null);

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
        if(!clickedLoot)
        {
            _upgradedEquipmentImage.enabled = false;
            _upgradeEquipment.Hide();
            HideDetails();

            return;
        }

        if (isDoingUpgradeEffect) return;
        if (clickedLoot.LootData.lootType != LootType.Equipment) return;

        currentLoot = clickedLoot;

        _upgradedEquipmentImage.enabled = true;
        _upgradedEquipmentImage.sprite = currentLoot.LootData.equipmentSprite;
        _upgradedEquipmentImage.SetNativeSize(); 

        _upgradeEquipment.SetUpgradeCost(clickedLoot.LootData.upgradeCost);

        ShowDetails(clickedLoot.LootData, clickedLoot.LootData.upgradedVersion);
    }

    private void UpgradeEquipment()
    {
        if (isDoingUpgradeEffect) return;
        if (!currentLoot) return;

        currentLoot.UpgradeLoot();
        _upgradeEquipment.SetUpgradeCost(currentLoot.LootData.upgradeCost);
        ShowDetails(currentLoot.LootData, currentLoot.LootData.upgradedVersion);

        StartCoroutine(DoEquipmentUpgradeEffectsCoroutine());
    }

    private IEnumerator DoEquipmentUpgradeEffectsCoroutine()
    {
        isDoingUpgradeEffect = true;

        _upgradedEquipmentImage.rectTransform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutCirc);

        _upgradedEquipmentImageFaded.enabled = true;
        _upgradedEquipmentImageFaded.rectTransform.localScale = Vector3.one * 3.0f;
        _upgradedEquipmentImageFaded.sprite = currentLoot.LootData.equipmentSprite;
        _upgradedEquipmentImageFaded.rectTransform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.InCirc);

        _upgradedEquipmentImageFaded.color = new Color(1, 1, 1, 0);
        _upgradedEquipmentImageFaded.DOFade(0.4f, 0.3f);

        yield return new WaitForSeconds(0.3f);

        _upgradedEquipmentImage.rectTransform.DOScale(Vector3.one * 0.5f, 0.1f);
        _upgradedEquipmentImageFaded.enabled = false;

        yield return new WaitForSeconds(0.1f);

        _upgradedEquipmentImage.rectTransform.DOScale(Vector3.one, 0.2f);

        yield return new WaitForSeconds(0.2f);

        isDoingUpgradeEffect = false;
    }

    private void ShowDetails(LootData baseLoot, LootData upgradedLoot)
    {
        if (!upgradedLoot) 
        {
            HideDetails();
            return;
        }

        _detailsBase.LoadDetails(baseLoot, Vector3.zero, false, true);
        _detailsUpgrade.LoadDetails(upgradedLoot, Vector3.zero, false, true);
        _detailsArrow.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    private void HideDetails()
    {
        _detailsBase.HideDetails();
        _detailsUpgrade.HideDetails();
        _detailsArrow.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
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
        //currentInventory.LootParent.localScale = Vector3.one * 0.75f;

        _upgradeInventory.SetUpgradeCost(currentInventory.UpgradeCosts[currentInventory.CurrentLevel]);
    }

    private void UpgradeInventory()
    {
        currentInventory.UpgradeInventory();
        _upgradeInventory.SetUpgradeCost(currentInventory.UpgradeCosts[currentInventory.CurrentLevel]);
    }

    #endregion
}
