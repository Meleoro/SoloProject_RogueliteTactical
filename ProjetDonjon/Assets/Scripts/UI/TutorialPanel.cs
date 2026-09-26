using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color highlightActionPointColor;
    [SerializeField] private Color noHighlightActionPointColor;
    [SerializeField] private Color highlightSkillPointColor;
    [SerializeField] private Color noHighlightSkillPointColor;

    [Header("Actions")]
    public Action OnHide;

    [Header("Public Infos")]
    public RectTransform RectTr { get { return _rectTr; } }

    [Header("Private Infos")]
    private TutoStepData currentTutoData;
    private Image[] highlightedImages = new Image[0];
    private Transform originalParent;
    private Vector3[] originalPositions;
    private Vector3[] originalLocalPositions;
    private Vector3 startPosSave;
    private bool isDisplayingTileHighlight;
    private bool isDisplayingSkillHighlight;
    private bool maintainPosition;
    private bool isDisplayed;
    private float currentProgress;
    private Coroutine currentHighlightCoroutine;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _stepNameText;
    [SerializeField] private TextMeshProUGUI _stepDescriptionText;
    [SerializeField] private RectTransform _continueButton;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private RectTransform _highlightParent;
    [SerializeField] private Image _backDarkImage;
    [SerializeField] private Image _progressImage;
    [SerializeField] private Image _progressImageBack;
    [SerializeField] private Image _buttonOutline;

    [Header("Outside References")]
    [SerializeField] private SkillsPanelButton _skillsPanelFirstButton;
    [SerializeField] private SkillsPanelButton _skillsPanelSecondButton;
    [SerializeField] private Animator _animator;



    private void Update()
    {
        if (!isDisplayed) return;

        if (currentProgress > 0)
        {
            currentProgress -= Time.deltaTime;
            _progressImage.fillAmount = 1 - currentProgress / currentTutoData.waitDuration;
            if (currentProgress <= 0) HideTutorial();
        }

        // To maintain the right depth
        for (int i = 0; i < highlightedImages.Length; i++)
        {
            highlightedImages[i].rectTransform.position = new Vector3(highlightedImages[i].rectTransform.position.x, 
                highlightedImages[i].rectTransform.position.y, 0);
        }

        if (!maintainPosition) return;

        for (int i = 0; i < highlightedImages.Length; i++)
        {
            highlightedImages[i].rectTransform.position = new Vector3(originalPositions[i].x, originalPositions[i].y, 0);
        }
    }


    public void LockUIInputs()
    {
        _backDarkImage.raycastTarget = true;
    }

    public void UnlockUIInputs()
    {
        _backDarkImage.raycastTarget = false;
    }


    public void DisplayTutorial(TutoStepData tutoData, Vector3 aimedLocalPos, Vector3 startLocalPos)
    {
        currentTutoData = tutoData;

        _progressImageBack.gameObject.SetActive(false);
        _progressImage.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(false);

        isDisplayed = true;

        if (tutoData.endCondition == TutoEndCondition.ClickContinue)
        {
            _continueButton.gameObject.SetActive(true);
        }
        else if(tutoData.endCondition == TutoEndCondition.WaitDuration)
        {
            _progressImage.gameObject.SetActive(true);
            _progressImageBack.gameObject.SetActive(true);

            currentProgress = tutoData.waitDuration;
        }

        _stepNameText.text = tutoData.stepName;

        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(tutoData.stepDescription, _stepDescriptionText));

        _rectTr.localPosition = startLocalPos;
        startPosSave = startLocalPos;

        _rectTr.DOLocalMove(aimedLocalPos, 0.35f).SetEase(Ease.OutBack);

        _backDarkImage.raycastTarget = true;

        StartHighlight();
    }

    public void HideTutorial()
    {
        if (!isDisplayed) return;
        isDisplayed = false;

        switch (currentTutoData.lockInputType)
        {
            case LockInputType.LockHeroInputs:
                HeroesManager.Instance.RestartControl();
                break;

            case LockInputType.LockAllInputs:
                InputManager.DisableInputs = false;
                break;

            case LockInputType.LockAllExceptEquipment:
                InputManager.DisableInputs = false;
                InputManager.ExceptEquipment = false;
                break;

            case LockInputType.LockAllExceptSkillTrees:
                InputManager.DisableInputs = false;
                InputManager.ExceptSkillTree = false;
                break;

            case LockInputType.LockAllExceptSkills:
                InputManager.DisableInputs = false;
                InputManager.ExceptSkills = false;
                break;
        }

        //_rectTr.DOScale(Vector3.one * 0f, 0.25f).SetEase(Ease.OutBack);
        _rectTr.DOLocalMove(startPosSave, 0.3f).SetEase(Ease.InBack);

        _backDarkImage.DOFade(0f, 0.2f).SetEase(Ease.OutBack);
        _backDarkImage.raycastTarget = false;

        //_animator.SetBool("IsDisplayed", false);

        OnHide?.Invoke();

        StopHighlight();
    }


    #region Highlights

    private void StartHighlight()
    {
        if (currentTutoData.highlightType == TutoHighlightType.None && currentTutoData.lockInputType != LockInputType.LockAllInputs)
        {
            _backDarkImage.DOFade(0.4f, 0.2f).SetEase(Ease.OutBack);

            _backDarkImage.raycastTarget = false;

            return;
        }
        _backDarkImage.DOFade(0.7f, 0.2f).SetEase(Ease.OutBack);

        switch (currentTutoData.highlightType)
        {
            case TutoHighlightType.HighlightActionPoints:
                maintainPosition = true;
                highlightedImages = UIManager.Instance.PlayerActionsMenu.ActionPointImages;
                for(int i = 0; i < highlightedImages.Length; i++)
                {
                    highlightedImages[i].enabled = true;
                }

                currentHighlightCoroutine = StartCoroutine(HighlightActionPointsCoroutine());
                break;

            case TutoHighlightType.HighlightSkillPoints:
                maintainPosition = true;
                highlightedImages = UIManager.Instance.PlayerActionsMenu.SkillPointImages;

                for (int i = 0; i < highlightedImages.Length; i++)
                {
                    highlightedImages[i].enabled = true;
                }

                currentHighlightCoroutine = StartCoroutine(HighlightSkillPointsCoroutine());
                break;
                
            case TutoHighlightType.HighlightMoveButton:
                //maintainPosition = true;
                highlightedImages = new Image[] { UIManager.Instance.PlayerActionsMenu.ButtonImages[0] };
                TutoManager.Instance.OnFirstStepValidated += HighlightMoveTile;
                break;

            case TutoHighlightType.HighlightSkillButton:
                maintainPosition = true;
                highlightedImages = new Image[] { UIManager.Instance.PlayerActionsMenu.ButtonImages[1] };
                TutoManager.Instance.OnFirstStepValidated += HighlightFirstSkillSkill;
                break;

            case TutoHighlightType.HighlightEnemy:
                BattleManager.Instance.CurrentEnemies[0].UI.Canvas.sortingOrder = 331;
                BattleManager.Instance.CurrentEnemies[0].SpriteRenderer.GetComponent<SpriteLayerer>().enabled = false;
                BattleManager.Instance.CurrentEnemies[0].SpriteRenderer.sortingOrder = 331;
                highlightedImages = new Image[] { };

                _backDarkImage.raycastTarget = true;
                break;

            case TutoHighlightType.HighlightReturnToCamp:
                highlightedImages = new Image[] { UIManager.Instance.Transition.StopButton };
                break;

            case TutoHighlightType.HighlightEquipmentMenu:
                UIManager.Instance.HUDExploration.ActualiseDisplayedButtons();
                UIManager.Instance.HUDExploration.Animator.enabled = false;
                highlightedImages = new Image[] { UIManager.Instance.HUDExploration.EquipmentMenuImage };
                break;

            case TutoHighlightType.HighlightSkillTreeMenu:
                UIManager.Instance.HUDExploration.ActualiseDisplayedButtons();
                UIManager.Instance.HUDExploration.Animator.enabled = false;
                highlightedImages = new Image[] { UIManager.Instance.HUDExploration.SkillTreeMenu };
                break;

            case TutoHighlightType.HighlightSkillsMenu:
                UIManager.Instance.HUDExploration.ActualiseDisplayedButtons();
                UIManager.Instance.HUDExploration.Animator.enabled = false;
                highlightedImages = new Image[] { UIManager.Instance.HUDExploration.SkillsMenu };
                break;
        }

        if (highlightedImages.Length == 0)
            return;

        originalPositions = new Vector3[highlightedImages.Length];
        originalLocalPositions = new Vector3[highlightedImages.Length];
        originalParent = highlightedImages[0].rectTransform.parent;

        for(int i = 0; i < highlightedImages.Length; i++)
        {
            originalPositions[i] = highlightedImages[i].rectTransform.position;
            originalLocalPositions[i] = highlightedImages[i].rectTransform.localPosition;
            //Vector3 saveScale = highlightedImages[i].rectTransform.localScale;

            highlightedImages[i].rectTransform.SetParent(_highlightParent, true);
            highlightedImages[i].rectTransform.position = new Vector3(originalPositions[i].x, originalPositions[i].y, 0);

            //_highlightParent.localScale = highlightedImages[i].rectTransform.localScale;
            //highlightedImages[i].rectTransform.localScale = saveScale;
        }
    }

    private void HighlightMoveTile()
    {
        StopHighlight();

        isDisplayingTileHighlight = true;

        Vector2Int enemyCoordinates = BattleManager.Instance.CurrentEnemies[0].CurrentTile.TileCoordinates;
        BattleManager.Instance.TilesManager.battleRoom.
            PlacedBattleTiles[enemyCoordinates.x - 1, enemyCoordinates.y].SetToFirstLayer(330);

        TutoManager.Instance.OnFirstStepValidated -= HighlightMoveTile;
    }

    private void HighlightFirstSkillSkill()
    {
        StopHighlight();

        _backDarkImage.DOFade(0f, 0.2f).SetEase(Ease.OutBack);
        _backDarkImage.raycastTarget = false;

        TutoManager.Instance.OnFirstStepValidated -= HighlightFirstSkillSkill;

        //isDisplayingSkillHighlight = true;

        //StartCoroutine(HighlightSkillButtonDelayCoroutine(0.8f, 0));
    }

    private IEnumerator HighlightSkillButtonDelayCoroutine(float delay, int buttonIndex)
    {
        yield return new WaitForSeconds(delay);

        originalParent = _skillsPanelFirstButton.GetComponent<RectTransform>().parent;
        for (int i = 0; i < highlightedImages.Length; i++)
        {
            _skillsPanelFirstButton.GetComponent<RectTransform>().parent = _highlightParent;
        }
    }


    private void StopHighlight()
    {
        if (currentTutoData.highlightType == TutoHighlightType.None) return;

        switch(currentTutoData.highlightType)
        {
            case TutoHighlightType.HighlightActionPoints:
                StopCoroutine(currentHighlightCoroutine);
                for(int i = 0; i < highlightedImages.Length; i++)
                {
                    highlightedImages[i].enabled = false;
                }
                break;

            case TutoHighlightType.HighlightSkillPoints:
                StopCoroutine(currentHighlightCoroutine);
                for (int i = 0; i < highlightedImages.Length; i++)
                {
                    highlightedImages[i].enabled = false;
                }
                break;

            case TutoHighlightType.HighlightEnemy:
                BattleManager.Instance.CurrentEnemies[0].SpriteRenderer.GetComponent<SpriteLayerer>().enabled = true;
                break;
        }

        maintainPosition = false;
        UIManager.Instance.HUDExploration.Animator.enabled = true;

        for (int i = 0; i < highlightedImages.Length; i++)
        {
            highlightedImages[i].rectTransform.SetParent(originalParent);
            highlightedImages[i].rectTransform.localPosition = originalLocalPositions[i];
        }
        highlightedImages = new Image[0];

        if (isDisplayingTileHighlight)
        {
            isDisplayingTileHighlight = false;

            Vector2Int enemyCoordinates = BattleManager.Instance.CurrentEnemies[0].CurrentTile.TileCoordinates;
            BattleManager.Instance.TilesManager.battleRoom.
                PlacedBattleTiles[enemyCoordinates.x - 1, enemyCoordinates.y].SetToNormalLayer();
        }
        else if (isDisplayingSkillHighlight)
        {
            isDisplayingSkillHighlight = false;

            for (int i = 0; i < highlightedImages.Length; i++)
            {
                _skillsPanelFirstButton.GetComponent<RectTransform>().parent = originalParent;
            }
        }
    }

    public IEnumerator HighlightActionPointsCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < highlightedImages.Length; i++)
            {
                highlightedImages[i].DOColor(highlightActionPointColor, 0.75f).SetEase(Ease.InOutSine);
            }

            yield return new WaitForSeconds(0.75f);

            for (int i = 0; i < highlightedImages.Length; i++)
            {
                highlightedImages[i].DOColor(noHighlightActionPointColor, 0.75f).SetEase(Ease.InOutSine);
            }

            yield return new WaitForSeconds(0.75f);
        }
    }
    public IEnumerator HighlightSkillPointsCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < highlightedImages.Length; i++)
            {
                highlightedImages[i].DOColor(highlightSkillPointColor, 0.75f).SetEase(Ease.InOutSine);
            }

            yield return new WaitForSeconds(0.75f);

            for (int i = 0; i < highlightedImages.Length; i++)
            {
                highlightedImages[i].DOColor(noHighlightSkillPointColor, 0.75f).SetEase(Ease.InOutSine);
            }

            yield return new WaitForSeconds(0.75f);
        }
    }

    #endregion


    #region Mouse Inputs

    public void HoverContinue()
    {
        _continueButton.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);
        _buttonOutline.DOFade(1, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnhoverContinue()
    {
        _continueButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
        _buttonOutline.DOFade(0, 0.2f).SetEase(Ease.OutBack);
    }

    public void ClickContinue()
    {
        HideTutorial();
    }

    #endregion
}
