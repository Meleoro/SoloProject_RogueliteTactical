using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class CollectionMenu : MonoBehaviour
{


    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private bool[] possessedRelics;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private CollectionRelic[] _collectionRelics;



    private void Start()
    {
        RelicsManager.Instance.OnRelicObtained += AddNewRelic;
    }


    public void LoadPage(int index)
    {
        int startRelicIndex = index * 12;

        for(int i = startRelicIndex; i < startRelicIndex + 12; i++)
        {
            if (i >= RelicsManager.Instance.AllRelics.Length) _collectionRelics[i - startRelicIndex].Setup(null, false);
            else _collectionRelics[i - startRelicIndex].Setup(RelicsManager.Instance.AllRelics[i], possessedRelics[i]);
        }
    }


    #region Add New Relic

    public void AddNewRelic(RelicData relicData, int relicIndex)
    {
        Show(relicIndex / 12);

        StartCoroutine(AddNewRelicCoroutine(relicData, relicIndex));
    }

    private IEnumerator AddNewRelicCoroutine(RelicData relicData, int relicIndex)
    {
        yield return new WaitForSeconds(0.4f);

        int pageIndex = (relicIndex / 12);
        int pageRelicIndex = relicIndex - (pageIndex * 12);

        StartCoroutine(_collectionRelics[pageRelicIndex].NewRelicEffectCoroutine());

        yield return new WaitForSeconds(0.3f);
    }

    #endregion


    #region Open / Close

    public void Show(int startPage = 0)
    {
        OnStartTransition?.Invoke();
        OnShow?.Invoke();

        if (GameManager.Instance.IsInExplo)
            HeroesManager.Instance.StopControl();

        _mainRectTr.gameObject.SetActive(true);
        _backgroundImage.DOFade(0.5f, 0.3f);

        possessedRelics = RelicsManager.Instance.PossessedRelicIndexes;
        LoadPage(startPage);

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        _mainRectTr.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition?.Invoke();
    }


    public void Hide()
    {
        OnStartTransition?.Invoke();
        OnHide?.Invoke();

        if(GameManager.Instance.IsInExplo)
            HeroesManager.Instance.RestartControl();

        _backgroundImage.DOFade(0f, 0.3f);

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.3f);

        _mainRectTr.gameObject.SetActive(false);

        OnEndTransition?.Invoke();
    }

    #endregion
}
