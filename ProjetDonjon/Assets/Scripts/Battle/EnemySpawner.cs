using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    struct SpawnerEnemyStruct
    {
        public AIUnit enemy;
        [Range(0, 100)] public int probability;
    }

    [Header("Debug Parameters")]
    [SerializeField] private bool isDebugSpawer;
    [SerializeField] private Unit spawnedUnit;

    [Header("Public Infos")]
    [HideInInspector] public BattleTile AssociatedTile;

    [Header("Private Infos")]
    private EnemySpawn[] possibleSpawns;


    private void Start()
    {
        possibleSpawns = ProceduralGenerationManager.Instance?.EnviroData.
            enemySpawnsPerFloor[ProceduralGenerationManager.Instance.CurrentFloor].possibleEnemies;
    }


    public (EnemySpawn, bool) GetSpawnedEnemy(int dangerAmountToFill)
    {
        if (isDebugSpawer)
        {
            EnemySpawn debugSpawn = new EnemySpawn();
            debugSpawn.enemyPrefab = spawnedUnit;
            debugSpawn.minEnemyCountBeforeSpawn = 0;
            debugSpawn.maxCountPerBattle = 100;

            return (debugSpawn, false);
        }

        int pickedProba = Random.Range(0, 100);
        int cumulatedProba = 0;

        for(int i = 0; i < possibleSpawns.Length; i++)
        {
            cumulatedProba += possibleSpawns[i].proba;
            if(cumulatedProba >= pickedProba)
            {
                return (possibleSpawns[i], Random.Range(0, 100) < possibleSpawns[i].eliteProba);
            }
        }

        return (null, false);
    }

    private void OnDrawGizmos()
    {
        if(isDebugSpawer) Gizmos.color = Color.blue;
        else Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
