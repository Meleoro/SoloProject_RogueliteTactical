using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Utilities;
using DG.Tweening;

[Serializable]
public class BackgroundType
{
    public Sprite backgroundSprite;
    public SpaceTakenRow[] spaceTaken;
}

public class Loot : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private LootData lootData;
    [SerializeField] private LayerMask UIcollisionLayer;
    [SerializeField] private BackgroundType[] possibleBackgrounds;
    [SerializeField] private Color[] lootColorAccordingToType;
    [SerializeField] private float dragLerpSpeed;
    [SerializeField] private bool debug;

    [Header("Appear Effect Parameters")]
    [SerializeField] private float minAppearAddedY;
    [SerializeField] private float maxAppearAddedY;
    [SerializeField] private float minAppearAddedX, maxAppearAddedX;
    [SerializeField] private float appearDuration;

    [Header("Public Infos")]
    public LootData LootData { get { return lootData; } }
    public Image Image { get { return _image; } }
    public Hero AssociatedHero { get { return associatedHero; } }
    public bool IsEquipped { get { return isEquipped; } }
    public InventorySlot BottomLeftSlot { get { return bottomLeftSlot; } }
    public int CurrentAngle { get { return currentAngle; } }

    [Header("Private Infos")]
    private bool isDragged;
    private bool isRotating;
    private bool isPlacedInInventory;
    private bool isSquishing;
    private bool isOverlayed;
    private bool isEquipped;
    private Vector3 addedSize;
    private InventorySlot[] slotsOccupied;
    private InventorySlot bottomLeftSlot;      // For Save
    private EquipmentSlot overlayedEquipmentSlot;
    private EquipmentSlot equipmentSlot;
    private Vector3 dragWantedPos;
    private Vector3 saveSize;
    private Coroutine inventoryBounceCoroutine;
    private Hero associatedHero;
    private int currentAngle;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Image _image;
    [SerializeField] private Image _imageBackground;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _secondaryText;
    [SerializeField] private SpriteRenderer _rays1SpriteRenderer;
    [SerializeField] private SpriteRenderer _rays2SpriteRenderer;
    [SerializeField] private Image _equippedImage;



    private void Start()
    {
        if(debug)
            Initialise(lootData);

        _imageBackground.material = new Material(_imageBackground.material);
        _image.material = new Material(_image.material);

        saveSize = Vector3.one * 0.75f;
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
    }

    private void Update()
    {
        if (isPlacedInInventory) return;
        if (isSquishing) return;

        _imageBackground.rectTransform.position = Vector3.Lerp(_imageBackground.rectTransform.position, dragWantedPos, Time.deltaTime * dragLerpSpeed);

        if (isOverlayed && !isDragged) return;

        //DoMoveSquishEffect();

        if (!isDragged) return;
        
        ManageDrag();
    }


    public void Initialise(LootData data, bool noAppear = false)
    {
        lootData = data;
        _spriteRenderer.sprite = lootData.sprite;
        _image.sprite = lootData.sprite;

        _image.enabled = false;
        _nameText.enabled = false;
        _secondaryText.enabled = false;

        if(!noAppear) StartCoroutine(AppearCoroutine());
    }


    #region Become / Quit Inventory

    public void BecomeInventoryItem(bool isStart = false, int angle = 0)
    {
        _image.enabled = true;
        _imageBackground.enabled = true;
        _spriteRenderer.enabled = false;
        _collider.enabled = false;
        _nameText.enabled = false;
        _secondaryText.enabled = false;
        _rays1SpriteRenderer.enabled = false;
        _rays2SpriteRenderer.enabled = false;

        _imageBackground.rectTransform.position = CameraManager.Instance.transform.position + Vector3.forward * 9;
        _imageBackground.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        _imageBackground.rectTransform.SetParent(InventoriesManager.Instance.MainLootParent);

        currentAngle = angle;

        _image.rectTransform.localScale = Vector3.one;
        _image.SetNativeSize();

        _imageBackground.sprite = GetBackgroundSprite(lootData.spaceTaken);
        _imageBackground.rectTransform.localScale = Vector3.one * 0.4f;
        _imageBackground.SetNativeSize();

        dragWantedPos = InventoriesManager.Instance.MainLootParent.position;

        _imageBackground.color = lootColorAccordingToType[(int)lootData.lootType];

        slotsOccupied = new InventorySlot[0];

        InventoriesManager.Instance.OnInventoryClose += BecomeWorldItem;
        InventoriesManager.Instance.AddItem(this);
        isPlacedInInventory = false;

        _imageBackground.material.SetColor("_ShineColor", Color.white);
        _image.material.SetColor("_ShineColor", Color.white);

        if (isStart) return;

        _imageBackground.rectTransform.localScale = Vector3.zero;
        
        StartCoroutine(SquishCoroutine(0.4f));
        inventoryBounceCoroutine = StartCoroutine(InventoryBounceCoroutine(1.25f, 0.15f));
    }

    public void BecomeWorldItem()
    {
        InventoriesManager.Instance.OnInventoryClose -= BecomeWorldItem;
        InventoriesManager.Instance.RemoveItem(this);

        for (int i = 0; i < slotsOccupied.Length; i++)
        {
            slotsOccupied[i].RemoveLoot();
        }

        if (isEquipped)
        {
            Unequip();
        }

        transform.position = HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].transform.position;
        StartCoroutine(AppearCoroutine());

        _image.enabled = false;
        _imageBackground.enabled = false;
        _equippedImage.enabled = false;
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }

    protected IEnumerator SquishCoroutine(float duration)
    {
        float aimedSize = 0.75f;
        isSquishing = true;
        _imageBackground.rectTransform.UChangeScale(duration * 0.24f, new Vector3(1.25f * aimedSize, 0.85f * aimedSize, 1f * aimedSize), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _imageBackground.rectTransform.UChangeScale(duration * 0.48f, new Vector3(0.85f * aimedSize, 1.15f * aimedSize, 1f * aimedSize), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        _imageBackground.rectTransform.UChangeScale(duration * 0.24f, new Vector3(1f * aimedSize, 1f * aimedSize, 1f * aimedSize), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.25f);

        isSquishing = false;
    }


    public void Equip(EquipmentSlot slot)
    {
        if(slot is null)
        {
            slot = UIManager.Instance.HeroInfosScreen.GetAppropriateEquipmentSlot(LootData.equipmentType);
        }

        isEquipped = true;

        equipmentSlot = slot;
        equipmentSlot.AddEquipment(this, true);

        float slotSize = InventoriesManager.Instance.slotSize;
        _equippedImage.rectTransform.localPosition = new Vector3(-_imageBackground.rectTransform.rect.width * 0.5f + 20, 
            -_imageBackground.rectTransform.rect.height * 0.5f + 20);
        _equippedImage.enabled = true;
    }

    public void Unequip()
    {
        if (!isEquipped) return;

        isEquipped = false;

        _equippedImage.enabled = false;
        equipmentSlot.RemoveEquipment(true);
        equipmentSlot = null;
    }


    public void DestroyItem()
    {
        InventoriesManager.Instance.OnInventoryClose -= BecomeWorldItem;

        for (int i = 0; i < slotsOccupied.Length; i++)
        {
            slotsOccupied[i].RemoveLoot();
        }

        Destroy(_imageBackground.gameObject);
        Destroy(gameObject);
    }

    #endregion


    #region Equipment Functions

    public void EnterEquipmentSlot(EquipmentSlot equimentSlot)
    {
        overlayedEquipmentSlot = equimentSlot;
    }

    public void ExitEquipmentSlot()
    {
        overlayedEquipmentSlot = null;
    }

    #endregion


    #region UI Drag & Drop Functions

    public void Drag()
    {
        if (!isDragged)
        {
            isDragged = true;
            UIManager.Instance.DraggedLoot = this;
            ActualiseOverlayedSlots();

            _imageBackground.raycastTarget = false;

            if (isPlacedInInventory)
            {
                isPlacedInInventory = false;
                InventoriesManager.Instance.OnInventoryClose += BecomeWorldItem;

                for (int i = 0;i < slotsOccupied.Length; i++)
                {
                    slotsOccupied[i].RemoveLoot();
                }
            }

            if(inventoryBounceCoroutine != null)
            {
                StopCoroutine(inventoryBounceCoroutine);
                addedSize = Vector3.zero;
            }
        }

        Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        dragWantedPos = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);
    }

    private void ManageDrag()
    {
        ActualiseOverlayedSlots();

        if (isRotating) return;
        if (InputManager.wantsToRotateLeft)
        {
            StartCoroutine(RotateLootCoroutine(0.1f, _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, -90),
                _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, -115)));

            currentAngle -= 90;
        }
        if (InputManager.wantsToRotateRight)
        {
            StartCoroutine(RotateLootCoroutine(0.1f, _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, 90),
                _imageBackground.rectTransform.rotation * Quaternion.Euler(0, 0, 115)));

            currentAngle += 90;
        }
    }

    public void Drop()
    {
        isDragged = false;
        UIManager.Instance.DraggedLoot = null;
        _imageBackground.raycastTarget = true;

        if (isRotating) return;

        // If we dropped the loot on an equipment slot
        if (overlayedEquipmentSlot is not null) 
        {
            Equip(overlayedEquipmentSlot);
        }

        List<InventorySlot> overlayedSlots = GetOverlayedSlots();

        // We check if the slots are empty
        int emptySlotsCount = GetOverlayedSlotsEmptyCount(overlayedSlots);
        if (emptySlotsCount != overlayedSlots.Count && overlayedEquipmentSlot is null) 
        {
            FailDrop();
            return; 
        }
        if (GetNeededOccupiedSpace() != emptySlotsCount && overlayedEquipmentSlot is null) 
        {
            FailDrop();
            return; 
        }

        // If no slots are selected
        if((overlayedSlots.Count == 0 && slotsOccupied.Length != 0) || overlayedEquipmentSlot is not null)
        {
            overlayedSlots = slotsOccupied.ToList();
        }
        else if(overlayedSlots.Count == 0 && slotsOccupied.Length == 0)
        {
            inventoryBounceCoroutine = StartCoroutine(InventoryBounceCoroutine(1.25f, 0.15f));
            return;
        }

        // We remove the loot from the previous tiles
        for (int i = 0; i < slotsOccupied.Length; i++)
        {
            slotsOccupied[i].RemoveLoot();
        }

        PlaceInInventory(overlayedSlots);
    }

    private void FailDrop()
    {
        if(slotsOccupied.Length > 0)
        {
            PlaceInInventory(slotsOccupied.ToList());
        }
        else
        {
            inventoryBounceCoroutine = StartCoroutine(InventoryBounceCoroutine(1.25f, 0.15f));
        }
    }

    public void PlaceInInventory(List<InventorySlot> overlayedSlots)
    {
        if(overlayedSlots.Count == 0)
        {
            return;
        }

        Vector3 position = Vector2.zero;
        slotsOccupied = new InventorySlot[overlayedSlots.Count];

        bottomLeftSlot = overlayedSlots[0];

        for (int i = 0; i < overlayedSlots.Count; i++)
        {
            position += overlayedSlots[i].RectTransform.position;
            overlayedSlots[i].AddLoot(this, _imageBackground);
            slotsOccupied[i] = overlayedSlots[i];
        }

        position /= overlayedSlots.Count;
        _imageBackground.rectTransform.position = position;
        dragWantedPos = position;

        InventoriesManager.Instance.OnInventoryClose -= BecomeWorldItem;
        isPlacedInInventory = true;
        overlayedEquipmentSlot = null;

        associatedHero = overlayedSlots[0].AssociatedInventory.AssociatedHero;

        _imageBackground.material.SetColor("_ShineColor", Color.black);
        _image.material.SetColor("_ShineColor", Color.black);
        _imageBackground.rectTransform.UBounceScale(0.05f, Vector3.one * 0.6f, 0.15f, Vector3.one * 0.74f, CurveType.EaseInOutSin);
    }

    #endregion


    #region Other Mouse Functions

    private Coroutine overlayCoroutine;

    public void OverlayLoot()
    {
        isOverlayed = true;

        UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(lootData, _imageBackground.transform.position, 
            CameraManager.Instance.Camera.WorldToViewportPoint(_image.rectTransform.position).x > 0.65f);

        AudioManager.Instance.PlaySoundOneShot(0, 0);

        if (overlayCoroutine is not null) StopCoroutine(overlayCoroutine);
        overlayCoroutine = StartCoroutine(OverlayLootCoroutine(0.1f));
    }

    private IEnumerator OverlayLootCoroutine(float duration)
    {
        _imageBackground.rectTransform.UChangeScale(duration, saveSize * 1.35f, CurveType.EaseInOutSin);

        yield return new WaitForSeconds(duration);

        _imageBackground.rectTransform.UChangeScale(duration * 0.3f, saveSize * 1.2f, CurveType.EaseInOutSin);
    }

    public void QuitOverlayLoot()
    {
        isOverlayed = false;
        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();

        if (overlayCoroutine is not null) StopCoroutine(overlayCoroutine);

        _imageBackground.rectTransform.UChangeScale(0.1f, saveSize, CurveType.EaseInOutSin);
        _imageBackground.material.ULerpMaterialColor(0.1f, new Color(0f, 0f, 0f, 0f), "_AddedColor");
        _image.material.ULerpMaterialColor(0.1f, new Color(0f, 0f, 0f, 0f), "_AddedColor");
    }

    public void OverlayOtherLoot()
    {
        _imageBackground.DOFade(0.7f, 0.2f);
        _image.DOFade(0.7f, 0.2f);
    }

    public void QuitOverlayOtherLoot()
    {
        _imageBackground.DOFade(1f, 0.2f);
        _image.DOFade(1f, 0.2f);
    }

    public void ClickLoot()
    {
        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();

        AudioManager.Instance.PlaySoundOneShot(0, 1);

        InventoriesManager.Instance.InventoryActionPanel.OpenPanel(this);
    }

    #endregion


    #region World Private Functions

    private IEnumerator AppearCoroutine()
    {
        _collider.enabled = false;
        float sign = Random.Range(0, 2) == 0 ? -1 : 1;

        Vector2 finalPos = transform.position + new Vector3(Random.Range(minAppearAddedX, maxAppearAddedX) * sign, Random.Range(-minAppearAddedY, minAppearAddedY));

        transform.UChangeLocalPosition(appearDuration, finalPos, CurveType.None);
        _spriteRenderer.transform.UChangeLocalPosition(appearDuration * 0.6f, new Vector3(0, Random.Range(minAppearAddedY, maxAppearAddedY)), CurveType.EaseOutSin);
        _spriteRenderer.material.SetColor("_Color", Color.black);
        _spriteRenderer.material.ULerpMaterialColor(appearDuration * 0.8f, Color.white, "_Color");

        _rays1SpriteRenderer.material.SetFloat("_MaxDistance", 0f);
        _rays2SpriteRenderer.material.SetFloat("_MaxDistance", 0f);
        _rays1SpriteRenderer.material.ULerpMaterialFloat(appearDuration * 0.6f, 0.5f, "_MaxDistance");
        _rays2SpriteRenderer.material.ULerpMaterialFloat(appearDuration * 0.6f, 0.4f, "_MaxDistance");

        yield return new WaitForSeconds(appearDuration * 0.6f);

        _spriteRenderer.transform.UChangeLocalPosition(appearDuration * 0.4f, new Vector3(0, 0), CurveType.EaseInSin);
        _rays1SpriteRenderer.material.ULerpMaterialFloat(appearDuration * 0.4f, 0.4f, "_MaxDistance");
        _rays2SpriteRenderer.material.ULerpMaterialFloat(appearDuration * 0.4f, 0.3f, "_MaxDistance");

        yield return new WaitForSeconds(appearDuration * 0.4f);

        _collider.enabled = true;

        StartCoroutine(IdleCoroutine());
    }

    private IEnumerator IdleCoroutine()
    {
        while (true)
        {
            _spriteRenderer.transform.UChangeLocalPosition(0.9f, new Vector3(0, -0.075f, 0), CurveType.EaseInOutSin);

            yield return new WaitForSeconds(1f);

            _spriteRenderer.transform.UChangeLocalPosition(0.9f, new Vector3(0, 0.075f, 0), CurveType.EaseInOutSin);

            yield return new WaitForSeconds(1f);
        }
    }

    #endregion


    #region Inventory Private Functions

    private IEnumerator RotateLootCoroutine(float duration, Quaternion finalRotation, Quaternion furtherFinalRotation)
    {
        _imageBackground.rectTransform.UChangeRotation(duration * 0.68f, furtherFinalRotation);
        isRotating = true;

        yield return new WaitForSeconds(duration * 0.7f);
        _imageBackground.rectTransform.UStopChangeRotation();
        _imageBackground.rectTransform.UChangeRotation(duration * 0.28f, finalRotation);

        yield return new WaitForSeconds(duration * 0.3f);

        isRotating = false;
    }

    private void DoMoveSquishEffect()
    {
        float ratio = _imageBackground.rectTransform.rect.width / _imageBackground.rectTransform.rect.height;

        float difX = Mathf.Abs(dragWantedPos.x - _imageBackground.rectTransform.position.x) * 0.8f;
        float difY = Mathf.Abs(dragWantedPos.y - _imageBackground.rectTransform.position.y) * 0.8f;
        Vector2 addedSize = new Vector3(difX, difY);
        float saveMagnitude = addedSize.magnitude;

        addedSize = addedSize.RotateDirection(((Vector2)_imageBackground.rectTransform.right).GetAngleFromVector()) * saveMagnitude;
        addedSize = new Vector3(Mathf.Clamp(addedSize.x, -1f, 1f), Mathf.Clamp(addedSize.y, -1f, 1f));

        _imageBackground.rectTransform.localScale = Vector3.Lerp(_imageBackground.rectTransform.localScale, 
            Vector3.one * 0.75f + (Vector3)addedSize * 0.5f + this.addedSize, Time.deltaTime * 10f);
    }

    private int GetOverlayedSlotsEmptyCount(List<InventorySlot> overlayedSlots)
    {
        int returnedValue = 0;
        foreach (InventorySlot slot in overlayedSlots)
        {
            if (!slot.VerifyHasLoot() || slotsOccupied.Contains(slot)) returnedValue++;
        }

        return returnedValue;
    }

    private void ActualiseOverlayedSlots()
    {
        List<InventorySlot> overlayedSlots = GetOverlayedSlots();

        foreach (InventorySlot slot in overlayedSlots)
        {
            slot.OverlaySlot();
        }
    }

    private IEnumerator InventoryBounceCoroutine(float duration, float maxSize)
    {
        float addedSizeFloat = 0;
        float timer = 0;
        float temporary = 0;

        while (true)
        {
            timer = 0;
            addedSize = Vector3.zero;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                temporary = Mathf.Lerp(0, maxSize, timer / duration);
                addedSizeFloat = Mathf.Lerp(addedSizeFloat, temporary, Time.deltaTime * 3f);
                addedSize = Vector3.one * addedSizeFloat;

                yield return new WaitForEndOfFrame();
            }

            timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                temporary = Mathf.Lerp(maxSize, 0, timer / duration);
                addedSizeFloat = Mathf.Lerp(addedSizeFloat, temporary, Time.deltaTime * 3f);
                addedSize = Vector3.one * addedSizeFloat;

                yield return new WaitForEndOfFrame();
            }
        }
    }

    private List<InventorySlot> GetOverlayedSlots()
    {
        Vector2 bottomLeft = new Vector2((lootData.spaceTaken[0].row.Length * InventoriesManager.Instance.slotSize) * -0.5f + InventoriesManager.Instance.slotSize * 0.5f,
            (lootData.spaceTaken.Length * InventoriesManager.Instance.slotSize) * -0.5f + InventoriesManager.Instance.slotSize * 0.5f);

        if (lootData.spaceTaken[0].row.Length == 1) bottomLeft.x = 0;
        if (lootData.spaceTaken.Length == 1) bottomLeft.y = 0;

        Vector2 raycastRelativePos = bottomLeft;
        Vector3 raycastPos = _imageBackground.rectTransform.TransformPoint(raycastRelativePos);
        InventorySlot bottomLeftSlot = InventoriesManager.Instance.VerifyIsOverlayingSlot(raycastPos, _imageBackground.rectTransform.position);

        if (bottomLeftSlot is null) return new List<InventorySlot>();
        List<InventorySlot> finalOverlayedSlots = bottomLeftSlot.AssociatedInventory.GetOverlayedCoordinates(bottomLeftSlot.SlotCoordinates, 
            lootData.spaceTaken, _imageBackground.rectTransform.eulerAngles.z);

        if(finalOverlayedSlots.Count == 0) return new List<InventorySlot>();

        return finalOverlayedSlots;
    }

    private int GetNeededOccupiedSpace()
    {
        int returnedValue = 0;

        for(int x = 0; x < lootData.spaceTaken.Length; x++)
        {
            for (int y = 0; y < lootData.spaceTaken[x].row.Length; y++)
            {
                if (lootData.spaceTaken[x].row[y]) returnedValue++;
            }
        }

        return returnedValue;
    }

    private Sprite GetBackgroundSprite(SpaceTakenRow[] spaceTaken)
    {
        foreach (BackgroundType possibleBackground in possibleBackgrounds)
        {
            bool found = true;

            for(int x = 0; x < spaceTaken.Length; x++)
            {
                if (possibleBackground.spaceTaken.Length <= x)
                {
                    found = false;
                    break;
                }

                for (int y = 0; y < spaceTaken[x].row.Length; y++)
                {
                    if (possibleBackground.spaceTaken[x].row.Length <= y)
                    {
                        found = false; 
                        break;
                    }

                    if (spaceTaken[x].row[y] != possibleBackground.spaceTaken[x].row[y]) found = false;
                }
            }

            if (found)
                return possibleBackground.backgroundSprite;
        }

        return null;
    }

    #endregion


    #region Interface Functions

    public void CanBePicked()
    {
        transform.UChangeScale(0.1f, Vector3.one * 1.1f);
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 1f, "_OutlineSize");
        _rays1SpriteRenderer.material.ULerpMaterialFloat(0.1f, 0.5f, "_MaxDistance");
        _rays2SpriteRenderer.material.ULerpMaterialFloat(0.1f, 0.4f, "_MaxDistance");

        _nameText.enabled = true;
        _nameText.text = lootData.lootName;
        _secondaryText.enabled = true;
    }

    public void CannotBePicked()
    {
        transform.UChangeScale(0.1f, Vector3.one);
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
        _rays1SpriteRenderer.material.ULerpMaterialFloat(0.1f, 0.4f, "_MaxDistance");
        _rays2SpriteRenderer.material.ULerpMaterialFloat(0.1f, 0.3f, "_MaxDistance");

        _nameText.enabled = false;
        _secondaryText.enabled = false;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        InventoriesManager.Instance.AddItemToInventories(this);
        BecomeInventoryItem();
    }

    #endregion
}
