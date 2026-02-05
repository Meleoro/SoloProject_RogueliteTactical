using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
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
    private bool isDisplayingTileHighlight;
    private bool isDisplayingSkillHighlight;
    private bool maintainPosition;
    private bool isDisplayed;
    private float currentProgress;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _stepNameText;
    [SerializeField] private TextMeshProUGUI _stepDescriptionText;
    [SerializeField] private RectTransform _continueButton;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private RectTransform _highlightParent;
    [SerializeField] private Image _backDarkImage;
    [SerializeField] private Image _progressImage;
    [SerializeField] private Image _progressImageBack;

    [Header("Outside References")]
    [SerializeField] private SkillsPanelButton _skillsPanelFirstButton;
    [SerializeField] private SkillsPanelButton _skillsPanelSecondButton;



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


    public void DisplayTutorial(TutoStepData tutoData)
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
        _stepDescriptionText.text = tutoData.stepDescription;

        _rectTr.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.OutBack);
        _backDarkImage.raycastTarget = true;

        StartHighlight();
    }

    public void HideTutorial()
    {
        if (!isDisplayed) return;
        isDisplayed = false;

        _rectTr.DOScale(Vector3.one * 0, 0.2f).SetEase(Ease.InBack);
        _backDarkImage.DOFade(0f, 0.2f).SetEase(Ease.OutBack);
        _backDarkImage.raycastTarget = false;

        OnHide?.Invoke();

        StopHighlight();
    }


    #region Highlights

    private void StartHighlight()
    {
        if (currentTutoData.highlightType == TutoHighlightType.None)
        {
            _backDarkImage.DOFade(0.3f, 0.2f).SetEase(Ease.OutBack);

            _backDarkImage.raycastTarget = false;

            return;
        }
        _backDarkImage.DOFade(0.7f, 0.2f).SetEase(Ease.OutBack);

        switch (currentTutoData.highlightType)
        {
            case TutoHighlightType.HighlightActionPoints:
                maintainPosition = true;
                highlightedImages = UIManager.Instance.PlayerActionsMenu.ActionPointImages;
                break;

            case TutoHighlightType.HighlightSkillPoints:
                maintainPosition = true;
                highlightedImages = UIManager.Instance.PlayerActionsMenu.SkillPointImages;
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
                BattleManager.Instance.CurrentEnemies[0].UI.Canvas.sortingOrder = 330;
                highlightedImages = new Image[] { };

                _backDarkImage.DOComplete();
                _backDarkImage.DOFade(0.3f, 0.2f).SetEase(Ease.OutBack);
                break;

            case TutoHighlightType.HighlightReturnToCamp:
                highlightedImages = new Image[] { UIManager.Instance.Transition.StopButton.image };
                break;

            case TutoHighlightType.HighlightEquipmentMenu:
                UIManager.Instance.HUDExploration.Animator.enabled = false;
                highlightedImages = new Image[] { UIManager.Instance.HUDExploration.EquipmentMenuImage };
                break;

            case TutoHighlightType.HighlightSkillTreeMenu:
                UIManager.Instance.HUDExploration.Animator.enabled = false;
                highlightedImages = new Image[] { UIManager.Instance.HUDExploration.SkillTreeMenu };
                break;

            case TutoHighlightType.HighlightSkillsMenu:
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

            highlightedImages[i].rectTransform.SetParent(_highlightParent, true);

            highlightedImages[i].rectTransform.position = new Vector3(originalPositions[i].x, originalPositions[i].y, 0);
        }
    }

    private void HighlightMoveTile()
    {
        StopHighlight();

        isDisplayingTileHighlight = true;

        Vector2Int enemyCoordinates = BattleManager.Instance.CurrentEnemies[0].CurrentTile.TileCoordinates;
        BattleManager.Instance.TilesManager.battleRoom.
            PlacedBattleTiles[enemyCoordinates.x, enemyCoordinates.y - 1].SetToFirstLayer(330);

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
                PlacedBattleTiles[enemyCoordinates.x, enemyCoordinates.y - 1].SetToNormalLayer();
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

    #endregion


    #region Mouse Inputs

    public void HoverContinue()
    {
        _continueButton.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);
    }

    public void UnhoverContinue()
    {
        _continueButton.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
    }

    public void ClickContinue()
    {
        HideTutorial();
    }

    #endregion
}
