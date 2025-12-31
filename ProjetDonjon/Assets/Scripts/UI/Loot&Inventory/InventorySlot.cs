using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float overlaySize;
    [SerializeField] private Sprite[] possibleSprites;

    [Header("Private Infos")]
    public List<InventorySlot> neighbors = new();
    public Vector2Int slotCoordinates;
    private Vector3 baseSize;
    private bool isOverlayed;
    private bool isAvailable;
    private int overlayCount;
    private Loot attachedLoot;
    private Inventory associatedInventory;

    [Header("Public Infos")]
    public Vector2Int SlotCoordinates { get { return slotCoordinates; } }
    public RectTransform RectTransform { get { return _rectTr; } }
    public Loot AttachedLoot { get { return attachedLoot; } }
    public Inventory AssociatedInventory { get { return associatedInventory; } }
    public bool IsAvailable { get { return isAvailable; } }

    [Header("References")]
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] public Image _mainImage;
    private RectTransform _lootParent;
    private RectTransform _overlayedParent;
    private RectTransform _baseParent;


    private void Start()
    {
        baseSize = _rectTr.localScale;
    }

    public void SetupCoordinates(Vector2Int coordinates)
    {
        slotCoordinates = coordinates;
    }

    public void SetupReferences(RectTransform lootPrt, RectTransform basePrt, RectTransform overlayedPrt, Inventory inventory)
    {
        _lootParent = lootPrt;
        _baseParent = basePrt;  
        _overlayedParent = overlayedPrt;
        associatedInventory = inventory;
    }

    public void SetIsAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;
    }


    #region Manage Slot Content Functions

    public bool VerifyHasLoot()
    {
        return attachedLoot != null;
    }

    public void AddLoot(Loot loot, Image image)
    {
        attachedLoot = loot;
        image.rectTransform.SetParent(_lootParent);
    }

    public Loot RemoveLoot()
    {
        Loot returnedLoot = attachedLoot;
        attachedLoot = null;
        return returnedLoot;
    }


    #endregion


    #region Neighbors Functions

    public void SetupNeighbors(InventorySlot[] allSlots, float maxDistBetweenTiles)
    {
        for(int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] == this) continue;

            float dist = Vector2.Distance(_rectTr.localPosition, allSlots[i]._rectTr.localPosition);
            if(dist <= maxDistBetweenTiles)
            {
                neighbors.Add(allSlots[i]);
            }
        }
    }

    public bool VerifyHasNeighbor(Vector2Int addedCoordinates)
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i].SlotCoordinates == slotCoordinates + addedCoordinates) 
                return true;
        }

        return false;
    }

    #endregion


    #region Mouse Feedbacks

    private void LateUpdate()
    {
        if (!isOverlayed) return;

        if(overlayCount == 1)
        {
            QuitOverlaySlot();
        }
        overlayCount++;
    }

    public void OverlaySlot()
    {
        transform.localScale = Vector3.one * overlaySize;
        _rectTr.SetParent(_overlayedParent);
        isOverlayed = true;
        overlayCount = 0;
    }

    public void QuitOverlaySlot()
    {
        transform.localScale = baseSize;
        _rectTr.SetParent(_baseParent);
        isOverlayed = false;
    }

    #endregion
}
