using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    [Serializable]
    public struct UpgradeCost
    {
        public LootData[] neededMaterials;
        public int[] neededMatQuantities;
    }

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



    private void Start()
    {
        InventoriesManager.Instance.OnInventoryChange += ActualiseResourcesFeedbacks;
    }

    public void DisplayLevel(int currentLevel)
    {
        if(currentLevel >= neededUpgradeCosts.Length)
        {
            hasUpgradeAvailable = false;

            return;
        }

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
