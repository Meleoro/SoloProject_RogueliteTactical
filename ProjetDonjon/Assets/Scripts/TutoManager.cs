using System;
using System.Collections;
using UnityEngine;
using Utilities;

public class TutoManager : GenericSingletonClass<TutoManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private TutoStepData[] tutoSteps;
    [SerializeField] private bool disableTuto;

    [Header("Actions")]
    public Action OnFirstStepValidated;

    [Header("Public Infos")]
    public bool DidTutorial { get {  return didTutorial || disableTuto; } }
    public bool DidBattleTuto { get {  return didTutorialStep[2]; } }
    public bool IsInTuto { get {  return didTutorialStep[0] && !didTutorialStep[10]; } }

    [Header("Private Infos")]
    private bool[] didTutorialStep;
    private bool didTutorial;
    private TutoStepData currentStep;
    private int currentTutoID;
    private bool[] endConditionsValidated;

    [Header("References")]
    [SerializeField] private RectTransform _leftPosRef;
    [SerializeField] private RectTransform _rightPosRef;
    [SerializeField] private TutorialPanel _tutorialPanel;
    

    public void StartTutorial()
    {
        didTutorial = true;
    }


    public IEnumerator DisplayTutorialWithDelayCoroutine(int tutoID, float delay)
    {
        yield return new WaitForSeconds(delay);

        DisplayTutorial(tutoID);
    }


    public void DisplayTutorial(int tutoID)
    {
        if(didTutorialStep[tutoID]) return;
        didTutorialStep[tutoID] = true;

        currentStep = tutoSteps[tutoID];
        currentTutoID = tutoID;

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
                UIManager.Instance.PlayerActionsMenu.OnSkillAction += ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.AddShield:
                UIManager.Instance.PlayerActionsMenu.OnSkillAction += ValidateEndCondition;
                BattleManager.Instance.CurrentHeroes[0].OnAlterationAdded += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.Interact:
                HeroesManager.Instance.InteractionManager.OnInteraction += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;
        }
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
    }


    private void EndTutorialStep()
    {
        _tutorialPanel.OnHide -= EndTutorialStep;

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

            case TutoEndCondition.AddShield:
                BattleManager.Instance.CurrentHeroes[0].OnAlterationAdded -= ValidateEndCondition;
                break;

            case TutoEndCondition.Interact:
                HeroesManager.Instance.InteractionManager.OnInteraction -= ValidateEndCondition;
                break;
        }

        if (!currentStep.playNextStepOnFinished) 
        {
            return;
        }

        StartCoroutine(DisplayTutorialWithDelayCoroutine(++currentTutoID, currentStep.playNextStepDelay));
    }


    private void DoSecondBattleTutorial(int index)
    {
        StartCoroutine(DisplayTutorialWithDelayCoroutine(index, 0.5f));
    }


    #region Save Interface

    public void LoadGame(GameData data)
    {
        didTutorialStep = data.finishedTutorialSteps;
        didTutorial = data.launchedTutorial;
    }

    public void SaveGame(ref GameData data)
    {
        data.finishedTutorialSteps = didTutorialStep;
        data.launchedTutorial = didTutorial;
    }

    #endregion
}
