using System;
using System.Collections;
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
    private SkillsPanelButton currentButtonSkill;
    private bool isOpenned;
    private SkillsPanelButton lockedButton;

    [Header("References")]
    [SerializeField] private SkillsPanelButton[] _skillButtons;
    [SerializeField] private RectTransform _detailsPosRectTr;
    [SerializeField] private VerticalLayoutGroup _verticalLayout;
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

            StartCoroutine(_skillButtons[i].ShowButtonCoroutine(0.04f + i * 0.04f));
        }

        StartCoroutine(ForceVerticalGroupDirtyCoroutine(1.0f));

        _animator.SetBool("IsOpenned", true);
    }

    private IEnumerator ForceVerticalGroupDirtyCoroutine(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            _verticalLayout.enabled = false;
            _verticalLayout.enabled = true;

            yield return new WaitForEndOfFrame();
        }
    } 

    public void CloseSkillsPanel()
    {
        if (!isOpenned) return;

        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();

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
        if (!isOpenned) return;

        UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(skillData, _detailsPosRectTr.position, true);
    }

    private void UnloadSkillDetails()
    {
        if(currentButtonSkill != null)
        {
            BattleManager.Instance.TilesManager.DisplayPossibleSkillTiles(currentButtonSkill.SkillData, BattleManager.Instance.CurrentUnit.CurrentTile);
        }
        else
        {
            BattleManager.Instance.TilesManager.ResetTiles();
        }
    }
}
