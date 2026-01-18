using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class ExpeditionsMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private EnviroData[] enviroDatas;

    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private int currentEnviroIndex;

    [Header("References")]
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private RectTransform[] _buttons;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _shownPositionRectTr;
    [SerializeField] private RectTransform _hiddenPositionRectTr;
    [SerializeField] private TextMeshProUGUI _dangerText;
    [SerializeField] private TextMeshProUGUI _resourcesText;
    [SerializeField] private TextMeshProUGUI _relicText;
    [SerializeField] private TextMeshProUGUI _mainExpeditionName;
    [SerializeField] private Image _mainExpeditionImage;
    [SerializeField] private RectTransform[] _arrowsRectTr;



    private void LoadEnviro(EnviroData enviroData)
    {
        _mainExpeditionName.text = enviroData.enviroName;
        _mainExpeditionImage.sprite = enviroData.enviroIllustration;

        _resourcesText.text = enviroData.enviroResourcesTexts;
        _dangerText.text = enviroData.enviroDangerText;
        _relicText.text =  "RELICS FOUND : " + RelicsManager.Instance.GetAreaCurrentRelicCount(enviroData.enviroIndex) + "/" +
            RelicsManager.Instance.GetAreaAllRelicCount(enviroData.enviroIndex);
    }


    #region Show / Hide

    public void Show()
    {
        OnStartTransition.Invoke();
        OnShow.Invoke();

        LoadEnviro(enviroDatas[currentEnviroIndex]);

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        _mainRectTr.DOMove(_shownPositionRectTr.position, 0.3f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    public void Hide(bool instant)
    {
        if (instant)
        {
            _mainRectTr.position = _hiddenPositionRectTr.position;
            return;
        }

        OnStartTransition?.Invoke();
        OnHide?.Invoke();

        _mainMetaMenu.Show(false);
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.DOMove(_hiddenPositionRectTr.position, 0.3f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    #endregion


    #region Buttons Inputs

    public void HoverButton(int index)
    {
        _buttons[index].UChangeScale(0.2f, Vector3.one * 1.1f, CurveType.EaseOutCubic);
    }

    public void UnHoverButton(int index)
    {
        _buttons[index].UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseInOutSin);
    }

    public void ClickButton(int index)
    {
        if(index == 0)
        {
            GameManager.Instance.StartExploration(enviroDatas[currentEnviroIndex], false);
        }
    }

    #endregion


    #region Arrows Inputs

    public void HoverArrow(int index)
    {
        _arrowsRectTr[index].UChangeScale(0.2f, Vector3.one * 1.2f, CurveType.EaseOutCubic);
    }

    public void UnHoverArrow(int index)
    {
        _arrowsRectTr[index].UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseInOutSin);
    }

    public void ClickArrow(int index)
    {
        if(index == 0)
        {
            currentEnviroIndex--;
            if (currentEnviroIndex < 0) currentEnviroIndex = enviroDatas.Length - 1;
        }
        else
        {
            currentEnviroIndex++;
            if(currentEnviroIndex >= enviroDatas.Length) currentEnviroIndex = 0;
        }

        LoadEnviro(enviroDatas[currentEnviroIndex]);
    }

    #endregion
}
