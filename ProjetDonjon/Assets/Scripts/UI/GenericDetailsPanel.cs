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
    [SerializeField] private Vector3 offsetAlteration;
    [SerializeField] private Vector3 offsetRelic;
    [SerializeField] private Vector3 offsetIntent;
    [SerializeField] private GenericDetailsData[] statDatas;

    [Header("Private Infos")]
    private Coroutine currentAppearCoroutine;
    private Vector3 statsOffsetSave;
    private Coroutine currentDescriptionAppearCoroutine;

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

    [Header("Appear Effect References")]
    [SerializeField] private Image[] _imagesToFade;
    [SerializeField] private TextMeshProUGUI[] _textsToFade;
    [SerializeField] private RectTransform _statRectTr;
    [SerializeField] private Image[] _imagesToFadeStats;
    [SerializeField] private TextMeshProUGUI[] _textsToFadeStats;


    private void Start()
    {
        statsOffsetSave = _statRectTr.localPosition;
        HideDetails();
    }

    private void SetPosition(Vector3 mainPosition, Vector3 offset)
    {
        Vector3 finalPosition = CameraManager.Instance.Camera.WorldToViewportPoint(mainPosition);
        finalPosition += offset;

        _mainRectTr.localPosition = Vector3.zero;
        transform.position = CameraManager.Instance.Camera.ViewportToWorldPoint(finalPosition);
    }


    #region All Load Functions

    public void LoadDetails(LootData lootData, Vector3 position, bool mirrorHorizontal, bool mirrorVertical, bool noAdditionalDetails = false)
    {
        _mainRectTr.gameObject.SetActive(true);

        if(position != Vector3.zero) 
            SetPosition(position, mirrorHorizontal ? offsetInventory - new Vector3(0.04f, 0, 0) : -offsetInventory - new Vector3(0.04f, 0, 0));

        _nameText.text = lootData.lootName;
        _descriptionText.text = lootData.lootDescription;
        _numberText.text = "";

        if (lootData.lootType != LootType.Equipment)
        {
            _statGlobalParent.gameObject.SetActive(false);
        }
        else    // Equipment side stats panel
        {
            _statGlobalParent.gameObject.SetActive(true);
            bool hasStatChange = false;

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

                hasStatChange = true;
            }
            if (lootData.strengthUpgrade != 0)
            {
                _statsParents[1].gameObject.SetActive(true);
                _statsTexts[1].text = lootData.strengthUpgrade.ToString();

                hasStatChange = true;
            }
            if (lootData.speedUpgrade != 0)
            {
                _statsParents[2].gameObject.SetActive(true);
                _statsTexts[2].text = lootData.speedUpgrade.ToString();

                hasStatChange = true;
            }
            if (lootData.luckUpgrade != 0)
            {
                _statsParents[3].gameObject.SetActive(true);
                _statsTexts[3].text = lootData.luckUpgrade.ToString();

                hasStatChange = true;
            }

            if(!hasStatChange)
            {
                _statGlobalParent.gameObject.SetActive(false);
            }
        }

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text, lootData.lootDescription));

        if (noAdditionalDetails) return;

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            if (i < lootData.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(lootData.additionalTooltipDatas[i], 0.15f + i * 0.15f, mirrorVertical);

            else
                _additionalTooltips[i].Hide();
        }
    }

    public void LoadDetails(SkillTreeNodeData nodeData, Vector3 position, bool mirrorHorizontal, bool mirrorVertical)
    {
        _mainRectTr.gameObject.SetActive(true);

        SetPosition(position, mirrorHorizontal ? offsetSkillTree : -offsetSkillTree);

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
                    _additionalTooltips[i].Show(nodeData.skillData.additionalTooltipDatas[i], 0.15f + i * 0.15f, mirrorVertical);

                else
                    _additionalTooltips[i].Hide();
            }
        }
        else if(nodeData.passiveData is not null)
        {
            _nameText.text = nodeData.passiveData.passiveName;
            _descriptionText.text = nodeData.passiveData.passiveDescription;
            _numberText.text = "";
            _treeNodeTypeText.text = "PASSIVE";

            for (int i = 0; i < _additionalTooltips.Length; i++)
            {
                if (i < nodeData.passiveData.additionalTooltipDatas.Length)
                    _additionalTooltips[i].Show(nodeData.passiveData.additionalTooltipDatas[i], 0.15f + i * 0.15f, mirrorVertical);

                else
                    _additionalTooltips[i].Hide();
            }
        }
        else
        {
            GenericDetailsData data = statDatas[((int)nodeData.statUpgradeType) - 1];

            _descriptionText.text = "";
            for (int i = 0; i < data.description.Length; i++)
            {
                if (data.description[i] == '?')
                {
                    _descriptionText.text += nodeData.upgradeStrength;
                }
                else
                {
                    _descriptionText.text += data.description[i];
                }
            }

            _nameText.text = data.name;
            _numberText.text = "";
            _treeNodeTypeText.text = "STAT";
        }

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text,_descriptionText.text));
    }

    public void LoadDetails(SkillData skillData, Vector3 position, bool noOffset)
    {
        _mainRectTr.gameObject.SetActive(true);
        SetPosition(position, noOffset ? Vector3.zero : offset);

        _nameText.text = skillData.skillName;
        _descriptionText.text = skillData.skillDescription;
        _numberText.text = "";

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            if (i < skillData.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(skillData.additionalTooltipDatas[i], 0.15f + i * 0.15f, true);

            else
                _additionalTooltips[i].Hide();
        }

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text,_descriptionText.text));
    }

    public void LoadDetails(PassiveData passiveData, Vector3 position)
    {
        _mainRectTr.gameObject.SetActive(true);
        SetPosition(position, offset);

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

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text, _descriptionText.text));
    }

    public void LoadDetails(AlterationStruct alterationData, Vector3 position, Unit attachedUnit)
    {
        _mainRectTr.gameObject.SetActive(true);
        SetPosition(position, offsetAlteration);

        _nameText.text = alterationData.alteration.alterationName;
        _descriptionText.text = alterationData.alteration.alterationDescription;

        if (!alterationData.alteration.isInfinite)
        {
            _numberText.text = alterationData.duration.ToString();
        }
        else
        {
            _numberText.text = "";
            switch (alterationData.alteration.alterationType)
            {
                case AlterationType.Shield:
                    _numberText.text = attachedUnit.CurrentShield.ToString();
                    break;
            }
        }

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text, _descriptionText.text));
    }

    public void LoadDetails(RelicData relicData, Vector3 position)
    {
        _mainRectTr.gameObject.SetActive(true);
        SetPosition(position, offsetRelic);

        _numberText.text = "";

        _nameText.text = "?????";
        _descriptionText.text = relicData.relicHint;

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text, _descriptionText.text));
    }

    public void LoadDetails(GenericDetailsData data, Vector3 position, bool mirrorHorizontal)
    {
        _mainRectTr.gameObject.SetActive(true);
        SetPosition(position, mirrorHorizontal ? -offset : offset);

        _numberText.text = "";

        _nameText.text = data.name;
        _descriptionText.text = data.description;

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text, _descriptionText.text));
    }

    public void LoadDetails(GenericDetailsData data, Vector3 position)
    {
        _mainRectTr.gameObject.SetActive(true);
        SetPosition(position, offsetIntent);

        _numberText.text = "";

        _nameText.text = data.name;
        _descriptionText.text = data.description;

        currentAppearCoroutine = StartCoroutine(AppearEffectCoroutine(_nameText.text, _descriptionText.text));
    }

    #endregion


    #region Appear / Disappear

    public void HideDetails()
    {
        if (currentAppearCoroutine != null) StopCoroutine(currentAppearCoroutine);

        _mainRectTr.DOComplete();
        _mainRectTr.gameObject.SetActive(false);

        _statGlobalParent.gameObject.SetActive(false);
        _treeNodeTypeText.enabled = false;
        _skillCostText.enabled = false;

        for(int i = 0; i < _skillCostImages.Length; i++)
        {
            _skillCostImages[i].enabled = false;    
        }

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            _additionalTooltips[i].Hide();
        }
    }

    private IEnumerator AppearEffectCoroutine(string title, string description)
    {
        if(currentDescriptionAppearCoroutine != null)
        {
            StopCoroutine(currentDescriptionAppearCoroutine);
        }

        StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(title, _nameText, 0.25f));
        currentDescriptionAppearCoroutine = StartCoroutine(UIManager.Instance.DisplayTextProgressivelyCoroutine(description, _descriptionText, 0.35f));

        float aimedSize = 0.75f;
        _mainRectTr.localScale = Vector3.one * aimedSize;
        //_mainRectTr.DOScale(Vector3.one * aimedSize * 1.1f, 0.12f).SetEase(Ease.OutCubic);

        _mainRectTr.localPosition = new Vector3(0, -80f, 0);
        _mainRectTr.DOLocalMove(new Vector3(0, 0f, 0), 0.25f).SetEase(Ease.OutBack);

        for (int i = 0; i < _imagesToFade.Length; i++)
        {
            _imagesToFade[i].DOKill();
            _imagesToFade[i].color = _imagesToFade[i].color * new Color(1, 1, 1, 0);
            _imagesToFade[i].DOFade(1, 0.35f).SetEase(Ease.OutSine);
        }
        for (int i = 0; i < _textsToFade.Length; i++)
        {
            _textsToFade[i].DOKill();
            _textsToFade[i].color = _textsToFade[i].color * new Color(1, 1, 1, 0);
            _textsToFade[i].DOFade(1, 0.35f).SetEase(Ease.OutSine);
        }

        for (int i = 0; i < _imagesToFadeStats.Length; i++)
        {
            _imagesToFadeStats[i].DOKill();
            _imagesToFadeStats[i].color = _imagesToFadeStats[i].color * new Color(1, 1, 1, 0);
        }
        for (int i = 0; i < _textsToFadeStats.Length; i++)
        {
            _textsToFadeStats[i].DOKill();
            _textsToFadeStats[i].color = _textsToFadeStats[i].color * new Color(1, 1, 1, 0);
        }

        yield return new WaitForSeconds(0.1f);

        _statRectTr.localPosition = statsOffsetSave + new Vector3(30, 0, 0);
        _statRectTr.DOLocalMove(statsOffsetSave, 0.25f).SetEase(Ease.OutBack);

        for (int i = 0; i < _imagesToFadeStats.Length; i++)
        {
            _imagesToFadeStats[i].DOFade(1, 0.35f).SetEase(Ease.OutSine);
        }
        for (int i = 0; i < _textsToFadeStats.Length; i++)
        {
            _textsToFadeStats[i].DOFade(1, 0.35f).SetEase(Ease.OutSine);
        }
    }

    private IEnumerator DisappearEffectCoroutine()
    {
        yield return null;
    }

    #endregion
}

