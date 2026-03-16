using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionFloor : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color highlightFadeColor;
    [SerializeField] private Color unhighlightFadeColor;

    [Header("Private Infos")]
    private EnviroData data;
    private int floorIndex;
    private Coroutine nextCoroutine;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelText;
    [SerializeField] private TextMeshProUGUI _clearedText;
    [SerializeField] private TextMeshProUGUI _nextText;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _bossIcon;


    public void Initialise(EnviroData data, int floorIndex)
    {
        this.data = data;
        this.floorIndex = floorIndex;

        _floorText.text = "FLOOR " + (floorIndex + 1);
        _recommandedLevelText.text = "REQ LEVEL : " + data.recommandedLevels[floorIndex];

        if(floorIndex == 2 || floorIndex == 5) _bossIcon.enabled = true;
        else _bossIcon.enabled = false;
        _bossIcon.enabled = false;

        _fadeImage.color = unhighlightFadeColor;
    }


    public void Highlight()
    {
        _fadeImage.DOColor(highlightFadeColor, 0.4f);
    }

    public void Unhighlight()
    {
        _fadeImage.DOColor(unhighlightFadeColor, 0.4f);
    }


    public IEnumerator DoClearedEffectCoroutine()
    {
        _clearedText.rectTransform.localScale = new Vector3(2, 2, 2);

        _clearedText.DOFade(1, 0.35f).SetEase(Ease.InOutCubic);
        _clearedText.rectTransform.DOScale(1f, 0.45f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.35f);

        _clearedText.DOFade(0.5f, 0.3f).SetEase(Ease.InOutCubic);
    }


    public void StartNextEffect()
    {
        nextCoroutine = StartCoroutine(DoNextEffectCoroutine());
    }

    public void EndNextEffect()
    {
        if(nextCoroutine != null)
            StopCoroutine(nextCoroutine);

        _nextText.DOKill();
        _nextText.DOFade(0f, 0.5f).SetEase(Ease.InOutSine);
    }

    private IEnumerator DoNextEffectCoroutine()
    {
        yield return new WaitForSeconds(1.2f);

        while (true)
        {
            _nextText.DOFade(0.5f, 1).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1.2f);

            _nextText.DOFade(0f, 1).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1.2f);
        }
    }
}
