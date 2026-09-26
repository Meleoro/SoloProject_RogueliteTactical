using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class SkillsMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color upButtonSelectedColor;
    [SerializeField] private Color upButtonNotSelectedColor;
    [SerializeField] private Color upButtonSelectedColorText;
    [SerializeField] private Color upButtonNotSelectedColorText;

    [Header("Actions")]
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private Hero currentHero;
    private int currentHeroIndex;   
    private bool isInTransition;
    private bool isOnSkills;
    private EquippableSkill draggedInfos;

    [Header("References Left")]
    [SerializeField] private RectTransform _leftPartParent;
    [SerializeField] private RectTransform _leftShownPosTr;
    [SerializeField] private EquippableSkill[] _equippableSkillButtons;
    [SerializeField] private RectTransform[] _upButtonsRectTr;
    [SerializeField] private TextMeshProUGUI[] _upButtonsTexts;
    [SerializeField] private Image[] _upButtonsImages;
    [SerializeField] private Image[] _upButtonsOutlines;
    [SerializeField] private RectTransform _buttonsParent;

    [Header("References Right")]
    [SerializeField] private RectTransform _rightPartParent;
    [SerializeField] private RectTransform _rightShownPosTr;
    [SerializeField] private EquippedSkill[] _equippedSkillButtons;
    [SerializeField] private TextMeshProUGUI _heroName;

    [Header("References Others")]
    [SerializeField] private RectTransform[] _heroQuickChangePositions;
    [SerializeField] private Image _backFadeImage;

    private void Start()
    {
        foreach(EquippableSkill equippableSkill in _equippableSkillButtons)
        {
            equippableSkill.OnHover += HoverEquippableSkill;
            equippableSkill.OnUnhover += UnhoverEquippableSkill;
            equippableSkill.OnClick += ClickEquippableSkill;
        }

        foreach (EquippedSkill equippedSkill in _equippedSkillButtons)
        {

        }
    }


    #region Equippable Buttons

    private void HoverEquippableSkill(EquippableSkill equippableSkill)
    {
        if(equippableSkill.PassiveData is null) 
            UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(equippableSkill.SkillData, equippableSkill.transform.position, false);

        else
            UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(equippableSkill.PassiveData, equippableSkill.transform.position);
    }

    private void UnhoverEquippableSkill()
    {
        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();
    }

    private void ClickEquippableSkill(EquippableSkill equippableSkill)
    {
        if (equippableSkill.IsEquipped)
        {
            Unequip(equippableSkill);
            equippableSkill.Unequip(true);
        }
        else
        {
            if (TryEquip(equippableSkill)) equippableSkill.Equip(true);
        }

        ActualiseHeroInfos();
    }

    private bool TryEquip(EquippableSkill equippableSkill)
    {
        for (int i = 0; i < _equippedSkillButtons.Length; i++)
        {
            if (_equippedSkillButtons[i].IsLocked) continue;
            if (_equippedSkillButtons[i].IsPassiveSlot && equippableSkill.PassiveData is null) continue;
            if (!_equippedSkillButtons[i].IsPassiveSlot && equippableSkill.PassiveData is not null) continue;
            if (_equippedSkillButtons[i].SkillData is not null || _equippedSkillButtons[i].PassiveData is not null) continue;

            if(equippableSkill.SkillData is not null) _equippedSkillButtons[i].SetEquippedElement(equippableSkill.SkillData);
            else _equippedSkillButtons[i].SetEquippedElement(equippableSkill.PassiveData);

            return true;
        }

        return false;
    }

    private void Unequip(EquippableSkill equippableSkill)
    {
        for (int i = 0; i < _equippedSkillButtons.Length; i++)
        {
            if (_equippedSkillButtons[i].IsLocked) continue;
            if (_equippedSkillButtons[i].IsPassiveSlot && equippableSkill.PassiveData is null) continue;
            if (_equippedSkillButtons[i].SkillData != equippableSkill.SkillData && equippableSkill.SkillData is not null) continue;
            if (_equippedSkillButtons[i].PassiveData != equippableSkill.PassiveData && equippableSkill.PassiveData is not null) continue;

            if (equippableSkill.SkillData is not null) _equippedSkillButtons[i].SetEquippedElement(null as SkillData);
            else _equippedSkillButtons[i].SetEquippedElement(null as PassiveData);

            return;
        }
    }

    private void EquipSkill(SkillData skillData)
    {
        for(int i = 0; i < 12; i++)
        {
            if (_equippableSkillButtons[i].SkillData is null) continue;
            if (_equippableSkillButtons[i].SkillData != skillData) continue;

            _equippableSkillButtons[i].Equip();
        }
    }

    private void EquipPassive(PassiveData passiveData)
    {
        for (int i = 0; i < 5; i++)
        {
            if (_equippableSkillButtons[i].PassiveData is null) continue;
            if (_equippableSkillButtons[i].PassiveData != passiveData) continue;

            _equippableSkillButtons[i].Equip();
        }
    }

    private void ActualiseHeroInfos()
    {
        SkillData[] currentSkills = new SkillData[6];
        PassiveData[] currentPassives = new PassiveData[3];

        for (int i = 0; i < _equippedSkillButtons.Length; i++)
        {
            if (i < 6) 
            {
                if (_equippedSkillButtons[i].SkillData is null) continue;

                currentSkills[i] = _equippedSkillButtons[i].SkillData;
            }
            else
            {
                if (_equippedSkillButtons[i].PassiveData is null) continue;

                currentPassives[i - 6] = _equippedSkillButtons[i].PassiveData;
            }
        }

        currentHero.ActualiseEquippedSkills(currentSkills);
        currentHero.ActualiseEquippedPassives(currentPassives);
    }

    #endregion


    #region Change Hero

    public void SetupQuickChangeButtons()
    {
        InventoriesManager.Instance.OnQuickChange += ChangeHero;
        InventoriesManager.Instance.DisplayQuickChangeButtons(_heroQuickChangePositions);
    }

    public void ChangeHero(int heroIndex)
    {
        currentHeroIndex = heroIndex;
        Hero hero = HeroesManager.Instance.Heroes[currentHeroIndex];
        SetupHero(hero);
    }

    public IEnumerator ChangeHeroCoroutine(bool goLeft)
    {
        isInTransition = true;

        Vector3 pos1L = goLeft ? new Vector3(-1200, 0, 0) : new Vector3(800, 0, 0);
        Vector3 pos1R = goLeft ? new Vector3(-800, 0, 0) : new Vector3(1200, 0, 0);
        Quaternion rot1 = goLeft ? Quaternion.Euler(0, 0, 15) : Quaternion.Euler(0, 0, -15);
        _leftPartParent.UChangeLocalPosition(0.2f, pos1L, CurveType.EaseInCubic);
        _leftPartParent.UChangeLocalRotation(0.2f, rot1, CurveType.EaseInCubic);
        _rightPartParent.UChangeLocalPosition(0.2f, pos1R, CurveType.EaseInCubic);
        _rightPartParent.UChangeLocalRotation(0.2f, rot1, CurveType.EaseInCubic);

        yield return new WaitForSeconds(0.2f);

        currentHeroIndex += goLeft ? -1 : 1;
        if (currentHeroIndex < 0) currentHeroIndex += HeroesManager.Instance.Heroes.Length;
        currentHeroIndex = currentHeroIndex % HeroesManager.Instance.Heroes.Length;
        Hero hero = HeroesManager.Instance.Heroes[currentHeroIndex];
        SetupHero(hero);

        _leftPartParent.localPosition = -pos1R;
        _rightPartParent.localPosition = -pos1L;

        _leftPartParent.UChangeLocalPosition(0.2f, _leftPartParent.parent.InverseTransformPoint(_leftShownPosTr.position), 
            CurveType.EaseOutCubic);
        _leftPartParent.UChangeLocalRotation(0.2f, Quaternion.Euler(0, 0, 0), CurveType.EaseOutCubic);

        _rightPartParent.UChangeLocalPosition(0.2f, _rightPartParent.parent.InverseTransformPoint(_rightShownPosTr.position), 
            CurveType.EaseOutCubic);
        _rightPartParent.UChangeLocalRotation(0.2f, Quaternion.Euler(0, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.2f);

        isInTransition = false;
    }

    #endregion


    #region Up Buttons

    public void HoverUpButton(int buttonIndex)
    {
        _upButtonsImages[buttonIndex].rectTransform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);
        _upButtonsOutlines[buttonIndex].DOFade(1.0f, 0.2f).SetEase(Ease.OutBack);

        return;

        // Hovered + Selected
        if ((isOnSkills && buttonIndex == 0) || (!isOnSkills && buttonIndex == 1))
        {
            _upButtonsImages[buttonIndex].rectTransform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);
            _upButtonsOutlines[buttonIndex].DOFade(1.0f, 0.3f).SetEase(Ease.OutBack);
        }
        else     // Hovered + Unselected
        {
            _upButtonsImages[buttonIndex].rectTransform.DOScale(Vector3.one * 0.9f, 0.15f).SetEase(Ease.OutBack);
            _upButtonsOutlines[buttonIndex].DOFade(1.0f, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public void UnhoverUpButton(int buttonIndex)
    {
        _upButtonsImages[buttonIndex].rectTransform.DOScale(Vector3.one * 1.0f, 0.15f).SetEase(Ease.OutBack);
        _upButtonsOutlines[buttonIndex].DOFade(0.0f, 0.2f).SetEase(Ease.OutBack);

        return;

        // Hovered + Selected
        if ((isOnSkills && buttonIndex == 0) || (!isOnSkills && buttonIndex == 1))
        {
            _upButtonsRectTr[buttonIndex].UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseOutSin);
        }
        else     // Hovered + Unselected
        {
            _upButtonsRectTr[buttonIndex].UChangeScale(0.2f, Vector3.one * 0.8f, CurveType.EaseOutSin);
        }
    }

    public void ClickUpButton(int buttonIndex)
    {
        if (buttonIndex == 0) isOnSkills = true;
        else isOnSkills = false;

        ActualiseUpButtons();
        LoadEquippableButtons();
        LoadEquippedButtons();
    }

    private void ActualiseUpButtons()
    {
        if (isOnSkills)
        {
            //_upButtonsImages[0].rectTransform.UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseOutSin);
            //_upButtonsImages[1].rectTransform.UChangeScale(0.2f, Vector3.one * 0.8f, CurveType.EaseOutSin);

            _upButtonsTexts[0].DOColor(upButtonSelectedColorText, 0.2f).SetEase(Ease.OutBack);
            _upButtonsTexts[1].DOColor(upButtonNotSelectedColorText, 0.2f).SetEase(Ease.OutBack);
        }
        else
        {
            //_upButtonsImages[0].rectTransform.UChangeScale(0.2f, Vector3.one * 0.8f, CurveType.EaseOutSin);
            //_upButtonsImages[1].rectTransform.UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseOutSin);

            _upButtonsTexts[0].DOColor(upButtonNotSelectedColorText, 0.2f).SetEase(Ease.OutBack);
            _upButtonsTexts[1].DOColor(upButtonSelectedColorText, 0.2f).SetEase(Ease.OutBack);
        }
    }

    #endregion


    #region Load Infos

    public void SetupHero(Hero hero)
    {
        currentHero = hero;
        _heroName.text = hero.HeroData.unitName;

        LoadEquippableButtons();
        ActualiseUpButtons();
    }

    private void LoadEquippableButtons()
    {
        SkillTreeData skillTreeData = currentHero.SkillTreeData;
        SkillTreeNodeData[] validNodes = GetAllPossessedNodes(skillTreeData, currentHero.SkillTreeUnlockedNodes);
        int height = 100;

        if (isOnSkills)
        {
            List<SkillData> availableSkills = new List<SkillData>();

            for(int i = 0; i < currentHero.HeroData.heroBaseSkills.Length; i++)
            {
                availableSkills.Add(currentHero.HeroData.heroBaseSkills[i]);
                height += 60;
            }

            for(int i = 0; i < validNodes.Length; i++)
            {
                if (validNodes[i].skillData is null) continue;
                availableSkills.Add(validNodes[i].skillData);
                height += 60;
            }

            for (int i = 0; i < _equippableSkillButtons.Length; i++)
            {
                _equippableSkillButtons[i].Unequip();

                if (availableSkills.Count <= i)
                {
                    _equippableSkillButtons[i].Hide();
                    continue;
                }

                _equippableSkillButtons[i].Show(availableSkills[i]);
            }
        }
        else
        {
            List<PassiveData> availablePassives = new List<PassiveData>();

            for (int i = 0; i < validNodes.Length; i++)
            {
                if (validNodes[i].passiveData is null) continue;
                availablePassives.Add(validNodes[i].passiveData);
                height += 60;
            }

            for(int i = 0; i < _equippableSkillButtons.Length; i++)
            {
                _equippableSkillButtons[i].Unequip();

                if (availablePassives.Count <= i)
                {
                    _equippableSkillButtons[i].Hide();
                    continue;
                }

                _equippableSkillButtons[i].Show(availablePassives[i]);
            }
        }

        LoadEquippedButtons();
        _buttonsParent.sizeDelta = new Vector2(_buttonsParent.rect.width, height); 
    }

    private SkillTreeNodeData[] GetAllPossessedNodes(SkillTreeData skillTreeData, bool[] unlockedNodes)
    {
        SkillTreeNodeData[] validNodes = new SkillTreeNodeData[unlockedNodes.Length];   // "2 +" because a hero starts with 2 skills at level 0 
        int i = 0;

        for (int x = 0; x < skillTreeData.skillTreeRows.Length; x++)
        {
            for (int y = 0; y < skillTreeData.skillTreeRows[x].rowNodes.Length; y++)
            {
                if (unlockedNodes[i])
                    validNodes[i] = skillTreeData.skillTreeRows[x].rowNodes[y];

                i++;
            }
        }

        return validNodes;
    }

    private void LoadEquippedButtons()
    {
        SkillData[] equippedSkills = currentHero.EquippedSkills;
        PassiveData[] equippedPassives = currentHero.EquippedPassives;

        for(int i = 0; i < 6; i++)
        {
            if(currentHero.CurrentLevel < currentHero.HeroData.heroSkillSlotsUnlockedLevels[i])
            {
                _equippedSkillButtons[i].SetIsLocked(currentHero.HeroData.heroSkillSlotsUnlockedLevels[i]);
            }
            else if(equippedSkills[i] is not null)
            {
                _equippedSkillButtons[i].SetEquippedElement(equippedSkills[i]);
                EquipSkill(equippedSkills[i]);
            }
            else
            {
                _equippedSkillButtons[i].SetEquippedElement(null as SkillData);
            }
        }

        for(int i = 6; i < 9; i++)
        {
            if (currentHero.CurrentLevel < currentHero.HeroData.heroPassiveSlotsUnlockedLevels[i - 6])
            {
                _equippedSkillButtons[i].SetIsLocked(currentHero.HeroData.heroPassiveSlotsUnlockedLevels[i - 6]);
            }
            else if (equippedPassives[i - 6] is not null)
            {
                _equippedSkillButtons[i].SetEquippedElement(equippedPassives[i - 6]);
                EquipPassive(equippedPassives[i - 6]);
            }
            else
            {
                _equippedSkillButtons[i].SetEquippedElement(null as PassiveData);
            }
        }
    }

    #endregion


    #region Hide / Show

    public void Show()
    {
        if (isInTransition || draggedInfos is not null) return;

        OnShow.Invoke();
        isInTransition = true;
        isOnSkills = true;

        HeroesManager.Instance.StopControl();
        SetupHero(HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex]);
        currentHeroIndex = HeroesManager.Instance.CurrentHeroIndex;

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        SetupQuickChangeButtons();

        _leftPartParent.DOLocalMove(_leftPartParent.parent.InverseTransformPoint(_leftShownPosTr.position), 0.3f).SetEase(Ease.OutBack);
        _rightPartParent.DOLocalMove(_rightPartParent.parent.InverseTransformPoint(_rightShownPosTr.position), 0.3f).SetEase(Ease.OutBack);

        _backFadeImage.enabled = true;
        _backFadeImage.DOFade(0.7f, 0.3f);

        yield return new WaitForSeconds(0.3f);

        isInTransition = false;
    }

    public void Hide()
    {
        if (isInTransition || draggedInfos is not null) return;

        OnHide.Invoke();
        isInTransition = true;

        HeroesManager.Instance.RestartControl();

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _backFadeImage.DOFade(0f, 0.25f);

        _leftPartParent.DOLocalMove(_leftPartParent.parent.InverseTransformPoint(_leftShownPosTr.position) + Vector3.left * 550f, 0.25f).SetEase(Ease.InBack);
        _rightPartParent.DOLocalMove(_rightPartParent.parent.InverseTransformPoint(_rightShownPosTr.position) + Vector3.right * 550f, 0.25f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.3f);

        _backFadeImage.enabled = false;
        isInTransition = false;
    }

    #endregion
}
