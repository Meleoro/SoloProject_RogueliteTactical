using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    [Header("Actions")]
    public Action OnTransitionStart;
    public Action OnTransitionEnd;

    [Header("Public Infos")]
    public TextMeshProUGUI StopButtonText { get { return _stopButtonText; } }
    public Image StopButton { get { return _stopButton; } }

    [Header("Private Infos")]
    private EnviroData currentEnviroData;
    private RectTransform[] buttonsRectTr;
    private Coroutine arrowsCoroutine;
    private int currentFloor;

    [Header("References")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private TransitionFloor[] _floorTransitions;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _floorsRectTr;
    [SerializeField] private TextMeshProUGUI _continueButtonText;
    [SerializeField] private TextMeshProUGUI _stopButtonText;
    [SerializeField] private Image _continueButton;
    [SerializeField] private Image _stopButton;
    [SerializeField] private Image[] _arrows;


    private void Start()
    {
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 1);

        buttonsRectTr = new RectTransform[2];
        buttonsRectTr[0] = _continueButtonText.GetComponent<RectTransform>();
        buttonsRectTr[1] = _stopButtonText.GetComponent<RectTransform>();

        _continueButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);

        _mainRectTr.localScale = Vector3.zero;
    }

    public void StartTransition(EnviroData enviroData, int floorIndex, bool instantFade)
    {
        currentEnviroData = enviroData;
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, instantFade ? 1 : 0);

        _mainRectTr.localScale = Vector3.zero;

        if (floorIndex == 0)
        {
            StartCoroutine(IntroCoroutine(2f));
        }
        else
        {
            _continueButton.gameObject.SetActive(true);
            _stopButton.gameObject.SetActive(true);

            StartCoroutine(ChangeFloorCoroutine(floorIndex, 1.75f));
        }
    }

    public void StartDeathTransition()
    {
        OnTransitionStart?.Invoke();

        _mainRectTr.DOScale(Vector3.zero, 0.5f);

        FadeScreen(0.2f, 1f);
    }


    public void FadeScreen(float duration, float endValue)
    {
        _fadeImage.DOFade(endValue, duration);

        if(endValue == 0)
        {
            _continueButton.gameObject.SetActive(false);
            _stopButton.gameObject.SetActive(false);
        }
    }


    #region Effects Coroutines

    private IEnumerator IntroCoroutine(float duration)
    {
        FadeScreen(duration * 0.2f, 1.0f);

        yield return new WaitForSeconds(duration * 0.6f);

        FadeScreen(duration * 0.3f, 0);

        yield return new WaitForSeconds(duration * 0.2f);
    }

    private IEnumerator ChangeFloorCoroutine(int floorIndex, float duration)
    {
        OnTransitionStart?.Invoke();

        HighlightFloor(floorIndex);
        FadeScreen(1, duration * 0.6f);

        yield return new WaitForSeconds(duration * 0.6f);

        _mainRectTr.DOScale(Vector3.one, duration * 0.4f);

        yield return new WaitForSeconds(duration * 0.4f);

        DisplayArrows();

        TutoManager.Instance.DisplayTutorial(12);
    }

    private IEnumerator ContinueCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration * 0.5f);

        _mainRectTr.DOScale(Vector3.zero, duration);

        ChangeFloor(currentFloor + 1, duration * 0.5f);
        HighlightFloor(currentFloor + 1);
        FadeScreen(duration, 0);

        yield return new WaitForSeconds(duration * 0.5f);

        OnTransitionEnd?.Invoke();
    }

    private IEnumerator StopCoroutine(float duration)
    {
        _mainRectTr.DOScale(Vector3.zero, duration);

        yield return new WaitForSeconds(duration);

        _continueButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);

        StartCoroutine(GameManager.Instance.EndExplorationCoroutine());
    }

    #endregion


    #region Floors 

    private void HighlightFloor(int currentFloor)
    {
        this.currentFloor = currentFloor;

        for (int i = 0; i < _floorTransitions.Length; i++)
        {
            if(i == currentFloor)
                _floorTransitions[i].Highlight();

            else
                _floorTransitions[i].Unhighlight();
        }
    }

    private void ChangeFloor(int newFloor, float duration)
    {
        Vector3 dif = _floorTransitions[newFloor].transform.position - _floorTransitions[currentFloor].transform.position;
        _floorsRectTr.DOMove(_floorsRectTr.position + dif, duration).SetEase(Ease.OutBack);
    }

    private void DisplayArrows()
    {
        for (int i = 0; i < _arrows.Length; i++)
        {
            _arrows[i].DOComplete();
            _arrows[i].DOFade(1, 0.5f);
        }

        arrowsCoroutine = StartCoroutine(BounceArrowsCoroutine());
    }

    private IEnumerator BounceArrowsCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            for(int i = 0; i < _arrows.Length; i++)
            {
                _arrows[i].DOFade(1.0f, 0.7f);
            }

            yield return new WaitForSeconds(1.0f);

            for (int i = 0; i < _arrows.Length; i++)
            {
                _arrows[i].DOFade(0.6f, 0.7f);
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    private void HideArrows()
    {
        for (int i = 0; i < _arrows.Length; i++)
        {
            _arrows[i].DOComplete();
            _arrows[i].DOFade(0, 0.2f);
        }
        StopCoroutine(arrowsCoroutine);
    }

    #endregion


    #region Buttons 

    public void HoverButton(int buttonIndex)
    {
        buttonsRectTr[buttonIndex].DOKill();
        buttonsRectTr[buttonIndex].DOScale(Vector3.one * 1.15f, 0.15f).SetEase(Ease.OutCubic);
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

        StartCoroutine(ContinueCoroutine(2f));
    }

    public void ClickStop()
    {
        _continueButton.enabled = false;
        _stopButton.enabled = false;

        StartCoroutine(StopCoroutine(1f));
    }

    #endregion
}
