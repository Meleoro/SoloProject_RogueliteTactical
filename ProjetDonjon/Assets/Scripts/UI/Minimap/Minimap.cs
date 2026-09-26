using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Minimap : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private MinimapRoom roomPrefab;
    [SerializeField] private Vector2 roomSize;

    [Header("Private Infos")]
    private MinimapRoom[,] minimapRooms = new MinimapRoom[0, 0];
    private List<MinimapRoom> roomsList = new List<MinimapRoom>();

    [Header("References")]
    private GenProPathCalculator _pathCalculator;
    [SerializeField] private RectTransform _roomsParent;


    #region Setup

    public void SetupMinimap(GenProTile[,] rooms, GenProPathCalculator pathCalculator)
    {
        _pathCalculator = pathCalculator;

        for(int i = roomsList.Count - 1; i >= 0; i--)
        {
            Destroy(roomsList[i].gameObject);
        }

        minimapRooms = new MinimapRoom[rooms.GetLength(0), rooms.GetLength(1)];
        roomsList.Clear();

        for (int x = 0; x < rooms.GetLength(0); x++)
        {
            for (int y = 0; y < rooms.GetLength(1); y++)
            {
                if (rooms[x, y].tileRoom is null)
                {
                    minimapRooms[x, y] = null;
                    continue;
                }

                MinimapRoom newRoom = Instantiate(roomPrefab, _roomsParent);
                newRoom.SetupRoom(new Vector2Int(x, y));
                newRoom.RectTr.localPosition = new Vector3(x * roomSize.x, y * roomSize.y, 0);

                roomsList.Add(newRoom);
                minimapRooms[x,y] = newRoom;
            }
        }

        foreach(MinimapRoom minimapRoom in roomsList)
        {
            MinimapRoom[] neighbors = GetRoomNeighbors(minimapRoom.Coodinates);
            minimapRoom.SetupNeighborsAndEntrances(neighbors);
        }
    }

    private MinimapRoom[] GetRoomNeighbors(Vector2Int roomCoordinates)
    {
        List<MinimapRoom> neighbors = new List<MinimapRoom>();

        if (_pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y + 1].tileRoom is not null)
        {
            if (_pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y + 1].tileRoom.VerifyHasDoorToward(roomCoordinates) &&
                _pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y].tileRoom.VerifyHasDoorToward(new Vector2Int(roomCoordinates.x, roomCoordinates.y + 1)))
            {
                neighbors.Add(minimapRooms[roomCoordinates.x, roomCoordinates.y + 1]);
            }
        }

        if (_pathCalculator.floorGenProTiles[roomCoordinates.x + 1, roomCoordinates.y].tileRoom is not null)
        {
            if (_pathCalculator.floorGenProTiles[roomCoordinates.x + 1, roomCoordinates.y].tileRoom.VerifyHasDoorToward(roomCoordinates) &&
                _pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y].tileRoom.VerifyHasDoorToward(new Vector2Int(roomCoordinates.x + 1, roomCoordinates.y)))
            {
                neighbors.Add(minimapRooms[roomCoordinates.x + 1, roomCoordinates.y]);
            }
        }

        if (_pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y - 1].tileRoom is not null)
        {
            if (_pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y - 1].tileRoom.VerifyHasDoorToward(roomCoordinates) && 
                _pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y].tileRoom.VerifyHasDoorToward(new Vector2Int(roomCoordinates.x, roomCoordinates.y - 1)))
            {
                neighbors.Add(minimapRooms[roomCoordinates.x, roomCoordinates.y - 1]);
            }
        }

        if (_pathCalculator.floorGenProTiles[roomCoordinates.x - 1, roomCoordinates.y].tileRoom is not null)
        {
            if (_pathCalculator.floorGenProTiles[roomCoordinates.x - 1, roomCoordinates.y].tileRoom.VerifyHasDoorToward(roomCoordinates) &&
                _pathCalculator.floorGenProTiles[roomCoordinates.x, roomCoordinates.y].tileRoom.VerifyHasDoorToward(new Vector2Int(roomCoordinates.x - 1, roomCoordinates.y)))
            {
                neighbors.Add(minimapRooms[roomCoordinates.x - 1, roomCoordinates.y]);
            }
        }


        return neighbors.ToArray();
    }

    #endregion


    public void EnterRoom(Vector2Int roomCoordinates)
    {
        MoveMap(roomCoordinates);

        minimapRooms[roomCoordinates.x, roomCoordinates.y].EnterRoom();
    }

    private void MoveMap(Vector2Int roomCoordinates)
    {
        Vector2 offset = minimapRooms[roomCoordinates.x, roomCoordinates.y].RectTr.transform.position - transform.position;

        _roomsParent.transform.position -= (Vector3)offset;
    }
}
