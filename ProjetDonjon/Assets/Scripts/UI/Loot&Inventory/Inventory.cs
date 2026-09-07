using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;


public class Inventory : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float distanceBetweenSlots;
    [SerializeField] private float inventoryScale;
    [SerializeField] private bool isChest;
    [SerializeField] private UpgradeCost[] upgradeCosts;

    [Header("Public Infos")]
    public RectTransform RectTransform { get { return _rectTransform; } }
    public RectTransform LootParent { get { return _lootParent; } }
    public Hero AssociatedHero { get { return associatedHero; } }
    public InventorySlot[,] InventorySlotsTab { get { return inventorySlotsTab; } }
    public bool IsChest { get { return isChest; } }
    public int CurrentLevel { get { return currentLevel; } }
    public RectTransform[] QuickChangeButtonsPositions { get { return _quickChangeButtonsPositions; } }
    public UpgradeCost[] UpgradeCosts { get { return upgradeCosts; } }

    [Header("Private Infos")]
    private InventorySlot[,] inventorySlotsTab = new InventorySlot[0, 0];
    private InventorySlot[] inventorySlots;
    private InventorySlot[] baseSlots;
    private RectTransform originalParent;
    private RectTransform originalLootParentParent;  
    private Hero associatedHero;
    private bool isMoving;
    private int currentLevel;

    [Header("References")]
    [SerializeField] private RectTransform _slotsParent;
    [SerializeField] private RectTransform _overlayedSlotsParent;
    [SerializeField] private RectTransform[] _quickChangeButtonsPositions;
    [SerializeField] private RectTransform[] _disabledSlots;
    [SerializeField] private InventorySlot[] _upgrade1Slots;
    [SerializeField] private InventorySlot[] _upgrade2Slots;
    private RectTransform _rectTransform;
    private RectTransform _hiddenRectTr;
    private RectTransform _showedRectTr;
    private RectTransform _lootParent;



    #region Initialisation Functions

    public void SetupPosition(RectTransform hiddenPosRectTr, RectTransform showedPosRectTr, RectTransform lootParent)
    {
        _hiddenRectTr = hiddenPosRectTr;
        _showedRectTr = showedPosRectTr;
        _lootParent = lootParent;

        _rectTransform = GetComponent<RectTransform>();

        if(_hiddenRectTr is not null)
            _rectTransform.position = _hiddenRectTr.position;

        originalParent = _rectTransform.parent as RectTransform;
        originalLootParentParent = _lootParent.parent as RectTransform;

        if(inventorySlots is null)
        {
            baseSlots = _slotsParent.GetComponentsInChildren<InventorySlot>();

            inventorySlots = baseSlots.Concat(_upgrade1Slots).Concat(_upgrade2Slots).ToArray();
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetupReferences(_lootParent, _slotsParent, _overlayedSlotsParent, this);
        }
    }

    public void InitialiseInventory(Hero hero)
    {
        for(int i = 0; i < _disabledSlots.Length; i++)
        {
            _disabledSlots[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < _upgrade1Slots.Length; i++)
        {
            _upgrade1Slots[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < _upgrade2Slots.Length; i++)
        {
            _upgrade2Slots[i].gameObject.SetActive(false);
        }


        associatedHero = hero;
        baseSlots = _slotsParent.GetComponentsInChildren<InventorySlot>(false);

        inventorySlots = baseSlots.Concat(_upgrade1Slots).Concat(_upgrade2Slots).ToArray();
        
        SetupSlots();

        if (isChest) InventoriesManager.Instance.AddSlots(inventorySlots);

        Vector2Int inventoryDimensions = GetInventoryDimensions();
        inventorySlotsTab = new InventorySlot[inventoryDimensions.x + 1, inventoryDimensions.y + 1];

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlotsTab[inventorySlots[i].SlotCoordinates.x, inventorySlots[i].SlotCoordinates.y] = inventorySlots[i];
        }

        for (int i = 0; i < baseSlots.Length; i++)
        {
            baseSlots[i].SetIsAvailable(true);
        }

        // We disable the upgrades
        for (int i = 0; i < _upgrade1Slots.Length; i++)
        {
            _upgrade1Slots[i].SetIsAvailable(false);
        }

        for (int i = 0; i < _upgrade2Slots.Length; i++)
        {
            _upgrade2Slots[i].SetIsAvailable(false);
        }
    }


    private void SetupSlots()
    {
        Vector2 bottomLeftPosition = inventorySlots[0].RectTransform.localPosition;

        // Neighbors + Find bottom left location
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetupNeighbors(inventorySlots, distanceBetweenSlots);

            if (inventorySlots[i].RectTransform.localPosition.x < bottomLeftPosition.x)
                bottomLeftPosition.x = inventorySlots[i].RectTransform.localPosition.x;

            if (inventorySlots[i].RectTransform.localPosition.y < bottomLeftPosition.y)
                bottomLeftPosition.y = inventorySlots[i].RectTransform.localPosition.y;
        }

        // Coordinates
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Vector2 slotOffset = (Vector2)inventorySlots[i].RectTransform.localPosition - bottomLeftPosition;
            Vector2Int slotCoordinates = new Vector2Int((int)(slotOffset.x / distanceBetweenSlots), (int)(slotOffset.y / distanceBetweenSlots));

            inventorySlots[i].SetupCoordinates(slotCoordinates);
        }
    }

    #endregion


    #region Open / Close Functions

    public bool GetCanOpenOrClose()
    {
        return !isMoving;
    }

    public IEnumerator OpenInventoryCoroutine(float duration, RectTransform forcedHiddenTr, RectTransform forcedShownTr)
    {
        _rectTransform.position = forcedHiddenTr.position;
        _lootParent.position = forcedHiddenTr.position;

        isMoving = true;

        _rectTransform.UChangePosition(duration * 0.75f, forcedShownTr.position + Vector3.up * 0.2f, CurveType.EaseOutSin);
        _lootParent.UChangePosition(duration * 0.75f, forcedShownTr.position + Vector3.up * 0.2f, CurveType.EaseOutSin);

        yield return new WaitForSeconds(duration * 0.75f + Time.deltaTime * 2);

        _rectTransform.UChangePosition(duration * 0.25f, forcedShownTr.position, CurveType.EaseInSin);
        _lootParent.UChangePosition(duration * 0.25f, forcedShownTr.position, CurveType.EaseInSin);

        yield return new WaitForSeconds(duration * 0.25f);

        isMoving = false;
    }

    public IEnumerator CloseInventoryCoroutine(float duration, RectTransform forcedHiddenTr)
    {
        isMoving = true;

        _rectTransform.UChangePosition(duration, forcedHiddenTr.position, CurveType.EaseOutCubic);
        _lootParent.UChangePosition(duration, forcedHiddenTr.position, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration);

        isMoving = false;
    }


    public IEnumerator OpenInventoryCoroutine(float duration, bool noMovement = false)
    {
        if (!noMovement)
        {
            RebootPosition();

            _rectTransform.position = _hiddenRectTr.position;
            _lootParent.position = _hiddenRectTr.position;

            isMoving = true;
        }

        if(!noMovement)
        {
            _rectTransform.DOLocalMove(_rectTransform.parent.InverseTransformPoint(_showedRectTr.position), duration).SetEase(Ease.OutBack);
            _lootParent.DOLocalMove(_rectTransform.parent.InverseTransformPoint(_showedRectTr.position), duration).SetEase(Ease.OutBack);
        }

        for(int i = 0; i < inventorySlots.Length; i++)
        {
            StartCoroutine(inventorySlots[i].AppearEffectCoroutine(Random.Range(0.0f, 0.15f) + duration * 0.5f));
        }

        List<Loot> inventoryLoot = InventoriesManager.Instance.GetInventoryLoot(associatedHero != null ? associatedHero.HeroIndex : -1, IsChest);
        for (int i = 0; i < inventoryLoot.Count; i++)
        {
            inventoryLoot[i].DisableRaycastTarget(false);
            StartCoroutine(inventoryLoot[i].AppearInInventoryCoroutine(Random.Range(0, 0.15f) + duration * 0.5f));
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < inventoryLoot.Count; i++)
        {
            inventoryLoot[i].EnableRaycastTarget();
        }

        isMoving = false;
    }

    public IEnumerator CloseInventoryCoroutine(float duration, bool noMovement = false)
    {
        if(!noMovement)
        {
            isMoving = true;
            //HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.RestartControl();

            _rectTransform.DOLocalMove(_rectTransform.parent.InverseTransformPoint(_hiddenRectTr.position), duration).SetEase(Ease.OutBack);
            _lootParent.DOLocalMove(_rectTransform.parent.InverseTransformPoint(_hiddenRectTr.position), duration).SetEase(Ease.OutBack);
        }

        List<Loot> inventoryLoot = InventoriesManager.Instance.GetInventoryLoot(associatedHero != null ? associatedHero.HeroIndex : -1, isChest);
        for (int i = 0; i < inventoryLoot.Count; i++)
        {
            inventoryLoot[i].DisableRaycastTarget(true);
        }

        yield return new WaitForSeconds(duration);

        isMoving = false; 
    }

    #endregion


    #region Auto Place 

    public bool VerifyCanPlaceItem(LootData data)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            List<InventorySlot> overlayedSlots = GetOverlayedCoordinates(inventorySlots[i].SlotCoordinates, data.spaceTaken, 0);
            if (overlayedSlots.Count == 0) continue;

            return true;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            List<InventorySlot> overlayedSlots = GetOverlayedCoordinates(inventorySlots[i].SlotCoordinates, data.spaceTaken, 90);
            if (overlayedSlots.Count == 0) continue;

            return true;
        }

        return false;
    }

    public bool AutoPlaceItem(Loot item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            List<InventorySlot> overlayedSlots = GetOverlayedCoordinates(inventorySlots[i].SlotCoordinates, item.LootData.spaceTaken, 0);
            if (overlayedSlots.Count == 0) continue;

            item.PlaceInInventory(overlayedSlots);
            return true;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            List<InventorySlot> overlayedSlots = GetOverlayedCoordinates(inventorySlots[i].SlotCoordinates, item.LootData.spaceTaken, 90);
            if (overlayedSlots.Count == 0) continue;

            item.PlaceInInventory(overlayedSlots);
            return true;
        }

        return false;
    }

    public List<InventorySlot> GetOverlayedCoordinates(Vector2Int bottomLeftCoord, SpaceTakenRow[] spaceTaken, float currentAngle)
    {
        List<InventorySlot> validSlots = new List<InventorySlot>();

        for (int y = 0; y < spaceTaken.Length; y++)
        {
            for (int x = 0; x < spaceTaken[y].row.Length; x++)
            {
                Vector2 currentCoord = bottomLeftCoord;
                currentCoord += new Vector2(x, y).RotateDirection(currentAngle);

                InventorySlot currentSlot = GetSlotAtCoord(new Vector2Int(Mathf.RoundToInt(currentCoord.x), Mathf.RoundToInt(currentCoord.y)));

                if (currentSlot is null) return new List<InventorySlot>();    // If the placement isn't valid
                if (currentSlot.VerifyHasLoot()) return new List<InventorySlot>();
                if (!currentSlot.IsAvailable) return new List<InventorySlot>();

                validSlots.Add(currentSlot);             // If the placement is valid
            }
        }

        return validSlots;
    }

    private InventorySlot GetSlotAtCoord(Vector2Int coord)
    {
        if (coord.x < 0 || coord.y < 0) return null;
        if (coord.x >= inventorySlotsTab.GetLength(0) || coord.y >= inventorySlotsTab.GetLength(1)) return null;
        return inventorySlotsTab[coord.x, coord.y];
    }

    #endregion


    #region Others

    public InventorySlot[] GetSlots()
    {
        return inventorySlots;
    }

    public void UpgradeInventory()
    {
        currentLevel++;

        UpdateInventoryLevel(currentLevel);
    }

    public void UpdateInventoryLevel(int newLevel)
    {
        currentLevel = newLevel;

        if(currentLevel >= 1)
        {
            for(int i = 0; i < _upgrade1Slots.Length; i++)
            {
                _upgrade1Slots[i].SetIsAvailable(true);
            }
        }
        else
        {
            for (int i = 0; i < _upgrade1Slots.Length; i++)
            {
                _upgrade1Slots[i].SetIsAvailable(false);
            }
        }

        if (currentLevel >= 2)
        {
            for (int i = 0; i < _upgrade2Slots.Length; i++)
            {
                _upgrade2Slots[i].SetIsAvailable(true);
            }
        }
        else
        {
            for (int i = 0; i < _upgrade2Slots.Length; i++)
            {
                _upgrade2Slots[i].SetIsAvailable(false);
            }
        }
    }

    public int GetItemCount(LootData item)
    {
        int currentCount = 0;
        HashSet<Loot> banned = new HashSet<Loot>();

        for(int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].AttachedLoot is null) continue;
            if (inventorySlots[i].AttachedLoot.LootData != item) continue;
            if (banned.Contains(inventorySlots[i].AttachedLoot)) continue;

            banned.Add(inventorySlots[i].AttachedLoot);
            currentCount++;
        }

        return currentCount;
    }

    private Vector2Int GetInventoryDimensions()
    {
        Vector2Int result = Vector2Int.zero;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].SlotCoordinates.x > result.x)
                result.x = inventorySlots[i].SlotCoordinates.x;

            if (inventorySlots[i].SlotCoordinates.y > result.y)
                result.y = inventorySlots[i].SlotCoordinates.y;
        }

        return result;
    }

    public void RebootPosition()
    {
        _rectTransform.SetParent(originalParent, true);
        _lootParent.SetParent(originalLootParentParent, true);

        _rectTransform.position = _hiddenRectTr.position;
        _lootParent.position = _hiddenRectTr.position;

        _rectTransform.localScale = Vector3.one * inventoryScale;
        _lootParent.localScale = Vector3.one;
    }

    public void EnterBlacksmith()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetIsInBlacksmith(true);
        }
    }

    public void ExitBlacksmith()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetIsInBlacksmith(false);
        }
    }

    public void EnableRaycastTarget()
    {
        List<Loot> inventoryLoot = InventoriesManager.Instance.GetInventoryLoot(associatedHero.HeroIndex, isChest);

        for (int i = 0; i < inventoryLoot.Count; i++)
        {
            inventoryLoot[i].EnableRaycastTarget();
        }

    }

    #endregion
}
