using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    [Header("Actions")]
    public Action OnHide;

    [Header("Private Infos")]
    private Coroutine currentHeroAnimCoroutine;
    private CampLevelData currentLevelData;

    [Header("References")]
    [SerializeField] private RectTransform _mainParent;
    [SerializeField] private RectTransform _continueRectTr;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _secondaryDescription;
    [SerializeField] private Image _blackFadeImage;
    [SerializeField] private Image _heroImage;


    public void DisplayPopUp(CampLevelData campLevelData)
    {
        currentLevelData = campLevelData;

        _nameText.text = campLevelData.unlockName;
        _descriptionText.text = campLevelData.unlockDescription;

        _mainParent.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.OutBack);

        _blackFadeImage.DOFade(0.6f, 0.25f);
        _blackFadeImage.raycastTarget = true;
    }

    public void DisplayHeroPopUp(CampLevelData campLevelData)
    {
        currentLevelData = campLevelData;

        _nameText.text = campLevelData.unlockName;
        _descriptionText.text = campLevelData.unlockDescription;
        _secondaryDescription.text = campLevelData.unlockAdditionalDescription;

        _mainParent.DOScale(Vector3.one * 0.75f, 0.25f).SetEase(Ease.OutBack);

        _blackFadeImage.DOFade(0.6f, 0.25f);
        _blackFadeImage.raycastTarget = true;

        currentHeroAnimCoroutine = StartCoroutine(PlayHeroAnimCoroutine());
    }

    public void HidePopUp()
    {
        _mainParent.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);

        _blackFadeImage.DOFade(0f, 0.25f);
        _blackFadeImage.raycastTarget = false;

        OnHide?.Invoke();

        if(currentHeroAnimCoroutine != null)
        {
            StopCoroutine(currentHeroAnimCoroutine);
        }
    }
    

    private IEnumerator PlayHeroAnimCoroutine()
    {
        int currentSpriteIndex = 0;

        while(true)
        {
            _heroImage.sprite = currentLevelData.heroAnim[currentSpriteIndex++];

            yield return new WaitForSeconds(0.2f);

            currentSpriteIndex = currentSpriteIndex % currentLevelData.heroAnim.Length;
        }
    }


    #region Player Inputs

    public void HoverContinue()
    {
        _continueRectTr.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnhoverContinue()
    {
        _continueRectTr.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void ClickContinue()
    {
        HidePopUp();
    }

    #endregion
}
