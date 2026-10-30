using System.Collections;
using UnityEngine;

public class MemoryPillarsTrial : Trial
{
    [Header("Parameters")]
    [SerializeField] private int sequenceLength;
    [SerializeField] private float flashDuration;

    [Header("Private Infos")]
    private int[] currentResultSequence;
    private int currentRespongIndex;

    [Header("References")]
    [SerializeField] private MemoryPillar[] _memoryPillars;



    private void Start()
    {
        for (int i = 0; i < _memoryPillars.Length; i++)
        {
            _memoryPillars[i].SetupPillar(i);
            _memoryPillars[i].OnInteraction += InteractWithPillar;
        }
    }


    public override void StartTrial()
    {
        StartDemo();
    }

    public void StartDemo()
    {
        if(currentResultSequence == null)
        {
            GenerateSequence();
        }

        currentRespongIndex = 0;
        StartCoroutine(StartDemoCoroutine());
    }

    private void GenerateSequence()
    {
        currentResultSequence = new int[sequenceLength];

        for (int i = 0; i < sequenceLength; i++)
        {
            currentResultSequence[i] = Random.Range(0, _memoryPillars.Length);
        }
    }

    private IEnumerator StartDemoCoroutine()
    {
        foreach(MemoryPillar pillar in _memoryPillars)
        {
            pillar.DisableInteractions();
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < sequenceLength; i++)
        {
            _memoryPillars[currentResultSequence[i]].DoFlashPillar(flashDuration, true);

            yield return new WaitForSeconds(flashDuration * 1.1f);
        }

        foreach (MemoryPillar pillar in _memoryPillars)
        {
            pillar.EnableInteractions();
        }
    }


    private void InteractWithPillar(int index)
    {
        if(index == currentResultSequence[currentRespongIndex])   // Good response
        {
            _memoryPillars[index].DoFlashPillar(flashDuration * 0.5f, true);

            currentRespongIndex++;
            if(currentRespongIndex == sequenceLength)
            {
                OnTrialEnd.Invoke();

                foreach (MemoryPillar pillar in _memoryPillars)
                {
                    pillar.DisableInteractions();
                }
            }
        }
        else    // Bad Response 
        {
            StartCoroutine(WrongResponseFeedbackCoroutine());
        }
    }

    private IEnumerator WrongResponseFeedbackCoroutine()
    {
        foreach (MemoryPillar pillar in _memoryPillars)
        {
            pillar.DisableInteractions();
            pillar.DoFlashPillar(flashDuration, false);
        }

        yield return new WaitForSeconds(flashDuration * 1.1f);

        foreach(MemoryPillar pillar in _memoryPillars)
        {
            pillar.DoFlashPillar(flashDuration, false);
        }

        yield return new WaitForSeconds(flashDuration * 1.5f);

        StartDemo();
    }
}
