using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Private Infos")]
    private Inventory[] heroesInventories = new Inventory[3];
    private List<InventorySlot> allSlots = new List<InventorySlot>();
    private List<Loot> allLoots = new List<Loot>();
    private int inventoryInstantiatedAmount = 0;
    private int currentCoins;
    private bool alreadyLoadedSave;

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


    private void Start()
    {
        _backInventoriesImage.rectTransform.position = _hiddenPosition.position;
    }

    public Inventory InitialiseInventory(Inventory inventoryPrefab, int index, Hero hero)
    {
        heroesInventories[index] = Instantiate(inventoryPrefab, _inventoriesParent);
        heroesInventories[index].SetupPosition(_hiddenInventoryPositions[index], _shownInventoryPositions[index], _lootParents[index]);
        heroesInventories[index].InitialiseInventory(hero);

        _backInventoriesImage.sprite = inventoryBackSprites[index];
        _backInventoriesImage.SetNativeSize();

        allSlots.AddRange(heroesInventories[index].GetSlots());
        inventoryInstantiatedAmount++;

        _hiddenInventoryPositions[index].gameObject.SetActive(true);
        _shownInventoryPositions[index].gameObject.SetActive(true);

        StartCoroutine(ResetPosDelayCoroutine());

        StartCoroutine(SetupStartItemsCoroutine(heroesInventories[index], hero));

        return heroesInventories[index];
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
            heroesInventories[i].RebootPosition(); 
        }
    }

    // Opens the inventories and add a new item to place
    public void AddItemToInventories(Loot loot)
    {
        if (!VerifyCanOpenCloseInventory()) return;

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
        for(int i = 0; i < allLoots.Count; i++)
        {
            if(allLoots[i].LootData == item)
            {
                RemoveItem(allLoots[i]);
                allLoots[i].DestroyItem();
                break;
            }
        }
    }

    private IEnumerator CallObserverWithDelayCoroutine()
    {
        yield return new WaitForSeconds(0.05f);

        OnInventoryChange?.Invoke();
    }


    #region Open / Close Functions

    public bool VerifyCanOpenCloseInventory()
    {
        for(int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            if (!heroesInventories[i].GetCanOpenOrClose())
                return false;
        }

        return true;
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


    public async void OpenInventories()
    {
        OnInventoryOpen?.Invoke();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.75f, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position + Vector3.up), CurveType.EaseOutSin);
        _backFadeImage.UFadeImage(openEffectDuration, 0.5f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(heroesInventories[i].OpenInventoryCoroutine(openEffectDuration));
        }

        await Task.Delay((int)(openEffectDuration * 0.75f * 1000));

        _backInventoriesImage.rectTransform.UChangeLocalPosition(openEffectDuration * 0.25f, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_shownPosition.position), CurveType.EaseInSin);
    }

    public async void CloseInventories()
    {
        OnInventoryClose?.Invoke();

        _inventoryActionsPanel.ClosePanel();

        _backInventoriesImage.rectTransform.UChangeLocalPosition(closeEffectDuration, 
            _backInventoriesImage.rectTransform.parent.InverseTransformPoint(_hiddenPosition.position), CurveType.EaseOutCubic);
        _backFadeImage.UFadeImage(closeEffectDuration, 0f, CurveType.EaseOutCubic);

        await Task.Delay(20);

        for (int i = 0; i < inventoryInstantiatedAmount; i++)
        {
            StartCoroutine(heroesInventories[i].CloseInventoryCoroutine(closeEffectDuration));
        }
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

        StartCoroutine(LoadGameWithDelayCoroutine(0.25f, data));
    }

    private IEnumerator LoadGameWithDelayCoroutine(float delay, GameData data)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < data.savedInventoryItems.Count; i++)
        {
            // First we pick the right inventory
            Inventory concernedInventory = null;
            if (data.savedInventoryItems[i].inventoryID == -1) concernedInventory = _chestInventory;
            else concernedInventory = heroesInventories[data.savedInventoryItems[i].inventoryID];

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
            int inventoryID = -1;
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

        for(int i = 0; i < heroesInventories.Length; i++)
        {
            if (heroesInventories[i] == null) continue;
            currentCount += heroesInventories[i].GetItemCount(material);
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
