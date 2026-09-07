using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class EquippedSkill : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private bool isPassiveSlot;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private Sprite lockIcon;

    [Header("Actions")]
    public Action<EquippedSkill> OnHover;
    public Action OnUnhover;

    [Header("Private Infos")]
    private bool isLocked;
    private SkillData skillData;
    private PassiveData passiveData;
    private Coroutine highlightCoroutine;

    [Header("Public Infos")]
    public bool IsLocked { get { return isLocked; } }
    public bool IsPassiveSlot { get { return isPassiveSlot; } }
    public SkillData SkillData { get { return skillData; } }
    public PassiveData PassiveData { get { return passiveData; } }

    [Header("References")]
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Image _backImage;
    [SerializeField] private Image _highlightImage;
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private Image _skillIcon;


    private void ActualiseVisuals()
    {
        if (isLocked)
        {
            _skillIcon.sprite = lockIcon;
        }
        else
        {
            _skillIcon.sprite = emptyIcon;
        }

        if(skillData is not null)
        {
            _mainText.enabled = true;
            _mainText.text = skillData.skillName;
            _skillIcon.sprite = skillData.skillHighlightIcon;
        }

        else if (passiveData is not null)
        {
            _mainText.enabled = true;
            _mainText.text = passiveData.passiveName;
            _skillIcon.sprite = passiveData.passiveHighlightIcon;
        }
        else if(!isLocked)
        {
            _mainText.enabled = false;  
        }
    }


    #region Set State

    public void SetEquippedElement(SkillData equippedSkill)
    {
        passiveData = null;
        skillData = equippedSkill;

        ActualiseVisuals();
    }

    public void SetEquippedElement(PassiveData equippedPassive)
    {
        skillData = null;
        passiveData = equippedPassive;

        ActualiseVisuals();
    }
    
    // Called when a slot is empty or locked
    public void SetIsLocked(int lockedLevel)
    {
        isLocked = true;

        _mainText.enabled = true;
        _mainText.text = "LV." + lockedLevel;

        skillData = null;
        passiveData = null;

        ActualiseVisuals();
    }

    #endregion


    #region Mouse Inputs

    public void Hover()
    {
        OnHover.Invoke(this);
    }

    public IEnumerator HoverFeelCoroutine()
    {
        _rectTr.UChangeScale(0.1f, Vector3.one * 1.2f, CurveType.EaseOutSin);

        yield return new WaitForSeconds(0.1f);

        _rectTr.UChangeScale(0.2f, Vector3.one * 1.1f, CurveType.EaseInOutSin);
    }

    public void Unhover()
    {
        OnUnhover.Invoke();

        _rectTr.UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseOutSin);
    }

    #endregion


    #region Polish

    public void StartHighlight()
    {
        if(highlightCoroutine is not null) StopCoroutine(highlightCoroutine);

        highlightCoroutine = StartCoroutine(HighlightCoroutine(3f));
    }

    public void EndHighlight()
    {
        if (highlightCoroutine is not null) StopCoroutine(highlightCoroutine);

        _highlightImage.UFadeImage(0.5f, 0f, CurveType.EaseInOutSin);
    }

    private IEnumerator HighlightCoroutine(float loopDuration)
    {
        while(true)
        {
            _highlightImage.UFadeImage(loopDuration * 0.45f, 1f, CurveType.EaseInOutSin);

            yield return new WaitForSeconds(loopDuration * 0.5f);

            _highlightImage.UFadeImage(loopDuration * 0.45f, 0f, CurveType.EaseInOutSin);

            yield return new WaitForSeconds(loopDuration * 0.5f);
        }
    }

    #endregion
}
