using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class QuickChangeButton : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int heroIndex;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite unselectedSprite;
    [SerializeField] private Sprite selectedIconSprite;
    [SerializeField] private Sprite unselectedIconSprite;

    [Header("Actions")]
    public Action<int> OnClick;

    [Header("Private Infos")]
    private bool isDisplayed;
    private bool isSelected;

    [Header("References")]
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Image _heroIconImage;
    [SerializeField] private Image _mainImage;


    public void ActualiseInfos(RectTransform parent, bool isDisplayed) 
    {
        _rectTr.SetParent(parent);
        _rectTr.localPosition = Vector3.zero;
        _rectTr.localScale = Vector3.one;   

        this.isDisplayed = isDisplayed;

        if (isDisplayed)
        {
            _heroIconImage.enabled = true;
            _mainImage.enabled = true;
        }
        else
        {
            _heroIconImage.enabled = false;
            _mainImage.enabled = false;
        }
    }


    #region Select / Show

    public void Show()
    {
        _mainImage.enabled = true;
        _heroIconImage.enabled = true;
    }

    public void Hide()
    {
        _mainImage.enabled = false;
        _heroIconImage.enabled = false;
    }

    public void SelectHero()
    {
        if (isSelected) return;
        isSelected = true;

        _mainImage.sprite = selectedSprite;
        _mainImage.SetNativeSize();

        _heroIconImage.sprite = selectedIconSprite;
        _heroIconImage.SetNativeSize();

        _rectTr.DOLocalMoveY(5, 0.2f).SetEase(Ease.OutBack);
        _heroIconImage.rectTransform.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnselectHero()
    {
        if (!isSelected) return;
        isSelected = false;

        _mainImage.sprite = unselectedSprite;
        _mainImage.SetNativeSize();

        _heroIconImage.sprite = unselectedIconSprite;
        _heroIconImage.SetNativeSize();

        _rectTr.DOLocalMoveY(0, 0.2f).SetEase(Ease.OutBack);
        _heroIconImage.rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    #endregion


    #region Mouse Inputs

    public void Hover()
    {
        _rectTr.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Unhover()
    {
        _rectTr.DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Click()
    {
        OnClick?.Invoke(heroIndex);
    }

    #endregion
}
