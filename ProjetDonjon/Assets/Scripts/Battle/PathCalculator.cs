using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static Enums;

public class PathCalculator
{

    #region Path Calculator Tiles Management

    public struct PathCalculatorTile
    {
        public PathCalculatorTile(BattleTile battleTile, Vector2Int tilePos)
        {
            this.battleTile = battleTile;
            this.tilePos = tilePos;
            isBlocked = false;
            previousPosition = Vector2Int.zero;
        }

        public BattleTile battleTile;
        public Vector2Int tilePos;
        public bool isBlocked;
        public Vector2Int previousPosition;
    }

    private PathCalculatorTile[,] pathCalculatorTiles;

    public void InitialisePathCalculator(BattleTile[,] tiles)
    {
        pathCalculatorTiles = new PathCalculatorTile[tiles.GetLength(0), tiles.GetLength(1)];

        for (int x = 0; x < pathCalculatorTiles.GetLength(0); x++)
        {
            for (int y = 0; y < pathCalculatorTiles.GetLength(1); y++)
            {
                pathCalculatorTiles[x, y] = new PathCalculatorTile(tiles[x, y], new Vector2Int(x, y));
            }
        }
    }

    public void ActualisePathCalculatorTiles(BattleTile[,] tiles)
    {
        for (int x = 0; x < pathCalculatorTiles.GetLength(0); x++)
        {
            for (int y = 0; y < pathCalculatorTiles.GetLength(1); y++)
            {
                if (tiles[x, y] is null)
                {
                    pathCalculatorTiles[x, y].isBlocked = true;
                    continue;
                }

                if (tiles[x, y].UnitOnTile is not null)
                    pathCalculatorTiles[x, y].isBlocked = true;
                else
                    pathCalculatorTiles[x, y].isBlocked = false;
            }
        }

    }

    #endregion


    #region Get Path

    public List<Vector2Int> GetPath(Vector2Int start, Vector2Int end, bool canUseDiagonals, bool includeEndStart = true)
    {
        List<Vector2Int> openList = new List<Vector2Int>();
        List<Vector2Int> closedList = new List<Vector2Int>();

        openList.Add(start);

        int antiCrashCounter = 0;
        while (antiCrashCounter++ < 100 && openList.Count > 0)
        {
            int bestDist = 10000;
            int pickedIndex = 0;
            for (int i = 0; i < openList.Count; i++)
            {
                int currentDist = GetManhattanDistance(end, openList[i]);
                currentDist += GetManhattanDistance(start, openList[i]);

                if (currentDist < bestDist)
                {
                    bestDist = currentDist;
                    pickedIndex = i;
                }
            }

            Vector2Int pickedPos = openList[pickedIndex];
            openList.RemoveAt(pickedIndex);
            closedList.Add(pickedPos);

            if (pickedPos == end)
            {
                return GetFinalPath(start, end, includeEndStart, includeEndStart);
            }

            List<Vector2Int> pickedLocationNeighbors = GetPossibleNeighborLocations(pickedPos, canUseDiagonals);
            for (int i = 0; i < pickedLocationNeighbors.Count; i++)
            {
                if (openList.Contains(pickedLocationNeighbors[i])) continue;
                if (closedList.Contains(pickedLocationNeighbors[i])) continue;

                openList.Add(pickedLocationNeighbors[i]);
                pathCalculatorTiles[pickedLocationNeighbors[i].x, pickedLocationNeighbors[i].y].previousPosition = pickedPos;
            }
        }

        return new List<Vector2Int>();
    }


    private List<Vector2Int> GetFinalPath(Vector2Int startLocation, Vector2Int endLocation, bool includeStart = true, bool includeEnd = true)
    {
        List<Vector2Int> finalPath = new List<Vector2Int>();
        PathCalculatorTile currentTile = pathCalculatorTiles[endLocation.x, endLocation.y];
        currentTile = pathCalculatorTiles[currentTile.previousPosition.x, currentTile.previousPosition.y];

        if (includeEnd) finalPath.Add(endLocation);

        while (currentTile.tilePos != startLocation)
        {
            finalPath.Add(currentTile.tilePos);

            currentTile = pathCalculatorTiles[currentTile.previousPosition.x, currentTile.previousPosition.y];
        }

        if (includeStart) finalPath.Add(startLocation);

        finalPath.Reverse();
        return finalPath;
    }


