using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

public class MainMetaMenu : MonoBehaviour
{
    [Header("Private Infos")]
    private Coroutine hoverCoroutine;
    private Vector2[] saveLocalPosShadows;
    private int unhoveredIndex = -1;
    private CampLevelData[] campLevels;
    private int currentCampLevel;
    private int currentRelicCount;
    private bool loadedCampProgress;

    [Header("References")]
    [SerializeField] private RectTransform[] _buttonsRectTr;
    [SerializeField] private RectTransform[] _shadowsRectTr;
    [SerializeField] private ExpeditionsMenu _expeditionsMenu;
    [SerializeField] private ChestMenu _chestMenu;
    [SerializeField] private CollectionMenu _collectionMenu;
    [SerializeField] private ShopMenu _shopMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private Transform _globalRectTr;
    [SerializeField] private RectTransform _shownPosRef;
    [SerializeField] private RectTransform _bottomHiddenPos;
    [SerializeField] private RectTransform _leftHiddenPos;
    [SerializeField] private RectTransform _rightHiddenPos;

    [Header("References Camp Lvl")]
    [SerializeField] private Image _campLevelProgressBar;
    [SerializeField] private Image _darkBackground;
    [SerializeField] private TextMeshProUGUI _campLevelProgressText;
    [SerializeField] private TextMeshProUGUI _campLevelMainText;
    [SerializeField] private PopUp _popUp;
    [SerializeField] private Lock _shopLock;
    [SerializeField] private Lock _smithLock;



    private void Start()
    {
        saveLocalPosShadows = new Vector2[_shadowsRectTr.Length];

        for (int i = 0; i < _shadowsRectTr.Length; i++)
        {
            saveLocalPosShadows[i] = _shadowsRectTr[i].localPosition;
        }
    }



    #region Show / Hide

    public void Hide(Vector3 hidePos)
    {
        StartCoroutine(HideCoroutine(hidePos));
    }

