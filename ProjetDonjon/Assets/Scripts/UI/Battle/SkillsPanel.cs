using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 offset;

    [Header("Actions")]
    public Action OnLaunchSkill;

    [Header("Private Infos")]
    private Hero currentHero;
    private SkillsPanelButton currentButtonSkill;
    private bool isOpenned;
    private SkillsPanelButton lockedButton;

    [Header("References")]
    [SerializeField] private SkillsPanelButton[] _skillButtons;
    [SerializeField] private AdditionalTooltip[] _additionalTooltips;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private TextMeshProUGUI _skillDescriptionText;
    private Animator _animator;


    private void Start()
    {
        _animator = GetComponent<Animator>();

        for(int i = 0; i < _skillButtons.Length; i++)
        {
            _skillButtons[i].OnButtonClick += ClickButton;
            _skillButtons[i].OnSkillOverlay += LoadSkillDetails;
            _skillButtons[i].OnSkillQuitOverlay += UnloadSkillDetails;
        }

        BattleManager.Instance.OnSkillUsed += CloseSkillsPanel;
    }


    #region Open / Close

    public void OpenSkillsPanel(Hero hero)
    {
        if (isOpenned) return;

        currentHero = hero;
        isOpenned = true;

        currentButtonSkill = null;

        transform.position = hero.transform.position + offset;

        for (int i = 0; i < _skillButtons.Length; i++)
        {
            if(hero.EquippedSkills.Length <= i || hero.EquippedSkills[i] == null)
            {
                _skillButtons[i].gameObject.SetActive(false);
                continue;
            }

            _skillButtons[i].gameObject.SetActive(true);
            _skillButtons[i].InitialiseButton(hero.EquippedSkills[i], hero);
            _skillButtons[i].ActualiseSkillPointsImages(hero);
            _skillButtons[i].QuitOverlayButtonInstant(true);
        }

        _animator.SetBool("IsOpenned", true);
    }

    public void CloseSkillsPanel()
    {
        if (!isOpenned) return;

        isOpenned = false;
        _animator.SetBool("IsOpenned", false);
    }

    #endregion


    // For tuto 
    public void LockOption(int index)
    {
        if (lockedButton)
        {
            lockedButton.UnlockButton();
            lockedButton = null;
        }

        if(index != -1)
        {
            _skillButtons[index].LockButton();
            lockedButton = _skillButtons[index];
        }
    }


    private void ClickButton(SkillsPanelButton button)
    {
        if (lockedButton == button) return;

        OnLaunchSkill.Invoke();
        currentButtonSkill = button;
    }


    private void LoadSkillDetails(SkillData skillData)
    {
        _skillNameText.text = skillData.skillName;
        _skillDescriptionText.text = skillData.skillDescription;

        for (int i = 0; i < _additionalTooltips.Length; i++)
        {
            if(i < skillData.additionalTooltipDatas.Length)
                _additionalTooltips[i].Show(skillData.additionalTooltipDatas[i], 0);

            else
                _additionalTooltips[i].Hide();
        }
    }

    private void UnloadSkillDetails()
    {
        if(currentButtonSkill != null)
        {
            BattleManager.Instance.TilesManager.DisplayPossibleSkillTiles(currentButtonSkill.SkillData, BattleManager.Instance.CurrentUnit.CurrentTile);

            _skillNameText.text = currentButtonSkill.SkillData.name;
            _skillDescriptionText.text = currentButtonSkill.SkillData.skillDescription;
        }
        else
        {
            BattleManager.Instance.TilesManager.ResetTiles();

            _skillNameText.text = "";
            _skillDescriptionText.text = "";
        }
    }
}
