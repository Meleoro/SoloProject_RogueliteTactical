using System.Collections;
using UnityEngine;
using Utilities;

public class GameManager : GenericSingletonClass<GameManager>
{
    [Header("Parameters")]
    [SerializeField] private bool startGameInExplo;
    [SerializeField] private EnviroData startEnviroData;


    [Header("Public Infos")]
    public bool IsInExplo { get; private set; }


    private void Start()
    {
        if (startGameInExplo)
        {
            StartExploration(startEnviroData);
        }
        else
        {
            UIMetaManager.Instance.EnterMetaMenu();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(EndExplorationCoroutine());
        }
    }


    #region Start / End Exploration

    public void StartExploration(EnviroData enviroData)
    {
        UIMetaManager.Instance.QuitMetaMenu();
        IsInExplo = true;

        UIManager.Instance.StartExploration(enviroData);
        RelicsManager.Instance.StartExploration(enviroData.enviroIndex);

        ProceduralGenerationManager.Instance.StartExploration(enviroData, !TutoManager.Instance.DidTutorial);
    }


    public IEnumerator EndExplorationCoroutine()
    {
        UIManager.Instance.FloorTransition.FadeScreen(0.8f, 1);

        yield return new WaitForSeconds(1);

        UIManager.Instance.FloorTransition.FadeScreen(0.8f, 0);

        ProceduralGenerationManager.Instance.EndExploration();
        HeroesManager.Instance.EndExploration();
        UIManager.Instance.EndExploration();

        IsInExplo = false;

        UIMetaManager.Instance.EnterMetaMenu();
    }


    #endregion
}
