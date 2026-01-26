using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;



[Serializable]
public struct RoomEntrance
{
    public int entranceWidth;
    public Vector2Int entranceDirection;
}

[Serializable]
public struct RoomBlockableEntranceStruct
{
    public RoomClosableEntrance entrance;
    public Vector2Int towardEntranceDir;
    public int entranceWidth;
}


public class Room : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private BattleTile battleTilePrefab;
    [SerializeField] private Vector3 battleTilesOffset;
    [SerializeField] private float startCameraSize;
    [SerializeField] private Tile[] holeTiles;
    [SerializeField] private Hole holePrefab;
    [SerializeField] private bool isDebugRoom;
    [SerializeField] private bool isBossRoom;
    [SerializeField] private bool isChallengeRoom;

    [Header("Private Infos")]
    private Vector2Int roomCoordinates;
    private Vector2Int roomSize;
    private Vector2Int roomsSizeUnits;
    private RoomEntrance[] roomEntrances;
    private List<BattleTile> battleTiles;
    private List<AIUnit> roomEnemies = new();
    private List<EnemySpawner> roomEnemySpawners;
    private bool battleIsDone;
    private List<RoomBlockableEntranceStruct> closedEntrances;
    private BattleTile[,] placedBattleTiles;
    private Vector2Int tabSize;

    [Header("Public Infos")]
    public Vector2Int RoomCoordinates { get { return roomCoordinates; } }
    public Vector2Int RoomSize { get { return roomSize; } }
    public RoomEntrance[] RoomEntrances { get { return roomEntrances; } }
    public List<AIUnit> RoomEnemies {  get { return roomEnemies; } }
    public BattleTile[,] PlacedBattleTiles {  get { return placedBattleTiles; } }
    public List<BattleTile> BattleTiles {  get { return battleTiles; } }

    [Header("References")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _battleGroundTilemap;
    [SerializeField] private Tilemap _wallsTilemap;
    [SerializeField] private Tilemap _bottomWallsTilemap;
    [SerializeField] private RoomBlockableEntranceStruct[] _blockableEntrances;
    [SerializeField] private Transform _spawnersParentTr;
    [SerializeField] private EnemySpawner _bossSpawner;

    [Header("Other References")]
    public Transform _heroSpawnerTr;
    public Transform _upLeftCornerTilemap;
    public Transform _upRightCornerTilemap;
    public Transform _bottomLeftCornerTilemap;
    public Transform _bottomRightCornerTilemap;



    private async void Start()
    {
        await Task.Delay(200);

        SetupBattleTiles();

        if (_spawnersParentTr == null) return;

        if (isDebugRoom)
        {
            await Task.Delay(200);

            StartBattle();
        }
    }


    #region Gen Pro Functions

    public void SetupRoom(Vector2Int roomCoordinates, Vector2Int roomsSizeUnits)
    {
        this.roomCoordinates = roomCoordinates;
        this.roomsSizeUnits = roomsSizeUnits;
        CreateRoomEntrances();
        roomSize = CalculateRoomSize();
    }

    public void CloseUnusedEntrances(List<Room> neighbors)
    {
        PlaceHoles();

        closedEntrances = new List<RoomBlockableEntranceStruct>();  // Needed to activate the corner tilemaps

        for (int i = 0; i < roomEntrances.Length; i++)
        {
            bool closeDoor = true;
            Vector2Int roomTowardPosition = roomEntrances[i].entranceDirection + roomCoordinates;

            for (int j = 0; j < neighbors.Count; j++)
            {
                if (!neighbors[j].VerifyRoomIsOnCoordinate(roomTowardPosition)) continue;

                if (!neighbors[j].VerifyHasDoorToward(roomCoordinates)) continue;

                closeDoor = false;
            }

            if (closeDoor)
            {
                _blockableEntrances[i].entrance.gameObject.SetActive(true);
                _blockableEntrances[i].entrance.ActivateBlockableEntrance();

                closedEntrances.Add(_blockableEntrances[i]);

                if(_upRightCornerTilemap != null)
                    VerifyCornersToActivate();
            }
        }
    }

    private void VerifyCornersToActivate()
    {
        bool didLeft = false;
        bool didRight = false;
        bool didUp = false;
        bool didDown = false;

        for(int i = 0; i < closedEntrances.Count; i++)
        {
            if (closedEntrances[i].towardEntranceDir.y > 0) didUp = true;
            if (closedEntrances[i].towardEntranceDir.y < 0) didDown = true;
            if (closedEntrances[i].towardEntranceDir.x > 0) didRight = true;
            if (closedEntrances[i].towardEntranceDir.x < 0) didLeft = true;

            if(didUp && didLeft) _upLeftCornerTilemap.gameObject.SetActive(true);
            if(didUp && didRight) _upRightCornerTilemap.gameObject.SetActive(true);
            if(didDown && didLeft) _bottomLeftCornerTilemap.gameObject.SetActive(true);
            if(didDown && didRight) _bottomRightCornerTilemap.gameObject.SetActive(true);
        }
    }

    private void PlaceHoles()
    {
        for (int x = _battleGroundTilemap.cellBounds.min.x; x <= _battleGroundTilemap.cellBounds.max.x; x++)
        {
            for (int y = _battleGroundTilemap.cellBounds.min.y; y <= _battleGroundTilemap.cellBounds.max.y; y++)
            {
                if (!_battleGroundTilemap.HasTile(new Vector3Int(x, y))) continue;
                if (!holeTiles.Contains(_battleGroundTilemap.GetTile(new Vector3Int(x, y)))) continue;

                Instantiate(holePrefab, _battleGroundTilemap.CellToWorld(new Vector3Int(x, y)) + new Vector3(1, 0.75f), 
                    Quaternion.Euler(0, 0, 0), transform);
            }
        }
    }

    private Vector2Int CalculateRoomSize()
    {
        Vector2Int bottomLeftCoordinates = new Vector2Int(100000, 100000);
        Vector2Int upRightCoordinates = new Vector2Int(-100000, -100000);

        for (int x = _groundTilemap.cellBounds.min.x; x <= _groundTilemap.cellBounds.max.x; x++)
        {
            for (int y = _groundTilemap.cellBounds.min.y; y <= _groundTilemap.cellBounds.max.y; y++)
            {
                if (!_groundTilemap.HasTile(new Vector3Int(x, y))) continue;

                if(bottomLeftCoordinates.x > x) bottomLeftCoordinates.x = x;    
                if(bottomLeftCoordinates.y > y) bottomLeftCoordinates.y = y;

                if (upRightCoordinates.x < x) upRightCoordinates.x = x;
                if (upRightCoordinates.y < y) upRightCoordinates.y = y;
            }
        }

        int roomWidth = Mathf.Abs(upRightCoordinates.x - bottomLeftCoordinates.x) + 1;
        int roomHeight = Mathf.Abs(upRightCoordinates.y - bottomLeftCoordinates.y) + 1;


        return new Vector2Int(roomWidth / roomsSizeUnits.x, roomHeight / roomsSizeUnits.y);
    }

    private void CreateRoomEntrances()
    {
        roomEntrances = new RoomEntrance[_blockableEntrances.Length];   

        for(int i = 0; i < _blockableEntrances.Length; i++)
        {
            RoomEntrance newEntrance = new RoomEntrance();
            newEntrance.entranceWidth = _blockableEntrances[i].entranceWidth;
            newEntrance.entranceDirection = _blockableEntrances[i].towardEntranceDir;
            roomEntrances[i] = (newEntrance);
        }
    }

    public bool VerifyRoomsCompatibility(Room otherRoom)
    {
        // Verifies the rooms dont overlap
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.x; y++)
            {
                if (otherRoom.VerifyRoomIsOnCoordinate(new Vector2Int(roomCoordinates.x + x, roomCoordinates.y + y))) return false;
            }
        }
        
        // Verifies the rooms have a common entrance point
        for (int i = 0; i < roomEntrances.Length; i++)
        {
            Vector2Int roomTowardPosition = roomEntrances[i].entranceDirection + roomCoordinates;
            if (!otherRoom.VerifyRoomIsOnCoordinate(roomTowardPosition)) continue;

            for(int j = 0; j < otherRoom.RoomEntrances.Length; j++)
            {
                Vector2Int otherRoomTowardPosition = otherRoom.RoomEntrances[j].entranceDirection + otherRoom.RoomCoordinates;
                if (!VerifyRoomIsOnCoordinate(otherRoomTowardPosition)) continue;

                return true;
            }
        }

        return false;
    }

    public bool VerifyRoomFitsPath(Vector2Int roomPos, List<Vector2Int> path)
    {
        int roomPathIndex = -1;
        int previousRoomIndex = -1;
        int nextRoomIndex = -1;

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == roomPos)
            {
                roomPathIndex = i;
                if (i != 0) previousRoomIndex = i - 1;
            }

            if(roomPathIndex != -1 && nextRoomIndex == -1)
            {
                if(!VerifyRoomIsOnCoordinate(path[i])) 
                {
                    nextRoomIndex = i;
                }
            }
        }

        if (nextRoomIndex != -1)
        {
            if (!VerifyHasDoorToward(path[nextRoomIndex])) return false;
        }
        if(previousRoomIndex != -1)
        {
            if (!VerifyHasDoorToward(path[previousRoomIndex])) return false;
        }

        return true;
    }

    public bool VerifyRoomIsOnCoordinate(Vector2Int checkedCoordinate)
    {
        for(int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.x; y++)
            {
                if(checkedCoordinate == new Vector2Int(roomCoordinates.x + x, roomCoordinates.y + y)) return true;
            }
        }

        return false;
    }

    public bool VerifyHasDoorToward(Vector2Int checkedTowardPosition)
    {
        for (int i = 0; i < roomEntrances.Length; i++)
        {
            Vector2Int roomTowardPosition = roomEntrances[i].entranceDirection + roomCoordinates;
            if (checkedTowardPosition == roomTowardPosition) return true;
        }

        return false;
    }

    #endregion


    #region Battle Functions

    public void StartBattle(bool isChallenge = false)
    {
        battleIsDone = true;

        if (!isBossRoom)
        {
            BattleManager.Instance.StartBattle(battleTiles, transform.position, startCameraSize, this);
            SetupSpawners(HeroesManager.Instance.Heroes);
            SetupEnemies(isChallenge);

            StartCoroutine(TutoManager.Instance.DisplayTutorialWithDelayCoroutine(3, 3.4f));
        }
        else
            StartCoroutine(DoBossIntroCoroutine());

        for (int i = 0; i < battleTiles.Count; i++)
        {
            StartCoroutine(battleTiles[i].ShowTileCoroutine(Random.Range(0, 0.2f)));
        }
    }

    public void EndBattle()
    {
        for (int i = 0; i < battleTiles.Count; i++)
        {
            StartCoroutine(battleTiles[i].HideTileCoroutine(Random.Range(0, 0.2f)));
        }
    }

    private void SetupBattleTiles()
    {
        battleTiles = new List<BattleTile>();
        tabSize = new Vector2Int(Mathf.Abs(_battleGroundTilemap.cellBounds.max.x - _battleGroundTilemap.cellBounds.min.x), Mathf.Abs(_battleGroundTilemap.cellBounds.max.y - _battleGroundTilemap.cellBounds.min.y));
        placedBattleTiles = new BattleTile[tabSize.x, tabSize.y];
         
        for (int x = _battleGroundTilemap.cellBounds.min.x; x < _battleGroundTilemap.cellBounds.max.x; x++)
        {
            for (int y = _battleGroundTilemap.cellBounds.min.y; y < _battleGroundTilemap.cellBounds.max.y; y++)
            {
                if (!_battleGroundTilemap.HasTile(new Vector3Int(x, y))) continue;

                Vector2Int currentCoordinates = new Vector2Int(x - _battleGroundTilemap.cellBounds.min.x, y - _battleGroundTilemap.cellBounds.min.y);
                Vector3 tilePosWorld = _battleGroundTilemap.CellToWorld(new Vector3Int(x, y));
                BattleTile newTile = Instantiate(battleTilePrefab, tilePosWorld + battleTilesOffset, Quaternion.Euler(0, 0, 0), transform);
                battleTiles.Add(newTile);

                newTile.SetupBattleTile(currentCoordinates, holeTiles.Contains(_battleGroundTilemap.GetTile(new Vector3Int(x, y))));
                StartCoroutine(newTile.HideTileCoroutine(Random.Range(0, 0.2f)));

                placedBattleTiles[currentCoordinates.x, currentCoordinates.y] = newTile;
            }
        }

        for (int x = 0; x < tabSize.x; x++)
        {
            for (int y = 0; y < tabSize.y; y++)
            {
                if (placedBattleTiles[x, y] == null) continue;

                if(x < tabSize.x - 1)
                    if (placedBattleTiles[x + 1, y] != null) placedBattleTiles[x, y].AddNeighbor(placedBattleTiles[x + 1, y]);

                if (y < tabSize.y - 1)
                    if (placedBattleTiles[x, y + 1] != null) placedBattleTiles[x, y].AddNeighbor(placedBattleTiles[x, y + 1]);

                if(x > 0)
                    if (placedBattleTiles[x - 1, y] != null) placedBattleTiles[x, y].AddNeighbor(placedBattleTiles[x - 1, y]);

                if (y > 0)
                    if (placedBattleTiles[x, y - 1] != null) placedBattleTiles[x, y].AddNeighbor(placedBattleTiles[x, y - 1]);
            }
        }
    }

    private void SetupSpawners(Hero[] heroes)
    {
        roomEnemySpawners = _spawnersParentTr.GetComponentsInChildren<EnemySpawner>().ToList();

        for (int i = roomEnemySpawners.Count - 1; i >= 0; i--)
        {
            roomEnemySpawners[i].AssociatedTile = GetNearestBattleTile(roomEnemySpawners[i].transform.position);

            for (int j = 0; j < heroes.Length; j++) 
            {
                if (heroes[j].CurrentHealth <= 0) continue;

                int dist = (int)Vector2Int.Distance(heroes[j].CurrentTile.TileCoordinates, roomEnemySpawners[i].AssociatedTile.TileCoordinates);
                if (dist <= 2 && TutoManager.Instance.DidBattleTuto) 
                {
                    roomEnemySpawners.RemoveAt(i);
                    break;
                }
            }
        }
    }


    public IEnumerator DoBossIntroCoroutine()
    {
        BattleManager.Instance.StartBattle(battleTiles, transform.position, startCameraSize, this, 5.5f);
        SetupSpawners(HeroesManager.Instance.Heroes);

        EnemySpawnData enemySpawnData = ProceduralGenerationManager.Instance.EnviroData.enemySpawnsPerFloor[ProceduralGenerationManager.Instance.CurrentFloor];
        int currentMin = isChallengeRoom ? enemySpawnData.minDangerAmountPerChallenge : enemySpawnData.minDangerAmountPerBattle;
        int currentMax = isChallengeRoom ? enemySpawnData.maxDangerAmountPerChallenge : enemySpawnData.maxDangerAmountPerBattle;

        // We spawn the boss
        (EnemySpawn boss, bool isElite) = _bossSpawner.GetSpawnedEnemy(currentMax);
        AIUnit newEnemy = Instantiate(boss.enemyPrefab as AIUnit, transform);
        newEnemy.Initialise(isElite);
        newEnemy.MoveUnit(_bossSpawner.AssociatedTile);

        roomEnemies.Add(newEnemy);
        roomEnemySpawners.Remove(_bossSpawner);
        BattleManager.Instance.AddUnit(newEnemy);
        newEnemy.EnterBattle(newEnemy.CurrentTile);

        yield return new WaitForSeconds(1f);

        // Camera Movement + Boss Anim
        CameraManager.Instance.FocusOnTransform(newEnemy.transform, 3);
        newEnemy._animator.SetTrigger("Intro");

        yield return new WaitForSeconds(4f);

        CameraManager.Instance.FocusOnPosition(transform.position, startCameraSize);
        SetupEnemies(false);
    }


    private BattleTile GetNearestBattleTile(Vector2 position)
    {
        float bestDist = Mathf.Infinity;
        BattleTile pickedTile = null;

        for (int i = 0; i< battleTiles.Count; i++)
        {
            float currentDist = Vector2.Distance(position, battleTiles[i].transform.position);
            if(currentDist < bestDist)
            {
                bestDist = currentDist;
                pickedTile = battleTiles[i];    
            }
        }

        return pickedTile;
    }

    private void SetupEnemies(bool isChallenge)
    {
        int currentDangerAmount = 0, currentSpawnedCount = 0, antiCrashCounter = 0;
        Dictionary<Unit, int> spawnCountPerEnemy = new Dictionary<Unit, int>();
        EnemySpawnData enemySpawnData = ProceduralGenerationManager.Instance.EnviroData.enemySpawnsPerFloor[ProceduralGenerationManager.Instance.CurrentFloor];
        int currentMin = isChallengeRoom ? enemySpawnData.minDangerAmountPerChallenge : enemySpawnData.minDangerAmountPerBattle;
        int currentMax = isChallengeRoom ? enemySpawnData.maxDangerAmountPerChallenge : enemySpawnData.maxDangerAmountPerBattle;

        while (currentDangerAmount < currentMin && antiCrashCounter++ < 100 && roomEnemySpawners.Count > 0)
        {
            int pickedSpawnerIndex = Random.Range(0, roomEnemySpawners.Count);
            EnemySpawner currentSpawner = roomEnemySpawners[pickedSpawnerIndex];

            (EnemySpawn wantedUnit, bool isElite) = currentSpawner.GetSpawnedEnemy(currentMax - currentDangerAmount, isChallenge);
            if (!isDebugRoom)
            {
                if (wantedUnit is null) continue;
                if (wantedUnit.minEnemyCountBeforeSpawn > currentSpawnedCount) continue;
                if (spawnCountPerEnemy.ContainsKey(wantedUnit.enemyPrefab) && spawnCountPerEnemy[wantedUnit.enemyPrefab] >= wantedUnit.maxCountPerBattle) continue;
            }

            if(wantedUnit.enemyPrefab.GetType() == typeof(AIUnit))
            {
                currentSpawnedCount++;
                if (spawnCountPerEnemy.ContainsKey(wantedUnit.enemyPrefab)) spawnCountPerEnemy[wantedUnit.enemyPrefab]++;
                else spawnCountPerEnemy.Add(wantedUnit.enemyPrefab, 1);

                AIUnit newEnemy = Instantiate(wantedUnit.enemyPrefab as AIUnit, transform);
                newEnemy.Initialise(isElite);
                newEnemy.MoveUnit(currentSpawner.AssociatedTile);

                currentDangerAmount += (wantedUnit.enemyPrefab as AIUnit).AIData.dangerLevel;

                roomEnemies.Add(newEnemy);
                roomEnemySpawners.Remove(currentSpawner);
                BattleManager.Instance.AddUnit(newEnemy);
                newEnemy.EnterBattle(newEnemy.CurrentTile);
            }
            else
            {
                Hero newHero = Instantiate(wantedUnit.enemyPrefab as Hero, transform);
                newHero.MoveUnit(currentSpawner.AssociatedTile);

                roomEnemySpawners.Remove(currentSpawner);
                BattleManager.Instance.AddUnit(newHero);
                newHero.EnterBattle(newHero.CurrentTile);
            }
        }
    }


    public BattleTile GetBattleTile(Vector2Int coordinates)
    {
        if (coordinates.x >= tabSize.x)
            return null;
        if (coordinates.y >= tabSize.y)
            return null;
        if (coordinates.x < 0)
            return null;
        if (coordinates.y < 0)
            return null;

        return PlacedBattleTiles[coordinates.x, coordinates.y];
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (battleIsDone) return;

        if (collision.CompareTag("Hero"))
        {
            StartBattle();
        }
    }

    #endregion
}
