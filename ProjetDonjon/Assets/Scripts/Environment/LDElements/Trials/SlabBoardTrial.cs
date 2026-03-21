using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SlabBoardTrial : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector2Int boardSize;
    [SerializeField] private int minPathLength;
    [SerializeField] private int maxPathLength;

    [Header("Private Infos")]
    private List<TrapSlab> correctPath;
    private TrapSlab[,] board;

    [Header("Referencess")]
    [SerializeField] private GameObject _slabsParent;


    private void Start()
    {
        GenerateBoard();
        GeneratePath();
    }

    private void GenerateBoard()
    {
        TrapSlab[] allSlabs = _slabsParent.GetComponentsInChildren<TrapSlab>();
        board = new TrapSlab[boardSize.x, boardSize.y];


        TrapSlab bottomLeft = allSlabs[0], upRight = allSlabs[0];
        for(int i = 0; i < allSlabs.Length; i++)
        {
            if(allSlabs[i].transform.position.x < bottomLeft.transform.position.x || allSlabs[i].transform.position.y < bottomLeft.transform.position.y)
            {
                bottomLeft = allSlabs[i];
            }
            else if (allSlabs[i].transform.position.x > upRight.transform.position.x || allSlabs[i].transform.position.y > upRight.transform.position.y)
            {
                upRight = allSlabs[i];   
            }
        }


        float distBetweenTilesX = (upRight.transform.position.x - bottomLeft.transform.position.x) / (boardSize.x - 1);
        float distBetweenTilesY = (upRight.transform.position.y - bottomLeft.transform.position.y) / (boardSize.y - 1);

        for (int i = 0; i < allSlabs.Length; i++)
        {
            int coordX = Mathf.RoundToInt((float)(allSlabs[i].transform.position.x - bottomLeft.transform.position.x) / distBetweenTilesX);
            int coordY = Mathf.RoundToInt((float)(allSlabs[i].transform.position.y - bottomLeft.transform.position.y) / distBetweenTilesY);

            board[coordX, coordY] = allSlabs[i];
        }
    }

    private void GeneratePath()
    {
        correctPath = new List<TrapSlab>();
        Vector2Int start = new Vector2Int(-1, Random.Range(1, boardSize.y - 1));
        Vector2Int current = start;
        int length = 0;

        bool goRight = false;

        while(current.x != boardSize.x - 1)
        {
            if (!goRight)
            {
                int addedLength = Random.Range(2, 4);
                goRight = true;

                for(int i = 0; i < addedLength; i++)
                {
                    current = current + Vector2Int.right;
                    correctPath.Add(board[current.x, current.y]);
                    length++;

                    if (current.x == boardSize.x - 1) break;
                }
            }
            else
            {
                int addedLength = Random.Range(1, 4);
                goRight = false;
                bool goUp = Random.Range(0, 2) == 0;
                if(current.y - addedLength < 0)
                {
                    goUp = true;
                } 
                else if(current.y + addedLength >= boardSize.y)
                {
                    goUp= false;
                }

                for (int i = 0; i < addedLength; i++)
                {
                    if (goUp) current += Vector2Int.up;
                    else current += Vector2Int.down;

                    correctPath.Add(board[current.x, current.y]);
                    length++;
                }
            }
        }

        foreach (TrapSlab slab in correctPath)
        {
            slab.Deactivate();
        }
    }


    private void DisplayCorrectPath()
    {
        foreach (TrapSlab slab in correctPath)
        {
            slab.Highlight();
        }
    }

    private void HideCorrectPath()
    {
        foreach (TrapSlab slab in correctPath)
        {
            slab.StopHighlight();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DisplayCorrectPath();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HideCorrectPath();
    }
}
