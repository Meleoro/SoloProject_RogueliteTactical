using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class GenericDetailsPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 offsetSkillTree;
    [SerializeField] private Vector3 offsetInventory;

    [Header("Private Infos")]
    private Coroutine appearCoroutine;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private Image[] _skillCostImages;
    [SerializeField] private TextMeshProUGUI _skillCostText;
    [SerializeField] private TextMeshProUGUI _treeNodeTypeText;
    [SerializeField] private AdditionalTooltip[] _additionalTooltips;
    [SerializeField] private RectTransform[] _statsParents;
    [SerializeField] private TextMeshProUGUI[] _statsTexts;
    [SerializeField] private RectTransform _statGlobalParent;


    private void Start()
    {
        HideDetails();
    }

    public void LoadDetails(LootData lootData, Vector3 position, bool mirrorHorizontal)
    {
        _mainRectTr.gameObject.SetActive(true);

        if (mirrorHorizontal) _mainRectTr.position = position + offsetInventory;
        else _mainRectTr.position = position - offsetInventory;

        _nameText.text = lootData.lootName;
        _descriptionText.text = lootData.lootDescription;

        if(lootData.lootType != LootType.Equipment)
        {
            _statGlobalParent.gameObject.SetActive(false);
        }
        else
        {
            _statGlobalParent.gameObject.SetActive(true);

            if (mirrorHorizontal) _mainRectTr.position = _mainRectTr.position + Vector3.left * 0.75f;
            else _mainRectTr.position = _mainRectTr.position;

            for (int i = 0; i < _statsParents.Length; i++)
            {
                _statsParents[i].gameObject.SetActive(false);
            }

            if (lootData.healthUpgrade != 0)
            {
                _statsParents[0].gameObject.SetActive(true);
                _statsTexts[0].text = lootData.healthUpgrade.ToString();
            }
            if (lootData.strengthUpgrade != 0)
            {
                _statsParents[1].gameObject.SetActive(true);
                _statsTexts[1].text = lootData.strengthUpgrade.ToString();
            }
            if (lootData.speedUpgrade != 0)
            {
                _statsParents[2].gameObject.SetActive(true);
                _statsTexts[2].text = lootData.speedUpgrade.ToString();
            }
            if (lootData.luckUpgrade != 0)
            {
                _statsParents[3].gameObject.SetActive(true);
                _statsTexts[3].text = lootData.luckUpgrade.ToString();
            }
        }

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            if (i < lootData.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(lootData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

            else
                _additionalTooltips[i].Hide();
        }

        appearCoroutine = StartCoroutine(AppearEffectCoroutine());
    }

    public void LoadDetails(SkillTreeNodeData nodeData, Vector3 position, bool mirrorHorizontal)
    {
        _mainRectTr.gameObject.SetActive(true);

        if(mirrorHorizontal) _mainRectTr.position = position + offsetSkillTree;
        else _mainRectTr.position = position - offsetSkillTree;

        _treeNodeTypeText.enabled = true;

        if (nodeData.skillData is not null)
        {
            _nameText.text = nodeData.skillData.skillName;
            _descriptionText.text = nodeData.skillData.skillDescription;
            
            _numberText.text = "";
            _treeNodeTypeText.text = "SKILL";

            _skillCostText.enabled = true;
            for(int i = 0; i < nodeData.skillData.skillPointCost; i++)
            {
                _skillCostImages[i].enabled = true;
            }

            for (int i = 0; i < _additionalTooltips.Length; i++)
            {
                if (i < nodeData.skillData.additionalTooltipDatas.Length)
                    _additionalTooltips[i].Show(nodeData.skillData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

                else
                    _additionalTooltips[i].Hide();
            }
        }
        else
        {
            _nameText.text = nodeData.passiveData.passiveName;
            _descriptionText.text = nodeData.passiveData.passiveDescription;
            _numberText.text = "";
            _treeNodeTypeText.text = "PASSIVE";

            for (int i = 0; i < _additionalTooltips.Length; i++)
            {
                if (i < nodeData.passiveData.additionalTooltipDatas.Length)
                    _additionalTooltips[i].Show(nodeData.passiveData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

                else
                    _additionalTooltips[i].Hide();
            }
        }

        appearCoroutine = StartCoroutine(AppearEffectCoroutine());
    }

    public void LoadDetails(SkillData skillData, Vector3 position)
    {
        _mainRectTr.gameObject.SetActive(true);
        _mainRectTr.position = position + offset;

        _nameText.text = skillData.skillName;
        _descriptionText.text = skillData.skillDescription;
        _numberText.text = "";

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            if (i < skillData.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(skillData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

            else
                _additionalTooltips[i].Hide();
        }

        appearCoroutine = StartCoroutine(AppearEffectCoroutine());
    }

    public void LoadDetails(PassiveData passiveData, Vector3 position)
    {
        _mainRectTr.gameObject.SetActive(true);
        _mainRectTr.position = position + offset;

        _nameText.text = passiveData.passiveName;
        _descriptionText.text = passiveData.passiveDescription;
        _numberText.text = "";

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            if (i < passiveData.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(passiveData.additionalTooltipDatas[i], 0.15f + i * 0.15f);

            else
                _additionalTooltips[i].Hide();
        }

        appearCoroutine = StartCoroutine(AppearEffectCoroutine());
    }

    public void LoadDetails(AlterationData alterationData, Vector3 position)
    {
        _mainRectTr.gameObject.SetActive(true);
        _mainRectTr.position = position + offset;

        _nameText.text = alterationData.alterationName;
        _descriptionText.text = alterationData.alterationDescription;

        appearCoroutine = StartCoroutine(AppearEffectCoroutine());
    }

    public void HideDetails()
    {
        if (appearCoroutine != null) StopCoroutine(appearCoroutine);

        _mainRectTr.DOComplete();
        _mainRectTr.gameObject.SetActive(false);

        _statGlobalParent.gameObject.SetActive(false);
        _treeNodeTypeText.enabled = false;
        _skillCostText.enabled = false;
        for(int i = 0; i < _skillCostImages.Length; i++)
        {
            _skillCostImages[i].enabled = false;    
        }
    }


    private IEnumerator AppearEffectCoroutine()
    {
        _mainRectTr.localScale = Vector3.zero;

        float aimedSize = 0.75f;
        _mainRectTr.DOScale(Vector3.one * aimedSize * 1.1f, 0.12f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.12f);

        _mainRectTr.DOScale(new Vector3(aimedSize, aimedSize, aimedSize), 0.15f).SetEase(Ease.OutCubic);
    }

    private IEnumerator DisappearEffectCoroutine()
    {
        yield return null;
    }
}

