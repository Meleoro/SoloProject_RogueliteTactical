using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private LootData itemData;
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;
    [SerializeField] private bool reverseDetails;

    [Header("Action")]
    public Action<ShopItem, LootData> OnValidClick;

    [Header("Private Infos")]
    private Vector3 originalPos;
    private Color baseColor;
    private Coroutine textBuyCoroutine;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _buyText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _itemColliderImage;



    private void Start()
    {
        _itemImage.material = new Material(_itemImage.material);
        _itemImage.material.SetVector("_TextureSize", new Vector2(_itemImage.sprite.texture.width, _itemImage.sprite.texture.height));

        originalPos = _itemImage.rectTransform.localPosition;
        baseColor = _priceText.color;

        LoadData(itemData);
    }

    private void LoadData(LootData data)
    {
        _itemImage.sprite = data.sprite;
        _itemColliderImage.sprite = data.sprite;

        _itemImage.SetNativeSize();
        _itemColliderImage.SetNativeSize();

        _priceText.text = data.value.ToString();
    }


    #region Mouse Inputs

    public void Hover()
    {
        _itemImage.DOComplete();

        UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(itemData, transform.position, reverseDetails);

        _itemImage.rectTransform.DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutCubic);
        _itemImage.rectTransform.DOLocalMove(originalPos + Vector3.up * 10f, 0.15f).SetEase(Ease.OutCubic);
        _itemImage.material.DOFloat(1f, "_OutlineSize", 0.15f).SetEase(Ease.OutCubic);
    }

    public void Unhover()
    {
        _itemImage.DOComplete();
        _itemImage.rectTransform.DOComplete();

        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();

        _itemImage.rectTransform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutCubic);
        _itemImage.rectTransform.DOLocalMove(originalPos, 0.15f).SetEase(Ease.OutCubic);
        _itemImage.material.DOFloat(0f, "_OutlineSize", 0.15f).SetEase(Ease.OutCubic);
    }

    public void Click()
    {
        if (InventoriesManager.Instance.CurrentCoins < itemData.value)
        {
            _itemImage.DOComplete();
            _itemImage.rectTransform.DOComplete();
            StopAllCoroutines();

            _itemImage.rectTransform.DOShakePosition(0.3f, 10.0f, 40, 90, false, true);
            StartCoroutine(BouncePriceColorCoroutine(invalidColor));

            return;
        }

        OnValidClick?.Invoke(this, itemData);

        _itemImage.DOComplete();
        StopAllCoroutines();

        StartCoroutine(BouncePriceColorCoroutine(validColor));
        StartCoroutine(BounceBuyCoroutine());

        if (textBuyCoroutine != null) 
        { 
            StopCoroutine(textBuyCoroutine);
            _buyText.DOComplete();
        }

        textBuyCoroutine = StartCoroutine(TextBuyCoroutine());
    }

    #endregion


    #region Game Feel

    private IEnumerator BounceBuyCoroutine()
    {
        _itemImage.rectTransform.DOScale(Vector3.one * 0.8f, 0.1f);

        yield return new WaitForSeconds(0.1f);

        _itemImage.rectTransform.DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutCubic);
    }

    private IEnumerator BouncePriceColorCoroutine(Color color)
    {
        _priceText.DOColor(color, 0.1f);
        _priceText.rectTransform.DOScale(Vector3.one * 0.8f, 0.1f);

        yield return new WaitForSeconds(0.1f);

        _priceText.DOColor(baseColor, 0.15f).SetEase(Ease.OutCubic);
        _priceText.rectTransform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutCubic);
    }

    private IEnumerator TextBuyCoroutine()
    {
        _buyText.enabled = true;
        _buyText.DOColor(new Color(_buyText.color.r, _buyText.color.g, _buyText.color.b, 1), 0.1f);
        _buyText.rectTransform.localPosition = Vector3.zero;
        _buyText.rectTransform.DOLocalMove(new Vector3(0, 50, 0), 0.5f).SetEase(Ease.OutCirc);

        yield return new WaitForSeconds(0.5f);

        _buyText.DOColor(new Color(_buyText.color.r, _buyText.color.g, _buyText.color.b, 0), 0.1f);

        yield return new WaitForSeconds(0.1f);

        _buyText.color = new Color(_buyText.color.r, _buyText.color.g, _buyText.color.b, 0);
        _buyText.enabled = true;
    }

    #endregion
}
