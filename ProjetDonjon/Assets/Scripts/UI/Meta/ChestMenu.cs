using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ChestMenu : MonoBehaviour, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private float distanceBetweenSlots;

    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Public Infos")]
    public Inventory ChestInventory { get {  return _chestInventory; } }

    [Header("Private Infos")]
    private Hero currentHero;
    private Inventory currentInventory;
    private int currentLevel;
    private int currentInventoryIndex;

    [Header("References")]
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _middlePosRectTr;
    [SerializeField] private RectTransform _rightPosRectTr;
    [SerializeField] private RectTransform _inventoryParent;
    [SerializeField] private RectTransform _chestSlotsParent;
    [SerializeField] private RectTransform _lootParent;
    [SerializeField] private RectTransform _upArrow;
    [SerializeField] private RectTransform _downArrow;
    [SerializeField] private Inventory _chestInventory;
    [SerializeField] private UpgradePanel _upgradePanel;


    private void Start()
    {
        _chestInventory.SetupPosition(null, null, _lootParent);
        _chestInventory.InitialiseInventory(null);

        _upgradePanel.OnUpgrade += UpgradeChestInventory;
        _upgradePanel.DisplayLevel(currentLevel);

        SaveManager.Instance.AddSaveableObject(this);
    }


    private void LoadInventory()
    {
        if (currentInventory is not null)
            currentInventory.RebootPosition();

        currentInventory = currentHero.Inventory;

        currentInventory.RectTransform.SetParent(_inventoryParent, true);
        currentInventory.RectTransform.localPosition = Vector3.zero;

        currentInventory.LootParent.SetParent(_inventoryParent, true);
        currentInventory.LootParent.localPosition = Vector3.zero;
    }


    private void UpgradeChestInventory()
    {
        currentLevel++;
        _chestInventory.UpdateInventoryLevel(currentLevel);

        _upgradePanel.DisplayLevel(currentLevel);
    }


    #region Change Inventory Arrows

    public void HoverArrow(bool upArrow)
    {
        if (upArrow)
        {
            _upArrow.DOScale(Vector3.one * 1.15f, 0.15f).SetEase(Ease.OutCubic);
        }
        else
        {
            _downArrow.DOScale(Vector3.one * 1.15f, 0.15f).SetEase(Ease.OutCubic);
        }
    }

    public void UnhoverArrow(bool upArrow)
    {
        if (upArrow)
        {
            _upArrow.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutCubic);
        }
        else
        {
            _downArrow.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutCubic);
        }
    }

    public void ClickArrow(bool upArrow)
    {
        if (upArrow)
        {
            currentInventoryIndex++;
            currentInventoryIndex = currentInventoryIndex % HeroesManager.Instance.AllHeroes.Length;
        }
        else
        {
            currentInventoryIndex--;
            if(currentInventoryIndex < 0) currentInventoryIndex = HeroesManager.Instance.AllHeroes.Length - 1;
        }

        currentHero = HeroesManager.Instance.AllHeroes[currentInventoryIndex]; 
        LoadInventory();
    }

    #endregion


    #region Show / Hide

    public void Show()
    {
        OnStartTransition.Invoke();
        OnShow.Invoke();

        currentInventoryIndex = 0;
        currentHero = HeroesManager.Instance.AllHeroes[currentInventoryIndex];
        LoadInventory();

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

        _mainMetaMenu.Show(false);
        InventoriesManager.Instance.InventoryActionPanel.ClosePanel();

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.DOMove(_rightPosRectTr.position, 0.3f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    #endregion


    #region Save

    public void LoadGame(GameData data)
    {
        currentLevel = data.chestLevel;

        _chestInventory.UpdateInventoryLevel(currentLevel);
        _upgradePanel.DisplayLevel(currentLevel);
    }

    public void SaveGame(ref GameData data)
    {
        data.chestLevel = currentLevel;
    }

    #endregion
}
