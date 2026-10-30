using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color buttonTextColorHover;
    [SerializeField] private Color buttonTextColorUnhover;
    [SerializeField] private Color buttonColorHover;
    [SerializeField] private Color buttonColorUnhover;

    [Header("Actions")]
    public Action OnTransitionStart;
    public Action OnTransitionEnd;

    [Header("Public Infos")]
    public TextMeshProUGUI StopButtonText { get { return _buttonsTexts[1]; } }
    public Image StopButton { get { return _buttonsImages[1] ; } }
    public bool IsActive { get { return isActive; } }

    [Header("Private Infos")]
    private EnviroData currentEnviroData;
    private int currentFloor;
    private bool isOnLastFloor;
    private bool isActive = false;
    private Vector3 saveFloorParentOffset;
    private Vector3 saveButtonScale;

    [Header("References")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private TransitionFloor[] _floorTransitions;
    [SerializeField] private TextMeshProUGUI[] _statsTexts;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _floorsRectTr;
    [SerializeField] private RectTransform[] _buttonsRectTr;
    [SerializeField] private Image[] _buttonsImages;
    [SerializeField] private Image[] _buttonsOutline;
    [SerializeField] private TextMeshProUGUI[] _buttonsTexts;
    [SerializeField] private Animator _anim;
    [SerializeField] private TextMeshProUGUI _introAreaNameText;
    [SerializeField] private TextMeshProUGUI _introFloorNameText;


    private void Start()
    {
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 1);

        for(int i = 0; i < _buttonsImages.Length; i++)
        {
            _buttonsImages[i].gameObject.SetActive(false);
        }

        _introAreaNameText.DOFade(0, 0);
        _introFloorNameText.DOFade(0, 0);
    }

    public void StartTransition(EnviroData enviroData, int floorIndex, bool instantFade)
    {
        currentEnviroData = enviroData;
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, instantFade ? 1 : 0);

        for (int i = 0; i < _buttonsImages.Length; i++)
        {
            _buttonsImages[i].raycastTarget = true;
        }

        _introAreaNameText.color = new Color(enviroData.areaNameTextColor.r, enviroData.areaNameTextColor.g, enviroData.areaNameTextColor.b, 0);
        _introFloorNameText.color = new Color(enviroData.areaNameTextColor.r, enviroData.areaNameTextColor.g, enviroData.areaNameTextColor.b, 0);

        if (floorIndex == enviroData.floorsAmount) isOnLastFloor = true;
        else isOnLastFloor = false;

        if (floorIndex == 0)
        {
            StartCoroutine(IntroCoroutine(2f));
            ResetFloors();
        }
        else
        {
            InputManager.DisableInputs = true;

            ActualiseFloorsTexts(enviroData);

            _statsTexts[0].text = " " + GameManager.Instance.ExpeditionCoinsCount;
            _statsTexts[1].text = " " + GameManager.Instance.ExpeditionKillCount;
            _statsTexts[2].text = " " + GameManager.Instance.ExpeditionChestCount;
            _statsTexts[3].text = " " + GameManager.Instance.ExpeditionRelicCount;

            if(!isOnLastFloor) _buttonsImages[0].gameObject.SetActive(true);   // Continue button
            else _buttonsImages[0].gameObject.SetActive(false);   // Continue button

            _buttonsImages[1].gameObject.SetActive(true);    // Stop button

            StartCoroutine(DisplayFloorTransitionCoroutine(floorIndex, 1.75f));
        }
    }

    public void StartDeathTransition()
    {
        OnTransitionStart?.Invoke();

        InputManager.DisableInputs = true;

        for (int i = 0; i < _buttonsImages.Length; i++)
        {
            _buttonsImages[i].raycastTarget = true;
        }

        _buttonsImages[0].gameObject.SetActive(false);   // Continue button
        _buttonsImages[1].gameObject.SetActive(true);    // Stop button

        ActualiseFloorsTexts(currentEnviroData);

        _statsTexts[0].text = " " + GameManager.Instance.ExpeditionCoinsCount;
        _statsTexts[1].text = " " + GameManager.Instance.ExpeditionKillCount;
        _statsTexts[2].text = " " + GameManager.Instance.ExpeditionRelicCount;
        _statsTexts[3].text = " " + GameManager.Instance.ExpeditionChestCount;

        StartCoroutine(StartDeathCoroutine());
    }


    public void FadeScreen(float duration, float endValue)
    {
        _fadeImage.DOFade(endValue, duration);

        if(endValue == 0)
        {
            for (int i = 0; i < _buttonsImages.Length; i++)
            {
                _buttonsImages[i].gameObject.SetActive(false);
            }
        }
    }


    private void ResetFloors()
    {
        if(saveFloorParentOffset != Vector3.zero)
        {
            _floorsRectTr.localPosition = saveFloorParentOffset;
        }

        for (int i = 0; i < _floorTransitions.Length; i++)
        {
            _floorTransitions[i].ResetVisuals();
        }
    }


    #region Effects Coroutines

    private IEnumerator IntroCoroutine(float duration)
    {
        FadeScreen(duration * 0.2f, 1.0f);

        yield return new WaitForSeconds(duration * 0.6f);

        StartCoroutine(CameraManager.Instance.DoIntroMovementCoroutine(duration * 0.4f));
        FadeScreen(duration * 0.3f, 0);

        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(currentEnviroData.enviroName.ToUpper(), _introAreaNameText, 0.2f));
        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine("FLOOR 1", _introFloorNameText, 0.2f));

        _anim.SetTrigger("DoIntro");

        yield return new WaitForSeconds(duration * 0.4f);
    }

    private IEnumerator DisplayFloorTransitionCoroutine(int floorIndex, float duration)
    {
        OnTransitionStart?.Invoke();

        HighlightFloor(floorIndex - 1);
        FadeScreen(1, duration * 0.6f);

        yield return new WaitForSeconds(duration * 0.65f);

        _anim.SetBool("IsDisplayed", true);
        isActive = true;

        StartCoroutine(TutoManager.Instance.DisplayTutorialWithDelayCoroutine(11, 1.75f));

        yield return new WaitForSeconds(duration * 0.35f);

        StartCoroutine(_floorTransitions[floorIndex - 1].DoClearedEffectCoroutine());
        if(!isOnLastFloor) _floorTransitions[floorIndex].StartNextEffect();

        InputManager.DisableInputs = false;

        UIManager.Instance.HUDExploration.Show();
    }

    private IEnumerator StartDeathCoroutine()
    {
        OnTransitionStart?.Invoke();

        HighlightFloor(currentFloor);
        FadeScreen(1, 1.0f);

        yield return new WaitForSeconds(1.0f);

        _anim.SetBool("IsDisplayed", true);
        isActive = true;

        yield return new WaitForSeconds(0.75f);

        StartCoroutine(_floorTransitions[currentFloor].DoDeadEffectCoroutine());

        InputManager.DisableInputs = false;
    }

    private IEnumerator ContinueCoroutine(float duration)
    {
        ChangeFloor(currentFloor + 1, duration * 0.3f);
        _floorTransitions[currentFloor + 1].EndNextEffect();

        HighlightFloor(currentFloor + 1);

        yield return new WaitForSeconds(duration * 0.4f);

        _anim.SetBool("IsDisplayed", false);
        isActive = false;
        UIManager.Instance.HUDExploration.Hide();

        yield return new WaitForSeconds(duration * 0.35f);

        ProceduralGenerationManager.Instance.GenerateNextFloor();

        yield return new WaitForSeconds(duration * 0.05f);

        FadeScreen(duration * 0.4f, 0);

        yield return new WaitForSeconds(duration * 0.05f);

        StartCoroutine(CameraManager.Instance.DoIntroMovementCoroutine(duration * 0.4f));

        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(currentEnviroData.enviroName.ToUpper(), _introAreaNameText, 0.2f));
        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine("FLOOR " + (currentFloor+1), _introFloorNameText, 0.2f));

        _anim.SetTrigger("DoIntro");

        yield return new WaitForSeconds(duration * 0.5f);

        OnTransitionEnd?.Invoke();
    }

    private IEnumerator StopCoroutine(float duration)
    {
        _floorTransitions[currentFloor + 1].EndNextEffect();
        _anim.SetBool("IsDisplayed", false);
        isActive = false;
        UIManager.Instance.HUDExploration.Hide();

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < _buttonsImages.Length; i++)
        {
            _buttonsImages[i].gameObject.SetActive(false);
        }

        StartCoroutine(GameManager.Instance.EndExplorationCoroutine());
    }

    #endregion


    #region Floors 

    private void ActualiseFloorsTexts(EnviroData data)
    {
        for (int i = 0; i < _floorTransitions.Length; i++)
        {
            _floorTransitions[i].Initialise(data, i);
        }
    }

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
        if (saveFloorParentOffset == Vector3.zero)
        {
            saveFloorParentOffset = _floorsRectTr.localPosition;
        }

        Vector3 dif = _floorTransitions[newFloor].transform.localPosition - _floorTransitions[currentFloor].transform.localPosition;
        _floorsRectTr.DOLocalMove(_floorsRectTr.localPosition - dif, duration).SetEase(Ease.OutBack);

        _floorTransitions[newFloor].DoEnterFloorEffects();

        _floorTransitions[newFloor].Highlight();
        _floorTransitions[currentFloor].Unhighlight();
    }

    #endregion


    #region Buttons 

    public void HoverButton(int buttonIndex)
    {
        _buttonsRectTr[buttonIndex].DOComplete();
        _buttonsImages[buttonIndex].DOComplete();
        _buttonsTexts[buttonIndex].DOComplete();

        saveButtonScale = _buttonsRectTr[buttonIndex].localScale;

        _buttonsRectTr[buttonIndex].DOScale(saveButtonScale * 1.05f, 0.15f).SetEase(Ease.OutBack);
        //_buttonsImages[buttonIndex].DOColor(buttonColorHover, 0.15f).SetEase(Ease.OutCubic);
        //_buttonsTexts[buttonIndex].DOColor(buttonTextColorHover, 0.15f).SetEase(Ease.OutCubic);

        _buttonsOutline[buttonIndex].DOFade(1, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnhoverButton(int buttonIndex)
    {
        _buttonsRectTr[buttonIndex].DOKill();
        _buttonsImages[buttonIndex].DOKill();
        _buttonsTexts[buttonIndex].DOKill();

        _buttonsRectTr[buttonIndex].DOScale(saveButtonScale, 0.15f).SetEase(Ease.InCubic);
        //_buttonsImages[buttonIndex].DOColor(buttonColorUnhover, 0.15f).SetEase(Ease.OutCubic);
        //_buttonsTexts[buttonIndex].DOColor(buttonTextColorUnhover, 0.15f).SetEase(Ease.OutCubic);

        _buttonsOutline[buttonIndex].DOFade(0, 0.2f).SetEase(Ease.OutBack);
    }

    public void ClickContinue()
    {
        for (int i = 0; i < _buttonsImages.Length; i++)
        {
            _buttonsImages[i].raycastTarget = false;
        }

        StartCoroutine(ContinueCoroutine(3.2f));
    }

    public void ClickStop()
    {
        for (int i = 0; i < _buttonsImages.Length; i++)
        {
            _buttonsImages[i].raycastTarget = false;
        }

        StartCoroutine(StopCoroutine(0.3f));
    }

    #endregion
}
