using System;
using System.Collections;
using UnityEngine;
using Utilities;

public class TutoManager : GenericSingletonClass<TutoManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private TutoStepData[] mainTutoSteps;
    [SerializeField] private TutoStepData[] additionalTutoSteps;
    [SerializeField] private bool disableTuto;

    [Header("Actions")]
    public Action OnFirstStepValidated;

    [Header("Public Infos")]
    public bool DidTutorial { get {  return (didTutorialStep[0] && didTutorialStep[10]) || disableTuto; } }
    public bool[] DidTutorialStep { get {  return didTutorialStep; } }
    public bool[] DidAdditionalTutorialStep { get {  return didAdditionalTutorialSteps; } }
    public bool DidBattleTuto { get {  return didTutorialStep[4]; } }
    public bool IsDisplayingTuto { get {  return isDisplayingTuto; } }
    public bool IsInTuto { get {  return didTutorialStep[0] && !didTutorialStep[10]; } }

    [Header("Private Infos")]
    private bool[] didTutorialStep;
    private bool[] didAdditionalTutorialSteps;
    private bool didTutorial;
    private bool isDisplayingTuto;
    private TutoStepData currentStep;
    private int currentTutoID;
    private bool[] endConditionsValidated;

    [Header("References")]
    [SerializeField] private RectTransform _leftPosRef;
    [SerializeField] private RectTransform _rightPosRef;
    [SerializeField] private RectTransform _upLeftPosRef;
    [SerializeField] private RectTransform _upRightPosRef;
    [SerializeField] private TutorialPanel _tutorialPanel;
    [SerializeField] private TutorialPanel _smallTutorialPanel;


    private void Start()
    {
        StartCoroutine(BindTutorialsDelayCoroutine());
    }

    private IEnumerator BindTutorialsDelayCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        BindTutorials();
    }

    // Called at the start to bind the tutorial steps the player hasn't done
    private void BindTutorials()
    {
        if (!didAdditionalTutorialSteps[3])
        {
            for (int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
            {
                HeroesManager.Instance.Heroes[i].OnLevelUp += BindLevelUpTurorialToBattleEnd;
            }
        }
    }


    #region Tutorial Launch

    public void StartTutorial()
    {
        didTutorial = true;

        for(int i = 0; i < didTutorialStep.Length; i++)
        {
            didTutorialStep[i] = false;
        }
    }


    public IEnumerator DisplayTutorialWithDelayCoroutine(int tutoID, float delay, bool isAdditionalStep = false)
    {
        yield return new WaitForSeconds(delay);

        DisplayTutorial(tutoID, isAdditionalStep);
    }


    public void DisplayTutorial(int tutoID, bool isAdditionalStep = false)
    {
        if (isAdditionalStep ? didAdditionalTutorialSteps[tutoID] : didTutorialStep[tutoID] ) return;
        //if (disableTuto) return;

        isDisplayingTuto = true;

        // To avoid having the camera moving while focusing on one element
        if (BattleManager.Instance.IsInBattle)
            CameraManager.Instance.LockCameraInputs();

        if(isAdditionalStep) didAdditionalTutorialSteps[tutoID] = true;
        else didTutorialStep[tutoID] = true;

        currentStep = isAdditionalStep ? additionalTutoSteps[tutoID] : mainTutoSteps[tutoID];
        currentTutoID = tutoID;

        // Classic Panel
        if(currentStep.tutoType == TutoType.ClassicPanel)
        {
            if (currentStep.leftSidePanel)
            {
                _tutorialPanel.RectTr.localPosition = _leftPosRef.localPosition;
                _tutorialPanel.DisplayTutorial(currentStep);
            }
            else
            {
                _tutorialPanel.RectTr.localPosition = _rightPosRef.localPosition;
                _tutorialPanel.DisplayTutorial(currentStep);
            }

            _tutorialPanel.OnHide += EndTutorialStep;
        }

        // Small Panel
        else
        {
            if (currentStep.leftSidePanel)
            {
                _smallTutorialPanel.RectTr.localPosition = _upLeftPosRef.localPosition;
                _smallTutorialPanel.DisplayTutorial(currentStep);
            }
            else
            {
                _smallTutorialPanel.RectTr.localPosition = _upRightPosRef.localPosition;
                _smallTutorialPanel.DisplayTutorial(currentStep);
            }

            _smallTutorialPanel.OnHide += EndTutorialStep;
        }

            ObserveEndConditions();
    }


    private void ObserveEndConditions()
    {
        switch(currentStep.endCondition)
        {
            case TutoEndCondition.MoveExplo:
                HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.OnMove += ValidateEndCondition;
                HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.OnJump += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.MoveBattle:
                UIManager.Instance.PlayerActionsMenu.OnMoveAction += ValidateEndCondition;
                Vector2Int enemyCoordinates = BattleManager.Instance.CurrentEnemies[0].CurrentTile.TileCoordinates;
                BattleManager.Instance.TilesManager.battleRoom.
                    PlacedBattleTiles[enemyCoordinates.x, enemyCoordinates.y - 1].OnUnitEnterTile += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.Attack:
                UIManager.Instance.PlayerActionsMenu.SkillsPanel.LockOption(1);
                UIManager.Instance.PlayerActionsMenu.OnSkillAction += ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.UseSecondSkill:
                UIManager.Instance.PlayerActionsMenu.SkillsPanel.LockOption(0);
                UIManager.Instance.PlayerActionsMenu.OnSkillAction += ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.Interact:
                HeroesManager.Instance.InteractionManager.OnInteraction += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.ClickEnemy:
                BattleManager.Instance.CurrentEnemies[0].OnClickUnit += ValidateEndCondition;
                BattleManager.Instance.OnBattleEnd += DoRelicTutorial;
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.ReturnToCamp:
                GameManager.Instance.OnReturnToCamp += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.PlaceItemInInventory:
                FindAnyObjectByType<Loot>().OnPlaceInInventory += ValidateEndCondition;
                endConditionsValidated = new bool[1];

                InventoriesManager.Instance.LockInventory();
                break;

            case TutoEndCondition.EquipEquiment:
                UIManager.Instance.HeroInfosScreen.OnShow += ValidateEndCondition;
                //InventoriesManager.Instance.OnInventoryClose -= DoEquipmentTutorial;
                endConditionsValidated = new bool[1];
                break;
        }
    }

    private void DoSecondBattleTutorial()
    {
        StartCoroutine(DisplayTutorialWithDelayCoroutine(6, 0.6f));
    }

    private void DoThirdBattleTutorial()
    {
        StartCoroutine(DisplayTutorialWithDelayCoroutine(8, 0.5f));
    }

    private void DoRelicTutorial()
    {
        BattleManager.Instance.OnBattleEnd -= DoRelicTutorial;
        StartCoroutine(DisplayTutorialWithDelayCoroutine(9, 1f));
    }

    private void BindLevelUpTurorialToBattleEnd()
    {
        for (int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
        {
            HeroesManager.Instance.Heroes[i].OnLevelUp -= DoLevelUpTutorial;
        }

        BattleManager.Instance.OnBattleEnd += DoLevelUpTutorial;
    }

    private void DoLevelUpTutorial()
    {
        BattleManager.Instance.OnBattleEnd -= DoLevelUpTutorial;

        StartCoroutine(DisplayTutorialWithDelayCoroutine(1, 1f, true));
    }

    #endregion


    #region Tutorial End 

    private void ValidateEndCondition()
    {
        endConditionsValidated[0] = true;

        for (int i = 0; i < endConditionsValidated.Length; i++)
        {
            if (!endConditionsValidated[i]) return;
        }

        _tutorialPanel.HideTutorial();
        _smallTutorialPanel.HideTutorial();
    }

    private void ValidateEndCondition(int actionType)
    {
        endConditionsValidated[actionType] = true;

        if (actionType == 0) OnFirstStepValidated?.Invoke();

        for (int i = 0; i < endConditionsValidated.Length; i++)
        {
            if (!endConditionsValidated[i]) return;
        }

        _tutorialPanel.HideTutorial();
        _smallTutorialPanel.HideTutorial();
    }


    private void EndTutorialStep()
    {
        isDisplayingTuto = false;
        _tutorialPanel.OnHide -= EndTutorialStep;
        _smallTutorialPanel.OnHide -= EndTutorialStep;

        UIManager.Instance.PlayerActionsMenu.SkillsPanel.LockOption(-1);

        CameraManager.Instance.UnlockCameraInputs();

        switch (currentStep.endCondition)
        {
            case TutoEndCondition.MoveExplo:
                HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.OnMove -= ValidateEndCondition;
                HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.OnJump -= ValidateEndCondition;
                break;

            case TutoEndCondition.MoveBattle:
                UIManager.Instance.PlayerActionsMenu.OnMoveAction -= ValidateEndCondition;
                Vector2Int enemyCoordinates = BattleManager.Instance.CurrentEnemies[0].CurrentTile.TileCoordinates;
                BattleManager.Instance.TilesManager.battleRoom.
                    PlacedBattleTiles[enemyCoordinates.x, enemyCoordinates.y - 1].OnUnitEnterTile -= ValidateEndCondition;

                BattleManager.Instance.OnHeroTurnStart += DoSecondBattleTutorial;
                break;

            case TutoEndCondition.Attack:
                UIManager.Instance.PlayerActionsMenu.OnSkillAction -= ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken -= ValidateEndCondition;
                break;

            case TutoEndCondition.UseSecondSkill:
                UIManager.Instance.PlayerActionsMenu.OnSkillAction -= ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken -= ValidateEndCondition;
                BattleManager.Instance.OnHeroTurnStart += DoThirdBattleTutorial;
                break;

            case TutoEndCondition.Interact:
                HeroesManager.Instance.InteractionManager.OnInteraction -= ValidateEndCondition;
                break;

            case TutoEndCondition.ClickEnemy:
                BattleManager.Instance.CurrentEnemies[0].OnClickUnit -= ValidateEndCondition;
                break;

            case TutoEndCondition.ReturnToCamp:
                GameManager.Instance.OnReturnToCamp -= ValidateEndCondition;
                break;

            case TutoEndCondition.PlaceItemInInventory:
                FindAnyObjectByType<Loot>().OnPlaceInInventory -= ValidateEndCondition;
                //InventoriesManager.Instance.OnInventoryClose += DoEquipmentTutorial;

                InventoriesManager.Instance.UnlockInventory();
                break;

            case TutoEndCondition.EquipEquiment:
                UIManager.Instance.HeroInfosScreen.OnShow -= ValidateEndCondition;
                break;
        }

        if (!currentStep.playNextStepOnFinished) 
        {
            return;
        }

        StartCoroutine(DisplayTutorialWithDelayCoroutine(++currentTutoID, currentStep.playNextStepDelay));
    }

    #endregion


    #region Save Interface

    public void LoadGame(GameData data)
    {
        didTutorialStep = data.finishedTutorialSteps;
        didAdditionalTutorialSteps = data.finishedAdditionalTutorialSteps;
        didTutorial = data.launchedTutorial;
    }

    public void SaveGame(ref GameData data)
    {
        data.finishedTutorialSteps = didTutorialStep;
        data.finishedAdditionalTutorialSteps = didAdditionalTutorialSteps;
        data.launchedTutorial = didTutorial;
    }

    #endregion
}
