using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ProceduralGenerationManager : GenericSingletonClass<ProceduralGenerationManager>
{
    private enum RoomType
    {
        Battle,
        Corridor,
        CorridorTrap,
        CorridorJump,
        Trap,
        Jump,
        Challenge,
        Trial
    }

    [Header("Parameters")]
    [SerializeField] private EnviroData enviroData;
    [SerializeField] private EnviroData tutoData;
    [SerializeField] private RoomGlobalCollider roomGlobalColliderPredab;
    [SerializeField] private Room[] tutorialRooms;
    [SerializeField] private Vector2Int roomSizeUnits;
    [SerializeField] private Vector2 offsetRoomCenter;
    [SerializeField] private bool noGeneration;
    
    [Header("Private Infos")]
    private int wantedRoomAmount;
    private List<Room> generatedRooms;
    private Vector3 spawnPos;
    private int currentFloor;
    public int[] trailFloorsIndexes;

    [Header("Public Infos")]
    public int CurrentFloor { get { return currentFloor; } }
    public EnviroData EnviroData { get { return enviroData; } }

    [Header("References")]
    [SerializeField] private HeroesManager _heroesManager;
    [SerializeField] private SpriteLayererManager _spriteLayererManager;
    [SerializeField] private Transform _roomsParent;
    private GenProPathCalculator _pathCalculator;


    #region Start / End Exploration

    public void StartExploration(EnviroData enviroData, bool isTuto)
    {
        this.enviroData = enviroData;
        _heroesManager.StartExploration(spawnPos);

        trailFloorsIndexes = new int[2];
        trailFloorsIndexes[0] = Random.Range(0, 2);
        trailFloorsIndexes[1] = Random.Range(3, 5);

        if (isTuto)
        {
            this.enviroData = tutoData;

            TutoManager.Instance.StartTutorial();
            GenerateTutorialFloor();

            StartCoroutine(_spriteLayererManager.InitialiseAllCoroutine(0.15f));
            StartCoroutine(TutoManager.Instance.DisplayTutorialWithDelayCoroutine(0, 1.5f));
            return;
        }

        if (!noGeneration)
            GenerateFloor(enviroData);
        StartCoroutine(_spriteLayererManager.InitialiseAllCoroutine(0.15f));
    }


    public void EndExploration()
    {
        Transform[] objToDestroy = _roomsParent.GetComponentsInChildren<Transform>();

        foreach (Transform obj in objToDestroy)
        {
            if (obj == transform) continue;
            Destroy(obj.gameObject);
        }
    }

    #endregion


    #region Generate Floors

    public void GenerateNextFloor()
    {
        Transform[] roomsToDestroy = _roomsParent.GetComponentsInChildren<Transform>();
        currentFloor++;

        for (int i = 0; i < roomsToDestroy.Length; i++)
        {
            if (roomsToDestroy[i] == _roomsParent) continue;

            Destroy(roomsToDestroy[i].gameObject);
        }

        if (currentFloor == 2) GenerateBossFloor(true);
        else if (currentFloor == 5) GenerateBossFloor(false);
        else GenerateFloor(enviroData);
    }

    public void GenerateFloor(EnviroData enviroData)
    {
        generatedRooms = new List<Room>();

        this.enviroData = enviroData;
        wantedRoomAmount = Random.Range(enviroData.minRoomAmount, enviroData.maxRoomAmount);

        int tabSize = wantedRoomAmount * 2;
        _pathCalculator = new GenProPathCalculator(tabSize);

        GenerateStartAndEnd(new Vector2Int(wantedRoomAmount, wantedRoomAmount));
        GenerateAlternativePathes(2);
        GenerateDeadEnds(3);

        if(currentFloor == trailFloorsIndexes[0] || currentFloor == trailFloorsIndexes[1])
        {
            GenerateTrialRoom();
        }

        CloseUnusedEntrances();

        UIManager.Instance.Minimap.SetupMinimap(_pathCalculator.floorGenProTiles, _pathCalculator);
    }

    public void GenerateBossFloor(bool isFirstBoss)
    {
        generatedRooms = new List<Room>();

        Room[] possibleBossRooms = isFirstBoss ? enviroData.possibleFirstBossRooms : enviroData.possibleSecondBossRooms;

        AddRoom(new Vector2Int(4, 4), enviroData.possibleStartRooms[Random.Range(0, enviroData.possibleStartRooms.Length)]);
        spawnPos = generatedRooms[0]._heroSpawnerTr.position;
        HeroesManager.Instance.Teleport(spawnPos);

        AddRoom(new Vector2Int(4, 5), possibleBossRooms[Random.Range(0, possibleBossRooms.Length)]);
        AddRoom(new Vector2Int(4, 6), enviroData.possibleStairsRooms[Random.Range(0, enviroData.possibleStairsRooms.Length)]);

        CloseUnusedEntrances();

        UIManager.Instance.Minimap.SetupMinimap(_pathCalculator.floorGenProTiles, _pathCalculator);
    }

    private void GenerateStartAndEnd(Vector2Int centerPosition)
    {
        // Start
        AddRoom(centerPosition, enviroData.possibleStartRooms[Random.Range(0, enviroData.possibleStartRooms.Length)]);
        spawnPos = generatedRooms[0]._heroSpawnerTr.position;

        HeroesManager.Instance.Teleport(spawnPos);

        // End
        Vector2Int endPos = new Vector2Int(0, 0);
        int antiCrashCounter = 0;
        while ((_pathCalculator.GetManhattanDistance(centerPosition, endPos) > enviroData.maxDistGoldenPath ||
            _pathCalculator.GetManhattanDistance(centerPosition, endPos) < enviroData.minDistGoldenPath) && ++antiCrashCounter < 1000)
        {
            endPos = new Vector2Int(centerPosition.x + Random.Range(-enviroData.minDistGoldenPath, enviroData.minDistGoldenPath),
                centerPosition.y + Random.Range(2, enviroData.maxDistGoldenPath));
        }

        AddRoom(endPos, enviroData.possibleStairsRooms[Random.Range(0, enviroData.possibleStairsRooms.Length)]);
        GeneratePath(centerPosition, endPos, (int)(_pathCalculator.GetManhattanDistance(centerPosition, endPos) * 0.5f));
    }

    private void GenerateAlternativePathes(int alternativePathesCount)
    {
        for(int i = 0; i < alternativePathesCount; ++i)
        {
            Room baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
            Vector2Int firstRoomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
            int antiCrash = 0;

            while((_pathCalculator.floorGenProTiles[firstRoomCoordinates.x, firstRoomCoordinates.y].tileRoom != null || 
                !baseRoom.VerifyHasDoorToward(firstRoomCoordinates)) && antiCrash++ < 200)
            {
                baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
                firstRoomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
            }

            baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
            Vector2Int secondRoomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
            antiCrash = 0;
            List<Vector2Int> path = _pathCalculator.GetPath(firstRoomCoordinates, secondRoomCoordinates, true, true, true);

            while ((_pathCalculator.floorGenProTiles[secondRoomCoordinates.x, secondRoomCoordinates.y].tileRoom != null ||
                !baseRoom.VerifyHasDoorToward(secondRoomCoordinates) || path.Count < 2 || path.Count > 4) && antiCrash++ < 200)
            {
                baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
                secondRoomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
                path = _pathCalculator.GetPath(firstRoomCoordinates, secondRoomCoordinates, true, true, true);
            }

            GeneratePath(firstRoomCoordinates, secondRoomCoordinates, 
                (int)(path.Count * 0.5f), true);
        }
    }

    private void GenerateDeadEnds(int deadEndsCount)
    {
        for (int i = 0; i < deadEndsCount; ++i)
        {
            Room baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
            Room newRoom = GetRoomFromType(i == deadEndsCount - 1 ? (RoomType)6 : (RoomType)Random.Range(4, 6));
            Vector2Int roomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
            newRoom.SetupRoom(roomCoordinates, roomSizeUnits);
            int antiCrash = 0;

            while ((_pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y].tileRoom != null ||
                !baseRoom.VerifyHasDoorToward(roomCoordinates) || !newRoom.VerifyHasDoorToward(baseRoom.RoomCoordinates)) && antiCrash++ < 200)
            {
                baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
                newRoom = GetRoomFromType(i == deadEndsCount - 1 ? (RoomType)6 : (RoomType)Random.Range(4, 6));
                roomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
                newRoom.SetupRoom(roomCoordinates, roomSizeUnits);
            }

            AddRoom(roomCoordinates, newRoom);
        }
    }

    private void GenerateTrialRoom()
    {
        Room baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
        Room newRoom = GetRoomFromType(RoomType.Trial);
        Vector2Int roomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
        newRoom.SetupRoom(roomCoordinates, roomSizeUnits);
        int antiCrash = 0;

        while ((_pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y].tileRoom != null ||
            !baseRoom.VerifyHasDoorToward(roomCoordinates) || !newRoom.VerifyHasDoorToward(baseRoom.RoomCoordinates)) && antiCrash++ < 200)
        {
            baseRoom = generatedRooms[Random.Range(0, generatedRooms.Count)];
            newRoom = GetRoomFromType(RoomType.Trial);
            roomCoordinates = baseRoom.RoomCoordinates + GetRandomDirection();
            newRoom.SetupRoom(roomCoordinates, roomSizeUnits);
        }

        AddRoom(roomCoordinates, newRoom);
    }

    private void CloseUnusedEntrances()
    {
        for(int i = 0; i < generatedRooms.Count; i++)
        {
            generatedRooms[i].CloseUnusedEntrances(GetNeighborRooms(generatedRooms[i].RoomCoordinates, generatedRooms[i]));
        }
    }

    #endregion


    #region Utility Functions

    private Vector2Int GetRandomDirection()
    {
        int random = Random.Range(0, 4);
        switch(random)
        {
            case 0:
                return new Vector2Int(1, 0);

            case 1:
                return new Vector2Int(-1, 0);

            case 2:
                return new Vector2Int(0, 1);

            case 3:
                return new Vector2Int(0, -1);

        }

        return Vector2Int.zero;
    }

    private RoomType[] GetPathRoomTypes(Vector2Int[] path, int wantedBattleRoomAmount)
    {
        RoomType[] result = new RoomType[path.Length];

        while (true)
        {
            int battleRoomAmount = 0;

            for(int i = 0; i < result.Length; i++)
            {
                result[i] = (RoomType)Random.Range(0, 4);
                if (result[i] == RoomType.Battle) battleRoomAmount++;
            }

            if(battleRoomAmount == wantedBattleRoomAmount)
            {
                return result;
            }
        }
    }

    private Room GetRoomFromType(RoomType roomType)
    {
        switch(roomType)
        {
            case RoomType.Battle:
                return enviroData.possibleBattleRooms[Random.Range(0, enviroData.possibleBattleRooms.Length)];

            case RoomType.Corridor:
                return enviroData.possibleCorridorRooms[Random.Range(0, enviroData.possibleCorridorRooms.Length)];

            case RoomType.CorridorTrap:
                return enviroData.possibleCorridorTrapRooms[Random.Range(0, enviroData.possibleCorridorTrapRooms.Length)];

            case RoomType.CorridorJump:
                return enviroData.possibleCorridorPlateformRooms[Random.Range(0, enviroData.possibleCorridorPlateformRooms.Length)];

            case RoomType.Trap:
                return enviroData.possibleTrapRooms[Random.Range(0, enviroData.possibleTrapRooms.Length)];

            case RoomType.Jump:
                return enviroData.possiblePlateformRooms[Random.Range(0, enviroData.possiblePlateformRooms.Length)];

            case RoomType.Challenge:
                return enviroData.possibleChallengeRooms[Random.Range(0, enviroData.possibleChallengeRooms.Length)];

            case RoomType.Trial:
                return enviroData.possiblePuzzleRooms[Random.Range(0, enviroData.possiblePuzzleRooms.Length)];
        }

        return null;
    }

    private void GeneratePath(Vector2Int start, Vector2Int end, int wantedBattleRoomAmount, bool includeStartEnd = false)
    {
        List<Vector2Int> path = _pathCalculator.GetPath(start, end, includeStartEnd, includeStartEnd, true);
        int antiCrashCounter = 0;
        int currentPathIndex = 0;

        RoomType[] pathRoomTypes = GetPathRoomTypes(path.ToArray(), wantedBattleRoomAmount);

        while(!VerifyPathIsValid(path) && ++antiCrashCounter < 1000)
        {
            Room testedRoom = GetRoomFromType(pathRoomTypes[currentPathIndex]);
            Vector2Int currentCoordinates = path[0];
            bool found = false;

            for (int i = 0; i < path.Count; i++)
            {
                if (_pathCalculator.floorGenProTiles[path[i].x, path[i].y].tileRoom != null) continue;
                testedRoom.SetupRoom(path[i], roomSizeUnits);

                if (!VerifyRoomFits(path[i], testedRoom)) continue;
                if (!testedRoom.VerifyRoomFitsPath(path[i], path)) continue;

                currentCoordinates = path[i];
                found = true;
                break;
            }

            if (found)
            {
                AddRoom(currentCoordinates, testedRoom);
                currentPathIndex++;
            }
        }
    }


    private void AddRoom(Vector2Int spawnCoordinates, Room room)
    {
        Room newRoom = Instantiate(room, (Vector2)(spawnCoordinates * roomSizeUnits * new Vector2(2, 1.5f)) + offsetRoomCenter, Quaternion.Euler(0, 0, 0),
            _roomsParent);
        newRoom.SetupRoom(spawnCoordinates, roomSizeUnits);

        _pathCalculator.AddRoom(newRoom, spawnCoordinates);
        generatedRooms.Add(newRoom);

        RoomGlobalCollider globalCollider = Instantiate(roomGlobalColliderPredab, newRoom.transform);
        globalCollider.transform.localPosition = Vector3.zero;
        globalCollider.Setup(newRoom);
    }


    private bool VerifyPathIsValid(List<Vector2Int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (_pathCalculator.floorGenProTiles[path[i].x, path[i].y].tileRoom == null) return false;
        }
        return true;
    }


    private bool VerifyRoomFits(Vector2Int coordinates, Room room)
    {
        List<Room> neighborRooms = GetNeighborRooms(coordinates, room);

        for (int i = 0; i < neighborRooms.Count; i++)
        {
            if (!room.VerifyRoomsCompatibility(neighborRooms[i])) return false;
        }

        return true;
    }

    private List<Room> GetNeighborRooms(Vector2Int coordinates, Room room)
    {
        List<Room> neighborRooms = new List<Room>();

        // We verify if the room doesnt overlap another and we get the neighbor rooms
        for (int x = 0; x < room.RoomSize.x; x++)
        {
            for (int y = 0; y < room.RoomSize.y; y++)
            {
                Vector2Int currentCoordinates = new Vector2Int(coordinates.x + x, coordinates.y + y);

                //if (_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y].tileRoom != null) continue;

                if (_pathCalculator.floorGenProTiles[currentCoordinates.x + 1, currentCoordinates.y].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x + 1, currentCoordinates.y].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x + 1, currentCoordinates.y].tileRoom);
                    }
                }
                if (_pathCalculator.floorGenProTiles[currentCoordinates.x - 1, currentCoordinates.y].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x - 1, currentCoordinates.y].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x - 1, currentCoordinates.y].tileRoom);
                    }
                }

                if (_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y + 1].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y + 1].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y + 1].tileRoom);
                    }
                }

                if (_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y - 1].tileRoom != null)
                {
                    if (!neighborRooms.Contains(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y - 1].tileRoom))
                    {
                        neighborRooms.Add(_pathCalculator.floorGenProTiles[currentCoordinates.x, currentCoordinates.y - 1].tileRoom);
                    }
                }
            }
        }

        return neighborRooms;
    }


    #endregion


    #region Tutorial Functions

    private void GenerateTutorialFloor()
    {
        generatedRooms = new List<Room>();

        wantedRoomAmount = tutorialRooms.Length;
        Vector2Int currentPos = new Vector2Int(wantedRoomAmount, wantedRoomAmount);

        _pathCalculator = new GenProPathCalculator(wantedRoomAmount * 2);

        for (int i = 0; i < wantedRoomAmount; i++)
        {
            AddRoom(currentPos, tutorialRooms[i]);
            currentPos += new Vector2Int(0, 1);
        }

        spawnPos = generatedRooms[0]._heroSpawnerTr.position;
        HeroesManager.Instance.Teleport(spawnPos);

        CloseUnusedEntrances();

        UIManager.Instance.Minimap.SetupMinimap(_pathCalculator.floorGenProTiles, _pathCalculator);
    }

    #endregion
}
