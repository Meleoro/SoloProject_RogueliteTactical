using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorTransition : MonoBehaviour
{
    [Header("Actions")]
    public Action OnTransitionStart;
    public Action OnTransitionEnd;

    [Header("Public Infos")]
    public TextMeshProUGUI StopButtonText { get { return _stopButtonText; } }
    public Button StopButton { get { return _stopButton; } }

    [Header("Private Infos")]
    private EnviroData currentEnviroData;
    private RectTransform[] buttonsRectTr;

    [Header("References")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _flootCounterText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelCounterText;
    [SerializeField] private TextMeshProUGUI _continueButtonText;
    [SerializeField] private TextMeshProUGUI _stopButtonText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _stopButton;

    [Header("Death Reference")]
    [SerializeField] private TextMeshProUGUI _mainDeathText;
    [SerializeField] private TextMeshProUGUI _secondaryDeathText;
    [SerializeField] private TextMeshProUGUI _backToCampText;
    [SerializeField] private Button _backToCampButton;


    private void Start()
    {
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 1);

        buttonsRectTr = new RectTransform[3];
        buttonsRectTr[0] = _continueButtonText.GetComponent<RectTransform>();
        buttonsRectTr[1] = _stopButtonText.GetComponent<RectTransform>();
        buttonsRectTr[2] = _backToCampText.GetComponent<RectTransform>();

        _recommandedLevelText.gameObject.SetActive(false);
        _recommandedLevelCounterText.gameObject.SetActive(false);

        _backToCampText.gameObject.SetActive(false);
        _mainDeathText.gameObject.SetActive(false);
        _secondaryDeathText.gameObject.SetActive(false);

        _floorText.gameObject.SetActive(false);
        _flootCounterText.gameObject.SetActive(false);

        _continueButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);
        _backToCampButton.gameObject.SetActive(false);
    }

    public void StartTransition(EnviroData enviroData, int floorIndex)
    {
        currentEnviroData = enviroData;

        _backToCampText.gameObject.SetActive(false);
        _mainDeathText.gameObject.SetActive(false);
        _secondaryDeathText.gameObject.SetActive(false);

        _floorText.gameObject.SetActive(true);
        _flootCounterText.gameObject.SetActive(true);

        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0);

        if (floorIndex == 0)
        {
            _recommandedLevelText.gameObject.SetActive(false);
            _recommandedLevelCounterText.gameObject.SetActive(false);

            _continueButton.gameObject.SetActive(false);
            _stopButton.gameObject.SetActive(false);
            _backToCampButton.gameObject.SetActive(false);

            _flootCounterText.text = (floorIndex + 1).ToString();

            StartCoroutine(IntroCoroutine(2f));
        }
        else
        {
            _recommandedLevelText.gameObject.SetActive(true);
            _recommandedLevelCounterText.gameObject.SetActive(true);

            _recommandedLevelText.color = new Color(_recommandedLevelText.color.r, _recommandedLevelText.color.g, _recommandedLevelText.color.b, 0);
            _recommandedLevelCounterText.color = new Color(_recommandedLevelText.color.r, _recommandedLevelText.color.g, _recommandedLevelText.color.b, 0);

            _continueButton.gameObject.SetActive(true);
            _stopButton.gameObject.SetActive(true);
            _backToCampButton.gameObject.SetActive(false);

            _continueButtonText.color = new Color(_continueButtonText.color.r, _continueButtonText.color.g, _continueButtonText.color.b, 0);
            _stopButtonText.color = new Color(_continueButtonText.color.r, _continueButtonText.color.g, _continueButtonText.color.b, 0);

            StartCoroutine(ChangeFloorCoroutine(floorIndex, 3.5f));
        }
    }

    public void StartDeathTransition()
    {
        OnTransitionStart?.Invoke();

        _recommandedLevelText.gameObject.SetActive(false);
        _recommandedLevelCounterText.gameObject.SetActive(false);

        _continueButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);
        _backToCampButton.gameObject.SetActive(true);

        _backToCampText.gameObject.SetActive(true);
        _mainDeathText.gameObject.SetActive(true);
        _secondaryDeathText.gameObject.SetActive(true);

        FadeScreen(0.2f, 1f);

        _backToCampText.DOFade(1, 0.2f);
        _mainDeathText.DOFade(1, 0.2f);
        _secondaryDeathText.DOFade(1, 0.2f);
    }


    public void FadeScreen(float duration, float endValue)
    {
        _fadeImage.DOFade(endValue, duration);

        if(endValue == 0)
        {
            _continueButton.gameObject.SetActive(false);
            _stopButton.gameObject.SetActive(false);
            _backToCampButton.gameObject.SetActive(false);
        }
    }



    #region Effects Coroutines

    private IEnumerator IntroCoroutine(float duration)
    {
        _floorText.DOFade(1, duration * 0.2f);
        _flootCounterText.DOFade(1, duration * 0.2f);
        FadeScreen(duration * 0.2f, 1.0f);

        yield return new WaitForSeconds(duration * 0.6f);

        FadeScreen(duration * 0.3f, 0);

        yield return new WaitForSeconds(duration * 0.2f);

        _floorText.DOFade(0, duration * 0.2f);
        _flootCounterText.DOFade(0, duration * 0.2f);
    }

    private IEnumerator ChangeFloorCoroutine(int floorIndex, float duration)
    {
        OnTransitionStart?.Invoke();

        FadeScreen(1, duration * 0.3f);

        yield return new WaitForSeconds(duration * 0.3f);

        _floorText.DOFade(1, duration * 0.2f);
        _flootCounterText.DOFade(1, duration * 0.2f);

        yield return new WaitForSeconds(duration * 0.2f);

        TutoManager.Instance.DisplayTutorial(12);

        Color saveColor = _flootCounterText.color;
        _flootCounterText.rectTransform.DOScale(Vector3.one * 1.4f, duration * 0.05f);
        _flootCounterText.DOColor(Color.white, duration * 0.05f);

        yield return new WaitForSeconds(duration * 0.05f);

        _flootCounterText.rectTransform.DOScale(Vector3.one, duration * 0.15f);
        _flootCounterText.DOColor(saveColor, duration * 0.15f);

        _recommandedLevelText.DOFade(1, duration * 0.2f);
        _recommandedLevelCounterText.DOFade(1, duration * 0.2f);

        _continueButtonText.DOFade(1, duration * 0.2f);
        _stopButtonText.DOFade(1, duration * 0.2f);

        if (floorIndex == currentEnviroData.recommandedLevels.Length) yield break;

        _recommandedLevelCounterText.text = currentEnviroData.recommandedLevels[floorIndex].ToString();
        _flootCounterText.text = (floorIndex + 1).ToString();
    }

    private IEnumerator ContinueCoroutine(float duration)
    {
        _floorText.DOFade(0, duration);
        _flootCounterText.DOFade(0, duration);

        _recommandedLevelText.DOFade(0, duration);
        _recommandedLevelCounterText.DOFade(0, duration);

        _continueButtonText.DOFade(0, duration);
        _stopButtonText.DOFade(0, duration);

        FadeScreen(duration, 0);

        yield return new WaitForSeconds(duration);

        OnTransitionEnd?.Invoke();
    }

    private IEnumerator StopCoroutine(float duration)
    {
        _floorText.DOFade(0, duration);
        _flootCounterText.DOFade(0, duration);

        _recommandedLevelText.DOFade(0, duration);
        _recommandedLevelCounterText.DOFade(0, duration);

        _continueButtonText.DOFade(0, duration);
        _stopButtonText.DOFade(0, duration);

        _backToCampText.DOFade(0, duration);
        _mainDeathText.DOFade(0, duration);
        _secondaryDeathText.DOFade(0, duration);

        yield return new WaitForSeconds(duration);

        _continueButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);

        StartCoroutine(GameManager.Instance.EndExplorationCoroutine());
    }

    #endregion


    #region Buttons 

    public void HoverButton(int buttonIndex)
    {
        buttonsRectTr[buttonIndex].DOKill();
        buttonsRectTr[buttonIndex].DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutCubic);
    }

    public void UnhoverButton(int buttonIndex)
    {
        buttonsRectTr[buttonIndex].DOKill();
        buttonsRectTr[buttonIndex].DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.InCubic);
    }

    public void ClickContinue()
    {
        _continueButton.enabled = false;
        _stopButton.enabled = false;
        _backToCampButton.enabled = false;

        StartCoroutine(ContinueCoroutine(1f));
    }

    public void ClickStop()
    {
        _continueButton.enabled = false;
        _stopButton.enabled = false;
        _backToCampButton.enabled = false;

        StartCoroutine(StopCoroutine(1f));
    }

    #endregion
}
