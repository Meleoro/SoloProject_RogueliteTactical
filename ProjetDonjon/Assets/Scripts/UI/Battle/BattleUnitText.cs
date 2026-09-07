using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using Utilities;

public class BattleUnitText : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float shakeStrength;
    [SerializeField] private float shakeDuration;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color critColor;
    [SerializeField] private Color healColor;
    [SerializeField] private Color poisonColor;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private RectTransform _mainParent;
    [SerializeField] private TextMeshProUGUI _critText;
    [SerializeField] private TextMeshProUGUI _healText;
    [SerializeField] private TextMeshProUGUI _poisonText;


    public IEnumerator DoDamageTextEffectCoroutine(int damages)
    {
        _critText.enabled = false;
        _healText.enabled = false;
        _poisonText.enabled = false;

        _numberText.text = "-" + damages;
        _numberText.color = normalColor;

        _numberText.rectTransform.DOShakePosition(shakeDuration, shakeStrength, 25);
        _mainParent.DOScale(new Vector3(1.3f, 0.9f, 1.0f), 0.1f).SetEase(Ease.InOutCubic);
        _mainParent.DOLocalMove(_numberText.rectTransform.localPosition + Vector3.up * 40f, 2f).SetEase(Ease.OutCubic);

        _numberText.DOColor(highlightColor, 0.2f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.1f);

        _mainParent.DOScale(new Vector3(0.85f, 1.15f, 1.0f), 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.1f);

        _numberText.DOColor(normalColor, 0.2f).SetEase(Ease.InCubic);

        yield return new WaitForSeconds(0.1f);

        _mainParent.DOScale(new Vector3(1f, 1f, 1f), 0.1f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(1.25f);

        _mainParent.DOScale(new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }

    public IEnumerator DoHealTextEffectCoroutine(int healAmount)
    {
        _critText.enabled = false;
        _healText.enabled = true;
        _poisonText.enabled = false;

        _numberText.text = healAmount.ToString();
        _numberText.color = healColor;

        _mainParent.DOScale(new Vector3(1.3f, 0.9f, 1.0f), 0.1f).SetEase(Ease.InOutCubic);
        _mainParent.DOLocalMove(_numberText.rectTransform.localPosition + Vector3.up * 40f, 2f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.1f);

        _mainParent.DOScale(new Vector3(0.85f, 1.15f, 1.0f), 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.2f);

        _mainParent.DOScale(new Vector3(1f, 1f, 1f), 0.1f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(1.25f);

        _mainParent.DOScale(new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }

    public IEnumerator DoDamageCritTextEffectCoroutine(int damages)
    {
        _critText.enabled = true;
        _healText.enabled = false;
        _poisonText.enabled = false;

        _numberText.text = "-" + damages;
        _numberText.color = critColor;

        _critText.rectTransform.DOShakePosition(shakeDuration, shakeStrength * 0.75f, 30);
        _numberText.rectTransform.DOShakePosition(shakeDuration, shakeStrength * 1.5f, 30);
        _mainParent.DOScale(new Vector3(1.3f, 0.9f, 1.0f), 0.1f).SetEase(Ease.InOutCubic);
        _mainParent.DOLocalMove(_numberText.rectTransform.localPosition + Vector3.up * 40f, 2f).SetEase(Ease.OutCubic);

       // _numberText.DOColor(highlightColor, 0.2f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.1f);

        _mainParent.DOScale(new Vector3(0.85f, 1.15f, 1.0f), 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.1f);

        //_numberText.DOColor(critColor, 0.2f).SetEase(Ease.InCubic);

        yield return new WaitForSeconds(0.1f);

        _mainParent.DOScale(new Vector3(1f, 1f, 1f), 0.1f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(1.25f);

        _mainParent.DOScale(new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }
}
