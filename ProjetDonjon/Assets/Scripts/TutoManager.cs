using System;
using System.Collections;
using UnityEngine;
using Utilities;

public class TutoManager : GenericSingletonClass<TutoManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private TutoStepData[] mainTutoSteps;
    [SerializeField] private bool disableTuto;

    [Header("Actions")]
    public Action OnFirstStepValidated;

    [Header("Public Infos")]
    public bool DidTutorial { get {  return (didTutorialStep[0] && didTutorialStep[12]) || disableTuto; } }
    public bool[] DidTutorialStep { get {  return didTutorialStep; } }
    public bool DidBattleTuto { get {  return didTutorialStep[5]; } }
    public bool IsDisplayingTuto { get {  return isDisplayingTuto; } }
    public bool IsInTuto { get {  return didTutorialStep[0] && !didTutorialStep[12]; } }

    [Header("Private Infos")]
    private bool[] didTutorialStep;
    private bool didTutorial;
    private bool isDisplayingTuto;
    private TutoStepData currentStep;
    private int currentTutoID;
    private bool[] endConditionsValidated;

    [Header("References")]
    [SerializeField] private RectTransform _leftPosRef;
    [SerializeField] private RectTransform _rightPosRef;
    [SerializeField] private TutorialPanel _tutorialPanel;


    #region Tutorial Begin

    public void StartTutorial()
    {
        didTutorial = true;

        for(int i = 0; i <= 12; i++)
        {
            didTutorialStep[i] = false;
        }
    }


    public IEnumerator DisplayTutorialWithDelayCoroutine(int tutoID, float delay)
    {
        yield return new WaitForSeconds(delay);

        DisplayTutorial(tutoID);
    }


    public void DisplayTutorial(int tutoID)
    {
        if (didTutorialStep[tutoID]) return;
        if (disableTuto) return;

        isDisplayingTuto = true;

        // To avoid having the camera moving while focusing on one element
        if (BattleManager.Instance.IsInBattle)
            CameraManager.Instance.LockCameraInputs();

        didTutorialStep[tutoID] = true;

        currentStep = mainTutoSteps[tutoID];
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
                UIManager.Instance.PlayerActionsMenu.SkillsPanel.LockOption(1);
                UIManager.Instance.PlayerActionsMenu.OnSkillAction += ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken += ValidateEndCondition;
                endConditionsValidated = new bool[2];
                break;

            case TutoEndCondition.AddShield:
                UIManager.Instance.PlayerActionsMenu.SkillsPanel.LockOption(0);
                UIManager.Instance.PlayerActionsMenu.OnSkillAction += ValidateEndCondition;
                BattleManager.Instance.CurrentHeroes[0].OnAlterationAdded += ValidateEndCondition;
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
                InventoriesManager.Instance.OnInventoryClose -= DoEquipmentTutorial;
                endConditionsValidated = new bool[1];
                break;
        }
    }

    private void DoEquipmentTutorial()
    {
        StartCoroutine(DisplayTutorialWithDelayCoroutine(3, 0.6f));
    }
    private void DoSecondBattleTutorial()
    {
        StartCoroutine(DisplayTutorialWithDelayCoroutine(7, 0.6f));
    }
    private void DoThirdBattleTutorial()
    {
        StartCoroutine(DisplayTutorialWithDelayCoroutine(9, 0.5f));
    }
    private void DoRelicTutorial()
    {
        BattleManager.Instance.OnBattleEnd -= DoRelicTutorial;
        StartCoroutine(DisplayTutorialWithDelayCoroutine(11, 1f));
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
        isDisplayingTuto = false;
        _tutorialPanel.OnHide -= EndTutorialStep;

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

            case TutoEndCondition.AddShield:
                UIManager.Instance.PlayerActionsMenu.OnSkillAction -= ValidateEndCondition;
                BattleManager.Instance.CurrentHeroes[0].OnAlterationAdded -= ValidateEndCondition;
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
                InventoriesManager.Instance.OnInventoryClose += DoEquipmentTutorial;

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
        didTutorial = data.launchedTutorial;
    }

    public void SaveGame(ref GameData data)
    {
        data.finishedTutorialSteps = didTutorialStep;
        data.launchedTutorial = didTutorial;
    }

    #endregion
}
