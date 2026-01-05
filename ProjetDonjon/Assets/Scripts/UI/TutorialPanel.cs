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
    private Image[] highlightedImages;
    private Transform originalParent;
    private bool isDisplayingTileHighlight;
    private bool isDisplayingSkillHighlight;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _stepNameText;
    [SerializeField] private TextMeshProUGUI _stepDescriptionText;
    [SerializeField] private RectTransform _continueButton;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private RectTransform _highlightParent;
    [SerializeField] private Image _backDarkImage;

    [Header("Outside References")]
    [SerializeField] private SkillsPanelButton _skillsPanelFirstButton;
    [SerializeField] private SkillsPanelButton _skillsPanelSecondButton;



    public void DisplayTutorial(TutoStepData tutoData)
    {
        currentTutoData = tutoData;

        if(tutoData.endCondition == TutoEndCondition.ClickContinue)
        {
            _continueButton.gameObject.SetActive(true);
        }
        else
        {
            _continueButton.gameObject.SetActive(false);
        }

        _stepNameText.text = tutoData.stepName;
        _stepDescriptionText.text = tutoData.stepDescription;

        _rectTr.DOScale(Vector3.one * 1, 0.2f).SetEase(Ease.OutBack);
        _backDarkImage.raycastTarget = true;

        StartHighlight();
    }


    private void StartHighlight()
    {
        if (currentTutoData.highlightType == TutoHighlightType.None)
        {
            _backDarkImage.DOFade(0.3f, 0.2f).SetEase(Ease.OutBack);
            return;
        }
        _backDarkImage.DOFade(0.7f, 0.2f).SetEase(Ease.OutBack);

        switch (currentTutoData.highlightType)
        {
            case TutoHighlightType.HighlightActionPoints:
                highlightedImages = UIManager.Instance.PlayerActionsMenu.ActionPointImages;
                break;

            case TutoHighlightType.HighlightSkillPoints:
                highlightedImages = UIManager.Instance.PlayerActionsMenu.SkillPointImages;
                break;

            case TutoHighlightType.HighlightMoveButton:
                highlightedImages = new Image[] { UIManager.Instance.PlayerActionsMenu.ButtonImages[0] };
                TutoManager.Instance.OnFirstStepValidated += HighlightMoveTile;
                break;

            case TutoHighlightType.HighlightSkillButton:
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
                highlightedImages = new Image[] { UIManager.Instance.FloorTransition.StopButton.image };

                _backDarkImage.DOComplete();
                _backDarkImage.DOFade(0.7f, 0.2f).SetEase(Ease.OutBack);
                break;
        }

        if (highlightedImages.Length == 0) return;

        originalParent = highlightedImages[0].rectTransform.parent;
        for(int i = 0; i < highlightedImages.Length; i++)
        {
            highlightedImages[i].rectTransform.parent = _highlightParent;
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

        for (int i = 0; i < highlightedImages.Length; i++)
        {
            highlightedImages[i].rectTransform.parent = originalParent;
        }

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


    public void HideTutorial()
    {
        _rectTr.DOScale(Vector3.one * 0, 0.2f).SetEase(Ease.InBack);
        _backDarkImage.DOFade(0f, 0.2f).SetEase(Ease.OutBack);
        _backDarkImage.raycastTarget = false;

        OnHide?.Invoke();

        StopHighlight();
    }


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
