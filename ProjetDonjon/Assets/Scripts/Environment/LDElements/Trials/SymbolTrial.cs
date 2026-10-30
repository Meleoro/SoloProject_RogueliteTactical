using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;


public class SymbolTrial : MonoBehaviour
{
    private struct SymbolInfos
    {
        public SymbolInfos(int categoryIndex, int symbolIndex)
        {
            this.categoryIndex = categoryIndex;
            this.symbolIndex = symbolIndex;
            isSelectedByPlayer = false;
        }

        public int categoryIndex;
        public int symbolIndex;
        public bool isSelectedByPlayer;
    }

    [Header("Parameters")]
    [SerializeField] private int resultSymbolCount;
    [SerializeField] private int categoryCount;
    [SerializeField] private int symbolsPerCategory;
    [SerializeField] private ParticleSystem dustVFXPrefab;
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private int arrowDamages;

    [Header("Private Infos")]
    private SymbolInfos[] resultInfos;
    private ParticleSystem[] dustParticleSystems;

    [Header("References")]
    [SerializeField] private SymbolPillar[] _symbolPillars;
    [SerializeField] private SymbolPillarResult _resultPillar;
    [SerializeField] private Chest _trialChest;
    [SerializeField] private Transform[] _arrowDispensers;


    private void Start()
    {
        SetupPillars();
        GenerateResult();

        dustParticleSystems = new ParticleSystem[_arrowDispensers.Length];

        for (int i = 0; i < _arrowDispensers.Length; i++)
        {
            dustParticleSystems[i] =
                Instantiate(dustVFXPrefab, _arrowDispensers[i].transform.position, _arrowDispensers[i].transform.rotation, transform);
        }
    }

    private void SetupPillars()
    {
        for(int categoryIndex = 0; categoryIndex < categoryCount; categoryIndex++)
        {
            for (int symbolIndex = 0; symbolIndex < symbolsPerCategory; symbolIndex++)
            {
                _symbolPillars[categoryIndex * symbolsPerCategory + symbolIndex].SetupPillar(categoryIndex, symbolIndex);
                _symbolPillars[categoryIndex * symbolsPerCategory + symbolIndex].OnInteract += InteractWithPillar;
            }
        }
    }

    private void GenerateResult()
    {
        List<int> bannedCategories = new List<int>();
        resultInfos = new SymbolInfos[resultSymbolCount];

        for (int i = 0; i < resultSymbolCount; i++)
        {
            bool found = false;

            while(!found)
            {
                int pickedCategory = Random.Range(0, 3);

                if(bannedCategories.Contains(pickedCategory))
                {
                    continue;
                }

                int pickedSymbol = Random.Range(0, symbolsPerCategory);

                bannedCategories.Add(pickedCategory);
                resultInfos[i] = new SymbolInfos(pickedCategory, pickedSymbol);
                _resultPillar.AddSymbol(pickedCategory, pickedSymbol,
                    _symbolPillars[pickedCategory * symbolsPerCategory + pickedSymbol].SymbolSpriteRenderer.sprite);

                found = true;
            }
        }
    }


    private void InteractWithPillar(int pillarCategoryIndex, int pillarSymbolIndex)
    {
        bool isValid = false;

        for(int i = 0; i < resultSymbolCount; i++)
        {
            if (resultInfos[i].categoryIndex != pillarCategoryIndex || resultInfos[i].symbolIndex != pillarSymbolIndex)
            {
                continue;
            }

            _resultPillar.ValidateSymbol(resultInfos[i].categoryIndex, resultInfos[i].symbolIndex);

            resultInfos[i].isSelectedByPlayer = true;
            isValid = true;
        }

        if(isValid)
        {
            VerifyResult();
            _symbolPillars[pillarCategoryIndex * symbolsPerCategory + pillarSymbolIndex].ActualisePillarVisuals(true);
        }
        else
        {
            TriggerTrap();
            _symbolPillars[pillarCategoryIndex * symbolsPerCategory + pillarSymbolIndex].ActualisePillarVisuals(false);
        }
    }

    private void VerifyResult()
    {
        bool isFinished = true;

        for (int i = 0; i < resultSymbolCount; i++)
        {
            if (resultInfos[i].isSelectedByPlayer)
            {
                continue;
            }

            isFinished = false;
            break;
        }

        if (isFinished)
        {
            foreach(SymbolPillar pillar in _symbolPillars)
            {
                pillar.DisablePillar();
            }

            _trialChest.Interact();
        }
    }

    private void TriggerTrap()
    {
        for (int i = 0; i < _arrowDispensers.Length; i++)
        {
            StartCoroutine(LaunchArrowCoroutine(Random.Range(0.0f, 0.1f), _arrowDispensers[i], dustParticleSystems[i]));
        }
    }

    private IEnumerator LaunchArrowCoroutine(float delay, Transform dispenserTr, ParticleSystem dustVFX)
    {
        yield return new WaitForSeconds(delay);

        Arrow newArrow = Instantiate(arrowPrefab, dispenserTr.position, dispenserTr.rotation);
        newArrow.InitialiseArrow(arrowSpeed, arrowDamages);

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 1, 7, 0);

        dustVFX.Play();
    }
}
