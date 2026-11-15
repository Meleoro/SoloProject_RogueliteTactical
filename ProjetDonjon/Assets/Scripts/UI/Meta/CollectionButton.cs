using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class CollectionButton : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;

    [Header("Actions")]
    public Action<int> OnClick;

    [Header("Private Infos")]
    private bool isSelected;
    private int buttonIndex;
    private Vector3 savePos;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private RectTransform _rectTr;


    public void SetupButton(int index)
    {
        buttonIndex = index;
    }

    public void SetIsSelected(bool selected)
    {
        isSelected = selected;

        _text.color = isSelected ? selectedColor : unselectedColor;
    }


    public void Hover()
    {
        if(isSelected)
        {
            return;
        }

        if(savePos == Vector3.zero)
        {
            savePos = _rectTr.localPosition;
        }

        _text.color = selectedColor;
        _rectTr.DOLocalMove(savePos + Vector3.up * 5, 0.2f).SetEase(Ease.OutBack);
    }

    public void Unhover()
    {
        if(!isSelected)
        {
            _text.color = unselectedColor;
            _rectTr.DOLocalMove(savePos, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void Click()
    {
        if (isSelected)
        {
            return;
        }

        OnClick.Invoke(buttonIndex);
        SetIsSelected(true);
    }
}
