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

    [Header("Public Infos")]
    public RectTransform MainLootParent { get { return _mainLootParent; } }
    public InventoryActionPanel InventoryActionPanel { get { return _inventoryActionsPanel; } }  
    public int CurrentCoins { get { return currentCoins; } }

    [Header("References")]
    [SerializeField] private RectTransform[] _hiddenInventoryPositions;
    [SerializeField] private RectTransform[] _shownInventoryPositions;
    [SerializeField] private RectTransform _mainLootParent;
    [SerializeField] private RectTransform _hiddenPosition;
    [SerializeField] private RectTransform _shownPosition;
    [SerializeField] private RectTransform _leftHiddenPosition;
    [SerializeField] private RectTransform _leftShownPosition;
    [SerializeField] private RectTransform[] _lootParents;
    [SerializeField] private RectTransform _inventoriesParent;
    [SerializeField] private Image _backInventoriesImage;
    [SerializeField] private Image _backFadeImage;
    [SerializeField] private InventoryActionPanel _inventoryActionsPanel;
    [SerializeField] private CoinUI _coinUI;
    [SerializeField] private Inventory _chestInventory;


    #region Setup

    private void Start()
    {
        _backInventoriesImage.rectTransform.position = _hiddenPosition.position;

        //AddCoins(50);
    }

    public Inventory InitialiseInventory(Inventory inventoryPrefab, int index, Hero hero)
    {
        allHeroesInventories[index] = Instantiate(inventoryPrefab, _inventoriesParent);
        allHeroesInventories[index].InitialiseInventory(hero);

        allSlots.AddRange(allHeroesInventories[index].GetSlots());

        StartCoroutine(ResetPosDelayCoroutine());
        StartCoroutine(SetupStartItemsCoroutine(allHeroesInventories[index], hero));

        return allHeroesInventories[index];
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

        OpenInventories();
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

    private IEnumerator CallObserverWithDelayCoroutine()
    {
        yield return new WaitForSeconds(0.05f);

        OnInventoryChange?.Invoke();
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

    public async void OpenInventories()
    {
        OnInventoryOpen?.Invoke();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.75f, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position + Vector3.up), CurveType.EaseOutSin);
        _backFadeImage.UFadeImage(openEffectDuration, 0.5f, CurveType.EaseOutCubic);

        UIManager.Instance.CoinUI.Show();

        await Task.Delay(20);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(currentHeroesInventories[i].OpenInventoryCoroutine(openEffectDuration));
        }

        await Task.Delay((int)(openEffectDuration * 0.75f * 1000));

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.25f, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position), CurveType.EaseInSin);
    }

    public async void CloseInventories()
    {
        if (lockedInInventory) return;

        OnInventoryClose?.Invoke();

        _inventoryActionsPanel.ClosePanel();

        UIManager.Instance.CoinUI.Hide();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(closeEffectDuration, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_hiddenPosition.position), CurveType.EaseOutCubic);
        _backFadeImage.UFadeImage(closeEffectDuration, 0f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(currentHeroesInventories[i].CloseInventoryCoroutine(closeEffectDuration));
        }
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
        currentCoins += quantity;
        _coinUI.ActualiseCoins(currentCoins);
    }

    public void RemoveCoins(int quantity)
    {
        currentCoins -= quantity;
        _coinUI.ActualiseCoins(currentCoins);
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

    #endregion
}
