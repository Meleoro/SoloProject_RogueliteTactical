using DG.Tweening;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CoinUI : MonoBehaviour
{
    [Header("Private Infos")]
    private int currentCoins;

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
        currentCoins = value;
        _coinText.text = currentCoins.ToString();
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
