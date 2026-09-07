using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using static GameData;

public class InventoriesManager : GenericSingletonClass<InventoriesManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private float openEffectDuration;
    [SerializeField] private float closeEffectDuration;
    public float slotSize;
    [SerializeField] private Sprite[] inventoryBackSprites;
    [SerializeField] private Loot lootPrefab;
    [SerializeField] private LootData[] allLootDatas;

    [Header("Actions")]
    public Action OnInventoryOpen;
    public Action OnInventoryClose;
    public Action OnInventoryChange;
    public Action OnEquipmentAdded;
    public Action<int> OnQuickChange;
    public Action<int> OnCoinsObtained;

    [Header("Private Infos")]
    private Inventory[] allHeroesInventories = new Inventory[4];
    private Inventory[] currentHeroesInventories = new Inventory[3];
    private bool[] enabledInventories = new bool[4];
    private List<InventorySlot> allSlots = new List<InventorySlot>();
    private List<Loot> allLoots = new List<Loot>();
    private int inventoryInstantiatedAmount = 0;
    private int currentCoins;
    private bool alreadyLoadedSave;
    private bool lockedInInventory;
    private bool isOpeningOrClosing;

    [Header("Public Infos")]
    public RectTransform MainLootParent { get { return _mainLootParent; } }
    public RectTransform FrontLootParent { get { return _frontLootPaent; } }
    public InventoryActionPanel InventoryActionPanel { get { return _inventoryActionsPanel; } }  
    public int CurrentCoins { get { return currentCoins; } }
    public Inventory[] CurrentHeroesInventories { get { return currentHeroesInventories; } }

    [Header("References Main")]
    [SerializeField] private RectTransform[] _lootParents;
    [SerializeField] private RectTransform _inventoriesParent;
    [SerializeField] private RectTransform _frontLootPaent;
    [SerializeField] private Image _backInventoriesImage;
    [SerializeField] private Image _backFadeImage;
    [SerializeField] private InventoryActionPanel _inventoryActionsPanel;
    [SerializeField] private CoinUI _topRightCointUI;
    [SerializeField] private CoinUI _hudBarCoinUI;
    [SerializeField] private Inventory _chestInventory;

    [Header("References Positions")]
    [SerializeField] private RectTransform[] _hiddenInventoryPositions;
    [SerializeField] private RectTransform[] _shownInventoryPositions;
    [SerializeField] private Inventory[] _possibleInventories;
    [SerializeField] private RectTransform _mainLootParent;
    [SerializeField] private RectTransform _hiddenPosition;
    [SerializeField] private RectTransform _shownPosition;
    [SerializeField] private RectTransform _leftHiddenPosition;
    [SerializeField] private RectTransform _leftShownPosition;

    [Header("References Quick Change")]
    [SerializeField] private QuickChangeButton[] _quickChangeButtons;
    


    #region Setup

    private void Start()
    {
        _backInventoriesImage.rectTransform.position = _hiddenPosition.position;

        for(int i = 0; i < _quickChangeButtons.Length; i++)
        {
            _quickChangeButtons[i].OnClick += ClickQuickChange;
            _quickChangeButtons[i].Hide();
        }

        SaveManager.Instance.AddSaveableObject(this);
    }

    public Inventory InitialiseInventory(Hero hero)
    {
        allHeroesInventories[hero.HeroIndex] = Instantiate(_possibleInventories[hero.HeroIndex], _inventoriesParent);
        allHeroesInventories[hero.HeroIndex].InitialiseInventory(hero);

        allSlots.AddRange(allHeroesInventories[hero.HeroIndex].GetSlots());

        StartCoroutine(ResetPosDelayCoroutine());
        StartCoroutine(SetupStartItemsCoroutine(allHeroesInventories[hero.HeroIndex], hero));

        return allHeroesInventories[hero.HeroIndex];
    }

    public void EnableInventory(int index)
    {
        if (enabledInventories[index]) return;

        currentHeroesInventories[inventoryInstantiatedAmount] = allHeroesInventories[index];
        enabledInventories[index] = true;

        currentHeroesInventories[inventoryInstantiatedAmount].SetupPosition(_hiddenInventoryPositions[inventoryInstantiatedAmount],
            _shownInventoryPositions[inventoryInstantiatedAmount], _lootParents[inventoryInstantiatedAmount]);

        _hiddenInventoryPositions[inventoryInstantiatedAmount].gameObject.SetActive(true);
        _shownInventoryPositions[inventoryInstantiatedAmount].gameObject.SetActive(true);

        _backInventoriesImage.sprite = inventoryBackSprites[inventoryInstantiatedAmount];
        _backInventoriesImage.SetNativeSize();

        inventoryInstantiatedAmount++;
    }

    public void DisableInventory(int index)
    {
        enabledInventories[index] = false;

        inventoryInstantiatedAmount--;

        _backInventoriesImage.sprite = inventoryBackSprites[inventoryInstantiatedAmount];
        _backInventoriesImage.SetNativeSize();
    }


    public void AddSlots(InventorySlot[] addedSlots)
    {
        allSlots.AddRange(addedSlots);
    }


    private IEnumerator ResetPosDelayCoroutine() 
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            currentHeroesInventories[i].RebootPosition(); 
        }
    }

    #endregion


    #region Main Functions

    // Opens the inventories and add a new item to place
    public void AddItemToInventories(Loot loot)
    {
        if (!VerifyCanOpenCloseInventory()) return;

        if (loot.LootData.lootType == LootType.Equipment) OnEquipmentAdded?.Invoke();

        ShowInventories();
    }

    // Called when a loot gets added to an inventory
    public void AddItem(Loot loot)
    {
        allLoots.Add(loot);

        StartCoroutine(CallObserverWithDelayCoroutine());
    }

    // Called when a loot gets removed to an inventory
    public void RemoveItem(Loot loot)
    {
        allLoots.Remove(loot);

        StartCoroutine(CallObserverWithDelayCoroutine());
    }

    // Called when an item is used
    public void DestroyItem(LootData item)
    {
        for (int i = 0; i < allLoots.Count; i++)
        {
            if (allLoots[i].LootData == item)
            {
                Loot loot = allLoots[i];
                loot.DestroyItem();
                break;
            }
        }
    }

    public List<Loot> GetInventoryLoot(int inventoryHeroIndex, bool isChest)
    {
        List<Loot> result = new List<Loot>();

        for (int i = allLoots.Count - 1; i >= 0; i--)
        {
            if (!allLoots[i].AssociatedHero && isChest)
            {
                result.Add(allLoots[i]);
                continue;
            }

            if (!allLoots[i].AssociatedHero) continue;
            if (allLoots[i].AssociatedHero.HeroIndex != inventoryHeroIndex) continue;

            result.Add(allLoots[i]);
        }

        return result;
    }

    // Called on death
    public void EmptyCurrentHeroesInventories()
    {
        for(int i = allLoots.Count - 1; i >= 0; i--)
        {
            if (!allLoots[i].AssociatedHero) continue;
            if (!currentHeroesInventories.Contains(allLoots[i].AssociatedHero.Inventory)) continue;

            allLoots[i].DestroyItem();
        }
    }

    #endregion

    
    #region Open / Close Functions

    public bool VerifyCanOpenCloseInventory(bool enterInventory = true)
    {
        if (lockedInInventory && !enterInventory) return false;
        if (isOpeningOrClosing) return false;

        for(int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            if (!currentHeroesInventories[i].GetCanOpenOrClose())
                return false;
        }

        return true;
    }

    public void LockInventory()
    {
        lockedInInventory = true;
    }

    public void UnlockInventory()
    {
        lockedInInventory = false;
    }

    public void ShowInventories()
    {
        OnInventoryOpen?.Invoke();

        _backInventoriesImage.rectTransform.DOLocalMove(_backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position),
            openEffectDuration).SetEase(Ease.OutBack);

        _backFadeImage.enabled = true;
        _backFadeImage.DOFade(0.7f, openEffectDuration).SetEase(Ease.OutSine);

        UIManager.Instance.CoinUI.Show();
        HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.StopControl();

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(currentHeroesInventories[i].OpenInventoryCoroutine(openEffectDuration));
        }

        StartCoroutine(ShowInventoriesCoroutine());
    }

    private IEnumerator ShowInventoriesCoroutine()
    {
        isOpeningOrClosing = true;

        yield return new WaitForSeconds(openEffectDuration);

        isOpeningOrClosing = false;
    }

    public void CloseInventories()
    {
        if (lockedInInventory || isOpeningOrClosing) return;

        if (UIManager.Instance.DraggedLoot != null)
        {
            UIManager.Instance.DraggedLoot.Drop();
        }

        OnInventoryClose?.Invoke();
        _inventoryActionsPanel.ClosePanel();

        UIManager.Instance.CoinUI.Hide();
        HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.RestartControl();

        _backInventoriesImage.rectTransform.DOLocalMove(_backInventoriesImage.rectTransform.parent.InverseTransformPoint(_hiddenPosition.position),
            openEffectDuration).SetEase(Ease.OutBack);

        _backFadeImage.DOFade(0f, openEffectDuration).SetEase(Ease.OutSine);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(currentHeroesInventories[i].CloseInventoryCoroutine(closeEffectDuration));
        }

        StartCoroutine(HideInventoriesCoroutine());
    }

    private IEnumerator HideInventoriesCoroutine()
    {
        isOpeningOrClosing = true;

        yield return new WaitForSeconds(closeEffectDuration);

        _backFadeImage.enabled = false;
        isOpeningOrClosing = false;
    }

    private IEnumerator CallObserverWithDelayCoroutine()
    {
        yield return new WaitForSeconds(0.05f);

        OnInventoryChange?.Invoke();
    }

    public IEnumerator OpenInventory(Inventory inventoryToOpen)
    {
        StartCoroutine(inventoryToOpen.OpenInventoryCoroutine(openEffectDuration, _leftHiddenPosition, _leftShownPosition));

        yield return new WaitForSeconds(openEffectDuration);
    }

    public IEnumerator CloseInventory(Inventory inventoryToClose)
    {
        StartCoroutine(inventoryToClose.CloseInventoryCoroutine(closeEffectDuration, _leftHiddenPosition));

        _inventoryActionsPanel.ClosePanel();

        yield return new WaitForSeconds(closeEffectDuration);
    }

    #endregion


    #region GameFeel

    public void HoverLoot(Loot hoveredLoot)
    {
        foreach(Loot loot in allLoots)
        {
            if (loot == hoveredLoot) continue;
            loot.OverlayOtherLoot();
        }
    }

    public void UnhoverLoot(Loot unhoveredLoot)
    {
        foreach (Loot loot in allLoots)
        {
            if (loot == unhoveredLoot) continue;
            loot.QuitOverlayOtherLoot();
        }
    }

    #endregion


    #region Coins

    public void AddCoins(int quantity)
    {
        OnCoinsObtained.Invoke(quantity);

        currentCoins += quantity;
        _topRightCointUI.ActualiseCoins(currentCoins);
        _hudBarCoinUI.ActualiseCoins(currentCoins);
    }

    public void RemoveCoins(int quantity)
    {
        currentCoins -= quantity;
        _topRightCointUI.ActualiseCoins(currentCoins);
        _hudBarCoinUI.ActualiseCoins(currentCoins);
    }

    #endregion


    #region Quick Change Buttons

    private bool quickChangeOnInventory;

    public void DisplayQuickChangeButtons(int inventoryIndex)
    {
        Inventory inventory = currentHeroesInventories[inventoryIndex];
        quickChangeOnInventory = true;

        for (int i = 0; i < _quickChangeButtons.Length; i++)
        {
            if (i >= currentHeroesInventories.Length || !currentHeroesInventories[i])     // If the hero isn't unlocked yet
            {
                _quickChangeButtons[i].ActualiseInfos(currentHeroesInventories[inventoryIndex].QuickChangeButtonsPositions[i], false);
                continue;
            }

            _quickChangeButtons[i].Show();
            _quickChangeButtons[i].ActualiseInfos(currentHeroesInventories[inventoryIndex].QuickChangeButtonsPositions[i], true);

            if (i == inventoryIndex) _quickChangeButtons[i].SelectHero();
            else _quickChangeButtons[i].UnselectHero();
        }
    }

    public void DisplayQuickChangeButtons(RectTransform[] positions)
    {
        quickChangeOnInventory = false;

        for (int i = 0; i < _quickChangeButtons.Length; i++)
        {
            if (i >= currentHeroesInventories.Length || !currentHeroesInventories[i])     // If the hero isn't unlocked yet
            {
                _quickChangeButtons[i].ActualiseInfos(positions[i], false);
                continue;
            }

            _quickChangeButtons[i].Show();
            _quickChangeButtons[i].ActualiseInfos(positions[i], true);

            if (i == 0) _quickChangeButtons[i].SelectHero();
        }
    }

    public void HideQuickChangeButtons()
    {
        for (int i = 0; i < currentHeroesInventories.Length; i++)
        {
            _quickChangeButtons[i].Hide();

            if (!currentHeroesInventories[i]) continue;

            _quickChangeButtons[i].ActualiseInfos(currentHeroesInventories[i].QuickChangeButtonsPositions[i], false);
        }
    }

    public void ClickQuickChange(int index)
    {
        Inventory inventory = currentHeroesInventories[index];

        for (int i = 0; i < _quickChangeButtons.Length; i++)
        {
            if (i >= currentHeroesInventories.Length || !currentHeroesInventories[i])     // If the hero isn't unlocked yet
            {
                _quickChangeButtons[i].ActualiseInfos(quickChangeOnInventory ? currentHeroesInventories[index].QuickChangeButtonsPositions[i] : null, false);
                continue;
            }

            _quickChangeButtons[i].ActualiseInfos(quickChangeOnInventory ? currentHeroesInventories[index].QuickChangeButtonsPositions[i] : null, true);

            if (i == index) _quickChangeButtons[i].SelectHero();
            else _quickChangeButtons[i].UnselectHero();
        }

        OnQuickChange?.Invoke(index);
    }

    #endregion


    #region Save

    public void LoadGame(GameData data)
    {
        // To avoid creating multiple times the same objects
        if (alreadyLoadedSave) return;
        alreadyLoadedSave = true;

        StartCoroutine(LoadGameWithDelayCoroutine(0.3f, data));
    }

    private IEnumerator LoadGameWithDelayCoroutine(float delay, GameData data)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < data.savedInventoryItems.Count; i++)
        {
            // First we pick the right inventory
            Inventory concernedInventory = null;
            if (data.savedInventoryItems[i].inventoryID == -1) concernedInventory = _chestInventory;
            else concernedInventory = allHeroesInventories[data.savedInventoryItems[i].inventoryID];

            if (concernedInventory is null) continue;

            // Next we find the item data
            LootData lootData = null;

            for (int j = 0; j < allLootDatas.Length; j++)
            {
                if (allLootDatas[j].lootName != data.savedInventoryItems[i].itemID) continue;

                lootData = allLootDatas[j];
                break;
            }

            if (lootData is null) continue;

            //Then we create the object and put it in the inventory
            Loot item = Instantiate(lootPrefab);
            item.Initialise(lootData);
            item.BecomeInventoryItem(false, data.savedInventoryItems[i].angle);

            item.PlaceInInventory(concernedInventory.GetOverlayedCoordinates(data.savedInventoryItems[i].coord,
                lootData.spaceTaken, data.savedInventoryItems[i].angle));
        }
    }

    public void SaveGame(ref GameData data)
    {
        List<ItemSaveStruct> itemsToSave = new List<ItemSaveStruct>();

        for (int i = 0; i < allLoots.Count; i++)
        {
            string itemID = allLoots[i].LootData.lootName;
            int inventoryID = -1;    // Chest Inventory Index
            if (allLoots[i].AssociatedHero is not null) inventoryID = allLoots[i].AssociatedHero.HeroIndex;
            Vector2Int coord = allLoots[i].BottomLeftSlot.SlotCoordinates;
            int angle = allLoots[i].CurrentAngle;
            bool isEquipped = allLoots[i].IsEquipped;

            itemsToSave.Add(new ItemSaveStruct(itemID, inventoryID, coord, angle, isEquipped));
        }

        data.savedInventoryItems = itemsToSave;
    }


    #endregion


    #region Others

    public int GetItemCount(LootData material)
    {
        int currentCount = 0;

        for(int i = 0; i < allHeroesInventories.Length; i++)
        {
            if (allHeroesInventories[i] == null) continue;
            currentCount += allHeroesInventories[i].GetItemCount(material);
        }

        currentCount += _chestInventory.GetItemCount(material);
        return currentCount;
    }

    private IEnumerator SetupStartItemsCoroutine(Inventory inventoryPrefab, Hero hero)
    {
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < hero.startLoot.Length; i++)
        {
            Loot item = Instantiate(lootPrefab);
            item.Initialise(hero.startLoot[i]);
            item.BecomeInventoryItem(false);

            inventoryPrefab.AutoPlaceItem(item);
        }
    }

    public InventorySlot VerifyIsOverlayingSlot(Vector3 raycastPos, Vector3 centerPos)
    {
        float maxDist = 75f;
        InventorySlot bestSlot = null;
        List<InventorySlot> possibleSlots = new List<InventorySlot>();

        for (int i = 0; i < allSlots.Count; i++)
        {
            float currentDist = Vector2.Distance(allSlots[i].RectTransform.InverseTransformPoint(raycastPos), Vector3.zero);
            if (currentDist < maxDist)
            {
                bestSlot = allSlots[i];
                maxDist = currentDist;
            }
        }

        return bestSlot;
    }


    public void EnterBlacksmith()
    {
        for(int i = 0; i < currentHeroesInventories.Count(); i++)
        {
            if (!currentHeroesInventories[i]) continue;

            currentHeroesInventories[i].EnterBlacksmith();
        }
    }

    public void ExitBlacksmith()
    {
        for (int i = 0; i < currentHeroesInventories.Count(); i++)
        {
            if (!currentHeroesInventories[i]) continue;

            currentHeroesInventories[i].ExitBlacksmith();
        }
    }

    #endregion
}
