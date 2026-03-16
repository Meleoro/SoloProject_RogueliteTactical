using System;
using System.Collections;
using UnityEngine;
using Utilities;

public class GameManager : GenericSingletonClass<GameManager>
{
    [Header("Parameters")]
    [SerializeField] private bool enableDebugCommands;
    [SerializeField] private bool startGameInExplo;
    [SerializeField] private EnviroData startEnviroData;

    [Header("Actions")]
    public Action<int> OnReturnToCamp;  // For tutorial

    [Header("Public Infos")]
    public bool IsInExplo { get; private set; }
    public bool EnableDebugCommands { get { return enableDebugCommands; } }
    public int ExpeditionCoinsCount { get; private set; }
    public int ExpeditionKillCount { get; private set; }
    public int ExpeditionRelicCount { get; private set; }
    public int ExpeditionChestCount { get; private set; }


    private void Start()
    {
        BattleManager.Instance.OnEnemyKilled += KillEnemy;
        InventoriesManager.Instance.OnCoinsObtained += ObtainGold;
        RelicsManager.Instance.OnRelicObtained += ObtainRelic;

        if (startGameInExplo || !TutoManager.Instance.DidTutorial)
        {
            StartExploration(startEnviroData, true);
        }
        else
        {
            UIMetaManager.Instance.EnterMetaMenu();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && EnableDebugCommands)
        {
            StartCoroutine(EndExplorationCoroutine());
        }
    }


    #region Start / End Expedition

    public void StartExploration(EnviroData enviroData, bool isStart)
    {
        UIMetaManager.Instance.QuitMetaMenu();
        IsInExplo = true;

        UIManager.Instance.StartExploration(enviroData, isStart);
        RelicsManager.Instance.StartExploration(enviroData.enviroIndex);

        ProceduralGenerationManager.Instance.StartExploration(enviroData, !TutoManager.Instance.DidTutorial);

        ExpeditionCoinsCount = 0;
        ExpeditionKillCount = 0;
        ExpeditionRelicCount = 0;
        ExpeditionChestCount = 0;
    }

    public IEnumerator EndExplorationCoroutine()
    {
        OnReturnToCamp?.Invoke(0);

        UIManager.Instance.Transition.FadeScreen(0.8f, 1);

        yield return new WaitForSeconds(1);

        UIManager.Instance.Transition.FadeScreen(0.8f, 0);

        ProceduralGenerationManager.Instance.EndExploration();
        HeroesManager.Instance.EndExploration();
        UIManager.Instance.EndExploration();

        IsInExplo = false;

        UIMetaManager.Instance.EnterMetaMenu();
    }


    #endregion


    #region Expedition Stats

    public void ObtainGold(int quantity)
    {
        ExpeditionCoinsCount += quantity;
    }

    public void KillEnemy()
    {
        ExpeditionKillCount += 1;
    }

    public void ObtainRelic(RelicData data, int index)
    {
        ExpeditionRelicCount += 1;
    }

    public void OpenChest()
    {
        ExpeditionChestCount += 1;
    }

    #endregion
}
