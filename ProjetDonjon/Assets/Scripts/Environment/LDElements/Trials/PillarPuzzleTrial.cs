using UnityEngine;

public class PillarPuzzleTrial : Trial
{
    [Header("Parameters")]
    [SerializeField] private int minTurnedOnPillars;
    [SerializeField] private int maxTurnedOnPillars;

    [Header("References")]
    [SerializeField] private Pillar[] _puzzlePillars;
    [SerializeField] private Chest _trialChest;

    private void Start()
    {
        InitPuzzle();
    }

    public void InitPuzzle()
    {
        bool[] turnedOnPillars = new bool[_puzzlePillars.Length];
        int turnedOnCount = Random.Range(minTurnedOnPillars, maxTurnedOnPillars + 1);

        for(int i = 0; i < turnedOnCount; i++)
        {
            turnedOnPillars[Random.Range(0, turnedOnPillars.Length)] = true;
        }

        for(int i = 0; i < _puzzlePillars.Length; i++)
        {
            if(i == 0)
            {
                _puzzlePillars[i].Initialise(turnedOnPillars[i], new Pillar[1] { _puzzlePillars[i + 1] });
            }
            else if (i == _puzzlePillars.Length - 1)
            {
                _puzzlePillars[i].Initialise(turnedOnPillars[i], new Pillar[1] { _puzzlePillars[i - 1] });
            }
            else
            {
                _puzzlePillars[i].Initialise(turnedOnPillars[i], new Pillar[2] { _puzzlePillars[i - 1], _puzzlePillars[i + 1] });
            }

            _puzzlePillars[i].OnChangeState += VerifySuccess;
        }
    }

    private void VerifySuccess()
    {
        bool success = true;
        for(int i = 0; i < _puzzlePillars.Length; i++)
        {
            if (_puzzlePillars[i].IsOn)
            {
                continue;
            }

            success = false;
            break;
        }

        if(success)
        {
            SuccessPuzzle();
        }
    }

    private void SuccessPuzzle()
    {
        for (int i = 0; i < _puzzlePillars.Length; i++)
        {
            _puzzlePillars[i].DeactivatePuzzle();
        }

        _trialChest.Interact();
    }
}
