using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class AdditionalTooltip : MonoBehaviour
{
    [Header("Private Infos")]
    private Coroutine showCoroutine;
    private Vector3 saveOffset;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Appear Effect References")]
    [SerializeField] private Image[] _imagesToFade;
    [SerializeField] private TextMeshProUGUI[] _textsToFade;


    private void Start()
    {
        saveOffset = _mainRectTr.localPosition;
        Hide();
    }

    public void Show(AdditionalTooltipData data, float delay, bool mirrorVertical = false) 
    {
        _nameText.text = data.tooltipName;
        _descriptionText.text = data.tooltipDescription;

        if(mirrorVertical)
        {
            _mainRectTr.localPosition = new Vector3(saveOffset.x, -1 * saveOffset.y, 1 * saveOffset.z);
        }

        if(showCoroutine != null) StopCoroutine(showCoroutine);

        showCoroutine = StartCoroutine(ShowCoroutine(delay, mirrorVertical));
    }

    private IEnumerator ShowCoroutine(float delay, bool mirrorVertical)
    {
        _mainRectTr.localScale = Vector3.zero;

        yield return new WaitForSeconds(delay);

        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(_nameText.text, _nameText, 0.15f));
        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(_descriptionText.text, _descriptionText, 0.35f));

        for (int i = 0; i < _imagesToFade.Length; i++)
        {
            _imagesToFade[i].color = _imagesToFade[i].color * new Color(1, 1, 1, 0);
            _imagesToFade[i].DOFade(1, 0.35f).SetEase(Ease.OutSine);
        }
        for (int i = 0; i < _textsToFade.Length; i++)
        {
            _textsToFade[i].color = _textsToFade[i].color * new Color(1, 1, 1, 0);
            _textsToFade[i].DOFade(1, 0.35f).SetEase(Ease.OutSine);
        }

        _mainRectTr.localScale = Vector3.one;

        if (mirrorVertical)
        {
            _mainRectTr.localPosition = new Vector3(saveOffset.x, -1 * saveOffset.y, 1 * saveOffset.z) + new Vector3(0, 80f, 0);
            _mainRectTr.DOLocalMove(new Vector3(saveOffset.x, -1 * saveOffset.y, 1 * saveOffset.z), 0.25f).SetEase(Ease.OutBack);
        }
        else
        {
            _mainRectTr.localPosition = saveOffset + new Vector3(0, -80f, 0);
            _mainRectTr.DOLocalMove(saveOffset, 0.25f).SetEase(Ease.OutBack);
        }
    }


    public void Hide()
    {
        if (showCoroutine is not null) StopCoroutine(showCoroutine);
        _mainRectTr.DOComplete();

        _mainRectTr.localScale = Vector3.zero;
    }
}
