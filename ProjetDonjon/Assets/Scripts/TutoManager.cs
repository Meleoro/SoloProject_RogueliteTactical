using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class TutoManager : GenericSingletonClass<TutoManager>, ISaveable
{
    [Header("Parameters")]
    [SerializeField] private TutoStepData[] mainTutoSteps;
    [SerializeField] private TutoStepData[] additionalTutoSteps;
    [SerializeField] private bool disableTuto;
    [SerializeField] private bool disableAdditionalTuto;

    [Header("Actions")]
    public Action OnFirstStepValidated;

    [Header("Public Infos")]
    public bool DidTutorial { get {  return (didTutorialStep[0] && didTutorialStep[10]) || disableTuto; } }
    public bool[] DidTutorialStep { get {  return didTutorialStep; } }
    public bool[] DidAdditionalTutorialStep { get {  return didAdditionalTutorialSteps; } }
    public bool DidBattleTuto { get {  return didTutorialStep[4]; } }
    public bool IsDisplayingTuto { get {  return isDisplayingTuto; } }
    public bool IsInTuto { get {  return !isInitialised || (didTutorialStep[0] && !didTutorialStep[10]); } }
    public bool TutoDisabled { get {  return disableTuto; } }

    [Header("Private Infos")]
    private bool[] didTutorialStep;
    private bool[] didAdditionalTutorialSteps;
    private bool didTutorial;
    private bool isDisplayingTuto;
    private bool isInitialised;
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
    [SerializeField] private TutorialPanel _screenTutorialPanel;



    private void Start()
    {
        StartCoroutine(WaitForBindCoroutine());
    }

    private IEnumerator WaitForBindCoroutine()
    {
        while(!HeroesManager.Instance.IsInitialised && !isInitialised)
        {
            yield return new WaitForSeconds(0.05f);
        }

        BindTutorials();
    }


    // Called at the start to bind the tutorial steps the player hasn't done
    private void BindTutorials()
    {
        if (!didAdditionalTutorialSteps[2])
        {
            InventoriesManager.Instance.OnEquipmentAdded += BindEquipmentTutorial;
        }

        if (!didAdditionalTutorialSteps[3])
        {
            for (int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
            {
                HeroesManager.Instance.Heroes[i].OnLevelUp += BindLevelUpTurorialToBattleEnd;
            }
        }

        if (!didAdditionalTutorialSteps[5])
        {
            BattleManager.Instance.OnBattleWithManyEnemiesStart += DoAllPatternsTutorial;
        }

        if (!didAdditionalTutorialSteps[7])
        {
            UIMetaManager.Instance.ChestMenu.OnShow += DoChestMenuTutorial;
        }

        if (!didAdditionalTutorialSteps[8])
        {
            for(int i = 0; i < HeroesManager.Instance.AllHeroes.Length; i++)
            {
                HeroesManager.Instance.AllHeroes[i].OnDead += DoHeroDeathTutorial;
            }
        }

        if (!didAdditionalTutorialSteps[9])
        {
            GameManager.Instance.OnExpeditionStart += DoExpeditionTutotial;
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
        if (isAdditionalStep ? didAdditionalTutorialSteps[tutoID] : didTutorialStep[tutoID]) yield break;
        if (disableTuto && !isAdditionalStep) yield break;

        TutoStepData step = isAdditionalStep ? additionalTutoSteps[tutoID] : mainTutoSteps[tutoID];

        switch (step.lockInputType)
        {
            case LockInputType.LockHeroInputs:
                HeroesManager.Instance.StopControl();
                break;

            case LockInputType.LockAllInputs:
                InputManager.DisableInputs = true;
                break;

            case LockInputType.LockAllExceptEquipment:
                InputManager.DisableInputs = true;
                InputManager.ExceptEquipment = true;
                break;

            case LockInputType.LockAllExceptSkillTrees:
                InputManager.DisableInputs = true;
                InputManager.ExceptSkillTree = true;
                break;

            case LockInputType.LockAllExceptSkills:
                InputManager.DisableInputs = true;
                InputManager.ExceptSkills = true;
                break;
        }
        _tutorialPanel.LockUIInputs();

        yield return new WaitForSeconds(delay);

        //if (step && step.lockInputType != LockInputType.DontLock)
        //{
        //    HeroesManager.Instance.RestartControl();
        //}
        _tutorialPanel.UnlockUIInputs();
        DisplayTutorial(tutoID, isAdditionalStep);
    }

    public void DisplayTutorial(int tutoID, bool isAdditionalStep = false)
    {
        if (isAdditionalStep ? didAdditionalTutorialSteps[tutoID] : didTutorialStep[tutoID] ) return;
        if (disableTuto && !isAdditionalStep) return;

        isDisplayingTuto = true;

        // To avoid having the camera moving while focusing on one element
        if (BattleManager.Instance.IsInBattle && !isAdditionalStep)
            CameraManager.Instance.LockCameraInputs();

        if (isAdditionalStep) didAdditionalTutorialSteps[tutoID] = true;
        else didTutorialStep[tutoID] = true;

        SaveManager.Instance.SaveGame(SaveManager.Instance.CurrentFileIndex);

        currentStep = isAdditionalStep ? additionalTutoSteps[tutoID] : mainTutoSteps[tutoID];
        currentTutoID = tutoID;

        switch(currentStep.lockInputType)
        {
            case LockInputType.LockHeroInputs:
                HeroesManager.Instance.StopControl();
                break;

            case LockInputType.LockAllInputs:
                InputManager.DisableInputs = true;
                break;

            case LockInputType.LockAllExceptEquipment:
                InputManager.DisableInputs = true;
                InputManager.ExceptEquipment = true;
                break;

            case LockInputType.LockAllExceptSkillTrees:
                InputManager.DisableInputs = true;
                InputManager.ExceptSkillTree = true;
                break;

            case LockInputType.LockAllExceptSkills:
                InputManager.DisableInputs = true;
                InputManager.ExceptSkills = true;
                break;
        }

        // Classic Panel
        if (currentStep.tutoType == TutoType.ClassicPanel)
        {
            if (currentStep.leftSidePanel)
            {
                _tutorialPanel.DisplayTutorial(currentStep, _leftPosRef.localPosition, _leftPosRef.localPosition + new Vector3(-300, 0, 0));
            }
            else
            {
                _tutorialPanel.DisplayTutorial(currentStep, _rightPosRef.localPosition, _rightPosRef.localPosition + new Vector3(300, 0, 0));
            }

            _tutorialPanel.OnHide += EndTutorialStep;
        }

        // Small Panel
        else if (currentStep.tutoType == TutoType.SmallPanel)
        {
            if (currentStep.leftSidePanel)
            {
                _smallTutorialPanel.DisplayTutorial(currentStep, _upLeftPosRef.localPosition, _upLeftPosRef.localPosition + new Vector3(0, 250, 0));
            }
            else
            {
                _smallTutorialPanel.DisplayTutorial(currentStep, _upRightPosRef.localPosition, _upRightPosRef.localPosition + new Vector3(0, 250, 0));
            }

            _smallTutorialPanel.OnHide += EndTutorialStep;
        }

        // Screen Panel
        else
        {
            _screenTutorialPanel.DisplayTutorial(currentStep, Vector2.zero, new Vector3(0, -500, 0));

            _screenTutorialPanel.OnHide += EndTutorialStep;
        }

        ObserveEndConditions();
    }

    private void ObserveEndConditions()
    {
        switch(currentStep.endCondition)
        {
            case TutoEndCondition.MoveExplo:
                HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.OnMove += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.Jump:
                HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.OnJump += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.MoveBattle:
                UIManager.Instance.PlayerActionsMenu.OnMoveAction += ValidateEndCondition;
                Vector2Int enemyCoordinates = BattleManager.Instance.CurrentEnemies[0].CurrentTile.TileCoordinates;
                BattleManager.Instance.TilesManager.battleRoom.
                    PlacedBattleTiles[enemyCoordinates.x - 1, enemyCoordinates.y].OnUnitEnterTile += ValidateEndCondition;
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
                BattleManager.Instance.CurrentHeroes[0].OnClickUnit += ValidateEndCondition;

                Vector2Int heroCoordinates = BattleManager.Instance.CurrentHeroes[0].CurrentTile.TileCoordinates;
                BattleManager.Instance.TilesManager.battleRoom.
                    PlacedBattleTiles[heroCoordinates.x, heroCoordinates.y].OnClickTile += ValidateEndCondition;
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
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.OpenSkillTree:
                UIManager.Instance.SkillTreeManager.OnShow += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;

            case TutoEndCondition.OpenSkills:
                UIManager.Instance.SkillsMenu.OnShow += ValidateEndCondition;
                endConditionsValidated = new bool[1];
                break;
        }
    }

    #endregion


    #region Specific Starts

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
        StartCoroutine(DisplayTutorialWithDelayCoroutine(10, 1f));
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

        StartCoroutine(DisplayTutorialWithDelayCoroutine(3, 1f, true));
    }

    private void DoSkillsMenuTutorial()
    {
        UIManager.Instance.SkillTreeManager.OnHide -= DoSkillsMenuTutorial;

        StartCoroutine(DisplayTutorialWithDelayCoroutine(4, 0.7f, true));
    }

    private void DoAllPatternsTutorial()
    {
        BattleManager.Instance.OnBattleStart -= DoAllPatternsTutorial;

        StartCoroutine(DisplayTutorialWithDelayCoroutine(5, 2f, true));
    }

    private void DoChestMenuTutorial()
    {
        UIMetaManager.Instance.ChestMenu.OnShow -= DoChestMenuTutorial;

        StartCoroutine(DisplayTutorialWithDelayCoroutine(7, 0.2f, true));
    }

    private void DoHeroDeathTutorial()
    {
        for (int i = 0; i < HeroesManager.Instance.AllHeroes.Length; i++)
        {
            HeroesManager.Instance.AllHeroes[i].OnDead -= DoHeroDeathTutorial;
        }

        StartCoroutine(DisplayTutorialWithDelayCoroutine(8, 0f, true));
    }

    private void BindEquipmentTutorial()
    {
        InventoriesManager.Instance.OnEquipmentAdded -= BindEquipmentTutorial;
        InventoriesManager.Instance.OnInventoryClose += DoEquipmentTutorial;
    }

    private void DoEquipmentTutorial()
    {
        InventoriesManager.Instance.OnInventoryClose -= DoEquipmentTutorial;

        StartCoroutine(DisplayTutorialWithDelayCoroutine(2, 0.5f, true));
    }

    private void DoExpeditionTutotial()
    {
        GameManager.Instance.OnExpeditionStart -= DoExpeditionTutotial;

        StartCoroutine(DisplayTutorialWithDelayCoroutine(9, 1.5f, true));
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

        switch (currentStep.lockInputType)
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

        if (currentStep.lockInputType == LockInputType.LockHeroInputs)
        {
            HeroesManager.Instance.RestartControl();
        }
        else if (currentStep.lockInputType == LockInputType.LockAllInputs)
        {
            InputManager.DisableInputs = false;
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
                    PlacedBattleTiles[enemyCoordinates.x - 1, enemyCoordinates.y].OnUnitEnterTile -= ValidateEndCondition;

                BattleManager.Instance.OnHeroTurnStart += DoSecondBattleTutorial;
                break;

            case TutoEndCondition.Attack:
                UIManager.Instance.PlayerActionsMenu.OnSkillAction -= ValidateEndCondition;
                BattleManager.Instance.CurrentEnemies[0].OnDamageTaken -= ValidateEndCondition;
                break;

            case TutoEndCondition.UseSecondSkill:
                UIManager.Instance.PlayerActionsMenu.OnSkillAction -= ValidateEndCondition;
                BattleManager.Instance.CurrentHeroes[0].OnClickUnit -= ValidateEndCondition;

                Vector2Int heroCoordinates = BattleManager.Instance.CurrentHeroes[0].CurrentTile.TileCoordinates;
                BattleManager.Instance.TilesManager.battleRoom.
                    PlacedBattleTiles[heroCoordinates.x, heroCoordinates.y].OnClickTile -= ValidateEndCondition;

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
                InventoriesManager.Instance.UnlockInventory();
                break;

            case TutoEndCondition.EquipEquiment:
                UIManager.Instance.HeroInfosScreen.OnShow -= ValidateEndCondition;
                break;

            case TutoEndCondition.OpenSkillTree:
                UIManager.Instance.SkillTreeManager.OnShow -= ValidateEndCondition;
                UIManager.Instance.SkillTreeManager.OnHide += DoSkillsMenuTutorial;
                break;

            case TutoEndCondition.OpenSkills:
                UIManager.Instance.SkillsMenu.OnShow -= ValidateEndCondition;
                break;
        }

        if(currentStep.stepName == "CAMP")
        {
            UIMetaManager.Instance.MainMetaMenu.ActualiseCampProgress();
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

        isInitialised = true;
    }

    public void SaveGame(ref GameData data)
    {
        data.finishedTutorialSteps = didTutorialStep;
        data.finishedAdditionalTutorialSteps = didAdditionalTutorialSteps;
        data.launchedTutorial = didTutorial;
    }

    #endregion
}
