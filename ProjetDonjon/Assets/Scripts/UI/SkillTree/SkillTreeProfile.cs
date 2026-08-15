using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeProfile : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int heroIndex;

    [Header("Actions")]
    public Action<int> OnClick;

    [Header("Private Infos")]
    private bool isSelected;
    private Vector3 baseOffset;

    [Header("References")]
    [SerializeField] private RectTransform _heroNameRectTr;
    [SerializeField] private RectTransform _heroPointsRectTr;
    [SerializeField] private SkillTreeProfile _leftProfile;
    [SerializeField] private SkillTreeProfile _rightProfile;
    [SerializeField] private TextMeshProUGUI _heroNameText;
    [SerializeField] private TextMeshProUGUI _heroPointsText;
    [SerializeField] private Image _darkenImage;
    [SerializeField] private RectTransform _mainRectTr;

    [Header("References Positions")]
    [SerializeField] private RectTransform _heroNameShownPos;
    [SerializeField] private RectTransform _heroPointsShownPos;



    private void Start()
    {
        _heroNameRectTr.localPosition = Vector3.zero;
        _heroPointsRectTr.localPosition = Vector3.zero;

        _heroNameRectTr.localScale = Vector3.one * 0.7f;
        _heroPointsRectTr.localScale = Vector3.one * 0.7f;

        baseOffset = _mainRectTr.localPosition;

        _darkenImage.color = new Color(0, 0, 0, 0.4f);
    }

    public void ActualiseInfos()
    {
        _heroNameText.text = HeroesManager.Instance.AllHeroes[heroIndex].HeroData.unitName;
        _heroPointsText.text = HeroesManager.Instance.AllHeroes[heroIndex].CurrentSkillTreePoints + " POINTS";
    }

    public void SelectProfile()
    {
        isSelected = true;

        ActualiseInfos();
        HighlightProfile();

        if (_leftProfile) _leftProfile.UnselectProfile(false, heroIndex);
        if (_rightProfile) _rightProfile.UnselectProfile(true, heroIndex);

        if (_mainRectTr.localPosition.x < 0)
        {
            _mainRectTr.DOLocalMove(new Vector3(0, baseOffset.y, baseOffset.z), 0.3f).SetEase(Ease.OutQuint);
        }
        else
        {
            _mainRectTr.DOLocalMove(new Vector3(0, baseOffset.y, baseOffset.z), 0.3f).SetEase(Ease.OutQuint);
        }
    }

    public void UnselectProfile(bool fromLeft, int selectedIndex)
    {
        if (isSelected) HideProfile();
        isSelected = false;

        if (!fromLeft && _leftProfile) _leftProfile.UnselectProfile(false, selectedIndex);
        if (fromLeft && _rightProfile) _rightProfile.UnselectProfile(true, selectedIndex);

        if(heroIndex < selectedIndex)
        {
            _mainRectTr.DOLocalMove(new Vector3(-115 + 100 * (heroIndex - selectedIndex), baseOffset.y, baseOffset.z), 0.3f).SetEase(Ease.OutQuint);
        }
        else
        {
            _mainRectTr.DOLocalMove(new Vector3(115 + 100 * (heroIndex - selectedIndex), baseOffset.y, baseOffset.z), 0.3f).SetEase(Ease.OutQuint);
        }
    }

    // Called when selected
    private void HighlightProfile()
    {
        _darkenImage.DOFade(0f, 0.25f).SetEase(Ease.OutBack);

        _heroNameRectTr.DOLocalMove(_heroNameShownPos.localPosition, 0.3f).SetEase(Ease.OutBack);
        _heroPointsRectTr.DOLocalMove(_heroPointsShownPos.localPosition, 0.3f).SetEase(Ease.OutBack);

        _heroNameRectTr.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        _heroPointsRectTr.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    // Called when unselected
    private void HideProfile()
    {
        _darkenImage.DOFade(0.4f, 0.25f).SetEase(Ease.OutBack);

        _heroNameRectTr.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
        _heroPointsRectTr.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutBack);

        _heroNameRectTr.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutBack);
        _heroPointsRectTr.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutBack);
    }


    #region Mouse Inputs

    public void HoverIcon()
    {
        if (isSelected) return;

        _darkenImage.DOFade(0.1f, 0.25f).SetEase(Ease.OutBack);
    }

    public void UnhoverIcon()
    {
        if (isSelected) return;

        _darkenImage.DOFade(0.4f, 0.25f).SetEase(Ease.OutBack);
    }

    public void ClickIcon()
    {
        OnClick?.Invoke(heroIndex);
        SelectProfile();
    }

    #endregion
}