    private int GetManhattanDistance(Vector2Int point1, Vector2Int point2)
    {
        return Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y);
    }


    private List<Vector2Int> GetPossibleNeighborLocations(Vector2Int startLocation, bool diagonal = false, bool takeBlocked = false, bool takeHole = false)
    {
        List<Vector2Int> returnedList = new List<Vector2Int>();

        // Left
        if (VerifyNeighborTile(startLocation + new Vector2Int(-1, 0), takeBlocked, takeHole))
        {
            returnedList.Add(startLocation + new Vector2Int(-1, 0));
        }

        // Right
        if (VerifyNeighborTile(startLocation + new Vector2Int(1, 0), takeBlocked, takeHole))
        {
            returnedList.Add(startLocation + new Vector2Int(1, 0));
        }

        // Up
        if (VerifyNeighborTile(startLocation + new Vector2Int(0, 1), takeBlocked, takeHole))
        {
            returnedList.Add(startLocation + new Vector2Int(0, 1));
        }

        // Down
        if (VerifyNeighborTile(startLocation + new Vector2Int(0, -1), takeBlocked, takeHole))
        {
            returnedList.Add(startLocation + new Vector2Int(0, -1));
        }

        if (diagonal) 
        {
            // Bottom Left
            if (VerifyNeighborTile(startLocation + new Vector2Int(-1, -1), takeBlocked, takeHole))
            {
                returnedList.Add(startLocation + new Vector2Int(-1, -1));
            }

            // Down Right
            if (VerifyNeighborTile(startLocation + new Vector2Int(1, -1), takeBlocked, takeHole))
            {
                returnedList.Add(startLocation + new Vector2Int(1, -1));
            }

            // Up Right
            if (VerifyNeighborTile(startLocation + new Vector2Int(1, 1), takeBlocked, takeHole))
            {
                returnedList.Add(startLocation + new Vector2Int(1, 1));
            }

            // Up Left
            if (VerifyNeighborTile(startLocation + new Vector2Int(-1, 1), takeBlocked, takeHole))
            {
                returnedList.Add(startLocation + new Vector2Int(-1, 1));
            }
        }

        return returnedList;
    }

    private bool VerifyNeighborTile(Vector2Int verifiedLocation, bool takeBlocked, bool takeHole)
    {
        if (verifiedLocation.x < 0 || verifiedLocation.x >= pathCalculatorTiles.GetLength(0) || verifiedLocation.y < 0 || verifiedLocation.y >= pathCalculatorTiles.GetLength(1)) return false;

        if (pathCalculatorTiles[verifiedLocation.x, verifiedLocation.y].battleTile is null) return false;

        if (takeHole) return true;

        if (pathCalculatorTiles[verifiedLocation.x, verifiedLocation.y].battleTile.IsHole) return false;

        if (takeBlocked) return true;

        if (pathCalculatorTiles[verifiedLocation.x, verifiedLocation.y].isBlocked) return false;

        return true;
    }


    #endregion



    public bool VerifyIsReachable(Vector2Int start, Vector2Int end, bool useDiagonals, ObstacleType obstacleType, BattleTile ignoredTile, bool includeEnd)
    {
        Vector2Int dir = end - start;

        if(dir.x > 0)
        {
            dir.x = 1;
        } 
        else if (dir.x < 0)
        {
            dir.x = -1;
        }

        if (dir.y > 0)
        {
            dir.y = 1;
        }
        else if (dir.y < 0)
        {
            dir.y = -1;
        }

        PathCalculatorTile currentTile = pathCalculatorTiles[start.x + dir.x, start.y + dir.y];
        Vector2Int currentCoord;
        while(currentTile.tilePos != end)
        {
            if (currentTile.battleTile != ignoredTile)
            {
                if (currentTile.battleTile is null) return false;
                if (currentTile.isBlocked && (obstacleType == ObstacleType.All || obstacleType == ObstacleType.Units 
                    || obstacleType == ObstacleType.UnitsIncluded)) return false;
                if (currentTile.battleTile.IsHole && (obstacleType == ObstacleType.All || obstacleType == ObstacleType.Holes)) return false;
            }

            currentCoord = currentTile.tilePos + dir;

            if (currentCoord.x < 0 || currentCoord.x >= pathCalculatorTiles.GetLength(0) ||
                currentCoord.y < 0 || currentCoord.y >= pathCalculatorTiles.GetLength(1)) return false;

            currentTile = pathCalculatorTiles[currentCoord.x, currentCoord.y];
        }

        if (includeEnd)
        {
            if (currentTile.battleTile != ignoredTile)
            {
                if (currentTile.battleTile is null) return false;
                if (currentTile.isBlocked && (obstacleType == ObstacleType.All || obstacleType == ObstacleType.Units
                    || obstacleType == ObstacleType.UnitsIncluded)) return false;
                if (currentTile.battleTile.IsHole && (obstacleType == ObstacleType.All || obstacleType == ObstacleType.Holes)) return false;
            }
        }

        return true;
    }
}
