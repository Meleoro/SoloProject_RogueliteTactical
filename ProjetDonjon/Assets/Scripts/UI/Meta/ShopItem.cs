using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private LootData itemData;

    [Header("Action")]
    public Action<ShopItem, LootData> OnValidClick;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _itemImage;



    private void Start()
    {
        LoadData(itemData);
    }

    private void LoadData(LootData data)
    {
        _itemImage.sprite = data.sprite;
        _priceText.text = data.value.ToString();
    }


    #region Mouse Inputs

    public void Hover()
    {
        _itemImage.rectTransform.DOScale(Vector3.one * 1.15f, 0.15f).SetEase(Ease.OutBack);
    }

    public void Unhover()
    {
        _itemImage.rectTransform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.InBack);
    }

    public void Click()
    {
        //if (InventoriesManager.Instance.CurrentCoins < itemData.value) return;

        OnValidClick?.Invoke(this, itemData);
    }

    #endregion
}
