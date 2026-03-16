using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct UpgradeCost
{
    public LootData[] neededMaterials;
    public int[] neededMatQuantities;
}

public class UpgradePanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private UpgradeCost[] neededUpgradeCosts;
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    [Header("Actions")]
    public Action OnUpgrade;

    [Header("Private Infos")]
    private UpgradeCost currentUpgradeData;
    private bool hasEnoughResources;
    private bool hasUpgradeAvailable;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI[] _matTexts;
    [SerializeField] private Image[] _matIcons;
    [SerializeField] private Image _upgradeButton;
    [SerializeField] private TextMeshProUGUI _buttonText;



    private void Start()
    {
        InventoriesManager.Instance.OnInventoryChange += ActualiseResourcesFeedbacks;
    }

    public void SetUpgradeCost(UpgradeCost upgrade)
    {
        currentUpgradeData = upgrade;

        if(currentUpgradeData.neededMaterials.Length > 0)    // If the upgrade is valid
        {
            neededUpgradeCosts = new UpgradeCost[1];
            neededUpgradeCosts[0] = upgrade;
        }

        else      // If no upgrade available
        { 
            neededUpgradeCosts = new UpgradeCost[0];
        }

        DisplayLevel(0);
    }


    public void DisplayLevel(int currentLevel)
    {
        // If no upgrade available
        if(currentLevel >= neededUpgradeCosts.Length)
        {
            hasUpgradeAvailable = false;
            _buttonText.text = "LEVEL MAX";

            for (int i = 0; i < _matTexts.Length; i++)
            {
                _matTexts[i].text = " ";
                _matIcons[i].enabled = false;
            }

            return;
        }

        // If upgrade available
        _buttonText.text = "UPGRADE";

        hasUpgradeAvailable = true;
        currentUpgradeData = neededUpgradeCosts[currentLevel];

        for(int i = 0; i < currentUpgradeData.neededMaterials.Length; i++)
        {
            _matTexts[i].text = currentUpgradeData.neededMatQuantities[i] + " " + currentUpgradeData.neededMaterials[i].lootName;
            _matIcons[i].enabled = true;
            _matIcons[i].sprite = currentUpgradeData.neededMaterials[i].sprite;
        }

        for (int i = currentUpgradeData.neededMaterials.Length; i < _matIcons.Length; i++)
        {
            _matTexts[i].text = "";
            _matIcons[i].enabled = false;
        }

        ActualiseResourcesFeedbacks();
    }

    public void ActualiseResourcesFeedbacks()
    {
        if (!hasUpgradeAvailable) return;

        hasEnoughResources = true;

        for (int i = 0; i < currentUpgradeData.neededMaterials.Length; i++)
        {
            if(InventoriesManager.Instance.GetItemCount(currentUpgradeData.neededMaterials[i]) < currentUpgradeData.neededMatQuantities[i])
            {
                hasEnoughResources = false;
                _matTexts[i].color = invalidColor;

                continue;
            }

            _matTexts[i].color = validColor;
        }
    }


    #region Mouse Inputs

    public void HoverUpgrade()
    {
        _upgradeButton.rectTransform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutCubic);
    }

    public void UnhoverUpgrade()
    {
        _upgradeButton.rectTransform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutCubic);
    }

    public void ClickUpgrade()
    {
        if (!hasEnoughResources) return;

        for(int i = 0; i < currentUpgradeData.neededMaterials.Length; i++)
        {
            for(int j = 0; j < currentUpgradeData.neededMatQuantities[i]; j++)
            {
                InventoriesManager.Instance.DestroyItem(currentUpgradeData.neededMaterials[i]);
            }
        }

        OnUpgrade?.Invoke();
    }

    #endregion
}
