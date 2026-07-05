using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using static UnityEngine.Rendering.DebugUI;

public class CoinUI : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private bool isGainMoneyFeedback;
    [SerializeField] private Color gainMoneyColorFeedback;
    [SerializeField] private Color baseColor;

    [Header("Private Infos")]
    private int currentCoins;
    private Coroutine gainMoneyCoroutine;
    private Coroutine displayCoroutine;

    [Header("Public Infos")]
    public RectTransform CoinAimedTr { get { return _coinAimedTr; } }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private RectTransform _coinAimedTr;
    [SerializeField] private RectTransform _shownPos;
    [SerializeField] private RectTransform _hiddenPos;
    [SerializeField] private RectTransform _mainTr;


    public void Show()
    {
        _mainTr.DOLocalMove(_shownPos.localPosition, 0.2f).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        _mainTr.DOLocalMove(_hiddenPos.localPosition, 0.2f).SetEase(Ease.InBack);
    }


    public void ActualiseCoins(int value)
    {
        if(value > currentCoins)
        {
            if(gainMoneyCoroutine != null)
            {
                StopCoroutine(gainMoneyCoroutine);
            }

            gainMoneyCoroutine = StartCoroutine(GainMoneyFeedbackCoroutine());
        }

        currentCoins = value;
        _coinText.text = currentCoins.ToString();

        if(isGainMoneyFeedback)
        {
            Show();

            if(displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            displayCoroutine = StartCoroutine(DisplayTemporarilyCoroutine(2.0f));
        }
    }

    private IEnumerator DisplayTemporarilyCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        Hide();
    }

    private IEnumerator GainMoneyFeedbackCoroutine()
    {
        _coinText.DOKill();
        _coinText.DOColor(gainMoneyColorFeedback, 0.1f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.1f);

        _coinText.DOColor(baseColor, 0.2f).SetEase(Ease.OutBack);
    }

    public void NotEnoughCoinsEffect()
    {

    }

    public void UseCoinsEffect(int value)
    {
        int startCoins = currentCoins;
        currentCoins = value;

        _coinText.text = currentCoins.ToString();
    }
}
