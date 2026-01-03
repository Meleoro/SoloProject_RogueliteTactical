using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class EquipmentSlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private int slotIndex;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color hiddenColor;
    [SerializeField] private float highlightSize;

    [Header("Actions")]
    public Action<Loot, int, Hero> OnEquipmentAdd;
    public Action<Loot, int, Hero> OnEquipmentRemove;

    [Header("Public Infos")]
    public EquipmentType EquipmentType { get { return equipmentType; } }
    public Loot EquipedLoot { get { return equipedLoot; } } 

    [Header("Private Infos")]
    private Loot equipedLoot;
    private Coroutine highlightCoroutine;
    private Color saveBaseColor;

    [Header("References")]
    [SerializeField] Image _equipmentImage;
    [SerializeField] Image _iconImage;
    [SerializeField] Image[] _allSlotImages;


    private void Start()
    {
        saveBaseColor = _iconImage.color;
    }


    #region Mouse Controls

    public void SelectSlot()
    {
        if(UIManager.Instance.DraggedLoot is not null)
        {
            if (UIManager.Instance.DraggedLoot.LootData.equipmentType != equipmentType) return;

            UIManager.Instance.DraggedLoot.EnterEquipmentSlot(this);
        }
    }

    public void UnselectSlot()
    {
        if (UIManager.Instance.DraggedLoot is not null)
        {
            if (UIManager.Instance.DraggedLoot.LootData.equipmentType != equipmentType) return;

            UIManager.Instance.DraggedLoot.ExitEquipmentSlot();
        }
    }

    #endregion


    #region Add / Remove Functions

    public bool VerifyCanAddLoot(Loot verifiedLoot)
    {
        if (verifiedLoot.LootData.lootType != LootType.Equipment) return false;

        return verifiedLoot.LootData.equipmentType == equipmentType;
    }

    public void AddEquipment(Loot addedLoot, bool callAction, Hero hero)
    {
        if(equipedLoot != null)
        {
            RemoveEquipment(callAction, hero);
        }

        _equipmentImage.sprite = addedLoot.LootData.equipmentSprite;
        _equipmentImage.enabled = true;
        _equipmentImage.SetNativeSize();

        equipedLoot = addedLoot;

        if (callAction) 
        {
            AudioManager.Instance.PlaySoundOneShot(0, 2);
            OnEquipmentAdd?.Invoke(addedLoot, slotIndex, hero); 
        }
    }



    public void RemoveEquipment(bool callAction, Hero hero)
    {
        _equipmentImage.sprite = null;
        _equipmentImage.enabled = false;

        equipedLoot = null;

        if (callAction) 
        {
            if (equipedLoot)
                equipedLoot.Unequip();

            AudioManager.Instance.PlaySoundOneShot(0, 3);

            OnEquipmentRemove?.Invoke(equipedLoot, slotIndex, hero); 
        }
    }

    #endregion


    #region Other Effects

    public void HightlightSlot()
    {
        highlightCoroutine = StartCoroutine(HighlightEffectCoroutine());

        _iconImage.DOColor(highlightColor, 0.2f).SetEase(Ease.OutCubic);

        /*for(int i = 0; i < _allSlotImages.Length; i++)
        {
            _allSlotImages[i].ULerpImageColor(0.2f, highlightColor);
        }*/
    }

    public void HideSlot()
    {
        _iconImage.DOColor(hiddenColor, 0.2f).SetEase(Ease.OutCubic);

        /*for (int i = 0; i < _allSlotImages.Length; i++)
        {
            _allSlotImages[i].ULerpImageColor(0.2f, hiddenColor);
        }*/
    }

    public void GetBackToNormal()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        for (int i = 0; i < _allSlotImages.Length; i++)
        {
            _iconImage.DOColor(saveBaseColor, 0.2f).SetEase(Ease.OutCubic);
        }
    }

    private IEnumerator HighlightEffectCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
    }

    #endregion
}