    private IEnumerator HideCoroutine(Vector3 hidePos)
    {
        Vector3 dir = _mainRectTr.position - hidePos;

        //_mainRectTr.DOMove(_mainRectTr.position + dir.normalized * 0.4f, 0.3f, false).SetEase(Ease.InOutSine);
        //_globalRectTr.DOScale(Vector3.one * 0.9f, 0.4f).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.3f);

        //_mainRectTr.DOMove(hidePos - dir.normalized * 0.4f, 0.3f, false).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.15f);

        //_globalRectTr.DOScale(Vector3.one * 1f, 0.4f).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.15f);

        _mainRectTr.DOMove(hidePos, 0.3f, false).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.15f);
    }


    public void Show(bool instant)
    {
        if (instant)
        {
            _mainRectTr.localPosition = Vector3.zero;
            return;
        }

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        Vector3 dir = _mainRectTr.position - _shownPosRef.position;

        //_mainRectTr.DOMove(_mainRectTr.position + dir.normalized * 0.4f, 0.3f, false).SetEase(Ease.InOutSine);
        //_globalRectTr.DOScale(Vector3.one * 0.9f, 0.4f).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.3f);

        //_mainRectTr.DOLocalMove(Vector3.zero - _mainRectTr.parent.InverseTransformVector(dir.normalized * 0.4f), 0.3f, false).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.15f);

        //_globalRectTr.DOScale(Vector3.one * 1f, 0.4f).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.15f);

        _mainRectTr.DOLocalMove(Vector3.zero, 0.3f, false).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.15f);
    }

    #endregion


    #region Open Menus

    public void ClickExpeditions()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        Hide(_bottomHiddenPos.position);
        _expeditionsMenu.Show();
    }

    public void ClickChest()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        Hide(_leftHiddenPos.position);
        _chestMenu.Show();
    }

    public void ClickTreasures()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        _collectionMenu.Show();

    }

    public void ClickShop()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        Hide(_rightHiddenPos.position);
        _shopMenu.Show();
    }

    public void ClickSmith()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

    }

    #endregion


    #region Buttons Hover

    public void HoverButton(int index)
    {
        if(hoverCoroutine != null) { 
            StopCoroutine(hoverCoroutine);
        }

        _buttonsRectTr[index].DOComplete();
        hoverCoroutine = StartCoroutine(HoverButtonCoroutine(index));
    }

    private IEnumerator HoverButtonCoroutine(int index)
    {
        //_buttonsRectTr[index].DOScale(Vector3.one * 1.3f, 0.1f).SetEase(Ease.InOutSine);

        //yield return new WaitForSeconds(0.1f);

        _buttonsRectTr[index].DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.4f);

        while(true)
        {
            _buttonsRectTr[index].DOScale(Vector3.one * 1.15f, 0.85f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1f);

            _buttonsRectTr[index].DOScale(Vector3.one * 1.1f, 0.85f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1f);
        }
    }

    public void UnHoverButton(int index)
    {
        if (unhoveredIndex == index) return;

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        _buttonsRectTr[index].DOComplete();
        _buttonsRectTr[index].DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.OutCubic);
    }

    #endregion


    #region Camps Level
    
    // Called when the relic count is loaded from the save
    public void LoadCampProgress()
    {
        loadedCampProgress = true;

        campLevels = RelicsManager.Instance.CampLevels;
        currentCampLevel = RelicsManager.Instance.CurrentCampLevel;
        currentRelicCount = RelicsManager.Instance.CurrentCampRelicCount;

        if (RelicsManager.Instance.VerifyHasCampUpgrade(CampLevelData.CampUnlockType.Shop))
        {
            _shopLock.Unlock(true);
        }

        if (RelicsManager.Instance.VerifyHasCampUpgrade(CampLevelData.CampUnlockType.Blacksmith))
        {
            _smithLock.Unlock(true);
        }

        if (currentCampLevel == campLevels.Length) return;

        _campLevelProgressBar.fillAmount = currentRelicCount / campLevels[currentCampLevel].neededRelicCount;
        _campLevelProgressText.text = currentRelicCount + "/" + campLevels[currentCampLevel].neededRelicCount;
        _campLevelMainText.text = "CAMP LEVEL " + currentCampLevel;
    }

    public void ActualiseCampProgress()
    {
        if (!loadedCampProgress) return;
        if (campLevels == null) return;

        int previousCampLevel = RelicsManager.Instance.CurrentCampLevel;
        int previousRelicCount = RelicsManager.Instance.CurrentCampRelicCount;

        if(previousCampLevel == 0)
        {
            _campLevelProgressBar.fillAmount = previousRelicCount / campLevels[previousCampLevel].neededRelicCount;
        }
        else
        {
            _campLevelProgressBar.fillAmount = previousRelicCount - campLevels[previousCampLevel - 1].neededRelicCount / 
                campLevels[previousCampLevel - 1].neededRelicCount;
        }
        _campLevelProgressText.text = previousRelicCount + "/" + campLevels[previousCampLevel].neededRelicCount;
        _campLevelMainText.text = "CAMP LEVEL " + previousCampLevel;

        RelicsManager.Instance.ActualiseCampLevel();

        currentRelicCount = RelicsManager.Instance.CurrentCampRelicCount;

        if(currentRelicCount != previousRelicCount)
        {
            _darkBackground.DOFade(0.6f, 0.2f);
            _darkBackground.raycastTarget = true;

            if(previousCampLevel == 0)
                StartCoroutine(PlayProgressCoroutine((float)currentRelicCount / campLevels[previousCampLevel].neededRelicCount));

            else
                StartCoroutine(PlayProgressCoroutine((float)currentRelicCount - campLevels[previousCampLevel - 1].neededRelicCount / 
                    campLevels[previousCampLevel - 1].neededRelicCount));
        }
    }

    private IEnumerator PlayProgressCoroutine(float endProgress)
    {
        _campLevelProgressBar.DOFillAmount(endProgress, 0.5f);
        _campLevelProgressText.text = Mathf.Clamp(currentRelicCount, 0, campLevels[currentCampLevel].neededRelicCount) 
            + "/" + campLevels[currentCampLevel].neededRelicCount;

        yield return new WaitForSeconds(0.5f);

        // If we gain no new level
        if(endProgress < 1)
        {
            _darkBackground.DOFade(0f, 0.2f);
            _darkBackground.raycastTarget = false;

            yield break;
        }

        // If we gain a new level
        _campLevelProgressBar.fillAmount = 0;
        _campLevelMainText.text = "CAMP LEVEL " + (currentCampLevel + 1);
        _campLevelProgressText.text = campLevels[currentCampLevel].neededRelicCount + "/" + campLevels[currentCampLevel].neededRelicCount;

        _popUp.DisplayPopUp(campLevels[currentCampLevel]);
        _popUp.OnHide += ContinueProgress;

        RelicsManager.Instance.CampLevelUp();

        if (campLevels[currentCampLevel].unlockType == CampLevelData.CampUnlockType.Shop)
        {
            _shopLock.Unlock(false);
        }

        else if (campLevels[currentCampLevel].unlockType == CampLevelData.CampUnlockType.Blacksmith)
        {
            _smithLock.Unlock(false);
        }

        currentCampLevel++;
    }

    public void ContinueProgress()
    {
        _popUp.OnHide -= ContinueProgress;

        _campLevelProgressText.text = campLevels[currentCampLevel - 1].neededRelicCount + "/" + campLevels[currentCampLevel].neededRelicCount;
        _campLevelMainText.text = "CAMP LEVEL " + currentCampLevel;

        if(currentRelicCount > campLevels[currentCampLevel - 1].neededRelicCount)
        {
            StartCoroutine(PlayProgressCoroutine((float)(currentRelicCount - campLevels[currentCampLevel - 1].neededRelicCount) / 
                (campLevels[currentCampLevel].neededRelicCount - campLevels[currentCampLevel - 1].neededRelicCount)));
            return;
        }

        _darkBackground.DOFade(0f, 0.2f);
        _darkBackground.raycastTarget = false;
    }

    #endregion
}
