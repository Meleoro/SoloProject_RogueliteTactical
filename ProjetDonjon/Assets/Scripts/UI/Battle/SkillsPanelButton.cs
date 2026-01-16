using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class SkillsPanelButton : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color overlayActionAddedColor;
    [SerializeField] private Color clickedActionAddedColor;
    [SerializeField] private Color impossibleActionRemovedColor;
    [SerializeField] private Color hoverTextColor;
    [SerializeField] private Color baseTextColor;
    [SerializeField] private Sprite fullSPSprite;
    [SerializeField] private Sprite emptySPSprite;

    [Header("Actions")]
    public Action<SkillsPanelButton> OnButtonClick;
    public Action<SkillData> OnSkillOverlay;
    public Action OnSkillQuitOverlay;

    [Header("Private Infos")]
    private SkillData skillData;
    private Color colorSave;
    private bool canBeUsed;
    private bool isLocked;
    private bool isClicked;
    private float skillAreaSize;

    [Header("Public Infos")]
    public SkillData SkillData { get { return skillData; } }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image[] _skillPointsImages;
    private RectTransform _rectTr;


    private void Start()
    {
        colorSave = _buttonImage.color;
        _rectTr = GetComponent<RectTransform>();
    }

    public void InitialiseButton(SkillData data, Hero hero)
    {
        skillData = data;

        _buttonText.text = skillData.skillName;
        _skillIcon.sprite = skillData.skillIcon;    

        canBeUsed = true;
    }

    public void ActualiseSkillPointsImages(Hero currentHero)
    {
        for(int i = 0; i < _skillPointsImages.Length; i++)
        {
            _skillPointsImages[i].sprite = i < currentHero.CurrentSkillPoints ? fullSPSprite : emptySPSprite;
            _skillPointsImages[i].gameObject.SetActive(i < skillData.skillPointCost);
        }

        if(currentHero.CurrentSkillPoints < skillData.skillPointCost)
        {
            canBeUsed = false;
        }
    }


    public void LockButton()
    {
        isLocked = true;
    }

    public void UnlockButton()
    {
        isLocked = false;
    }


    #region Overlay / Click Functions

    public void OverlayButton()
    {
        if (isClicked)
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave + overlayActionAddedColor + clickedActionAddedColor);
            _rectTr.UChangeScale(0.1f, Vector3.one * 1.05f);
        }
        else
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave + overlayActionAddedColor);
            _rectTr.UChangeScale(0.1f, Vector3.one * 1f);

            OnSkillOverlay.Invoke(skillData);
        }

        _skillIcon.color = hoverTextColor;
        _skillIcon.sprite = skillData.skillHighlightIcon;
        _buttonText.color = hoverTextColor;

        List<BattleTile> concernedTiles = BattleManager.Instance.TilesManager.DisplayPossibleSkillTiles(skillData, BattleManager.Instance.CurrentUnit.CurrentTile);
        float averageDist = 0;

        foreach(BattleTile battleTile in concernedTiles)
        {
            averageDist += Vector2.Distance(battleTile.transform.position, BattleManager.Instance.CurrentUnit.transform.position);
        }

        averageDist /= concernedTiles.Count;
        CameraManager.Instance.FocusOnTransform(BattleManager.Instance.CurrentUnit.transform, averageDist * 2f);
    }

    public void QuitOverlayButton(bool noActionCall = false)
    {
        if (isClicked)
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave + clickedActionAddedColor);
            _rectTr.UChangeScale(0.1f, Vector3.one * 1f);
        }
        else
        {
            _buttonImage.ULerpImageColor(0.1f, colorSave);
            _rectTr.UChangeScale(0.1f, Vector3.one * 0.95f);

            if(!noActionCall)
                OnSkillQuitOverlay.Invoke();
        }

        _skillIcon.color = Color.white;
        _skillIcon.sprite = skillData.skillIcon;
        _buttonText.color = baseTextColor;
    }

    public void QuitOverlayButtonInstant(bool noActionCall = false)
    {
        isClicked = false;

        _buttonImage.color = colorSave;
        _rectTr.localScale = Vector3.one * 0.95f;

        _skillIcon.color = Color.white;
        _skillIcon.sprite = skillData.skillIcon;
        _buttonText.color = baseTextColor;

        if (!noActionCall)
            OnSkillQuitOverlay.Invoke();
    }

    public void ClickButton()
    {
        if (!canBeUsed || isLocked) return;

        StartCoroutine(ClickEffectCoroutine());

        if (isClicked) return;

        isClicked = true;
        OnButtonClick.Invoke(this);
    }

    public void Unclick()
    {
        isClicked = false;
        QuitOverlayButton(true);
    }


    private IEnumerator ClickEffectCoroutine()
    {
        _buttonImage.ULerpImageColor(0.1f, colorSave - clickedActionAddedColor);
        _rectTr.UChangeScale(0.1f, Vector3.one * 0.9f);

        yield return new WaitForSeconds(0.1f);

        _buttonImage.ULerpImageColor(0.2f, colorSave + clickedActionAddedColor + overlayActionAddedColor);
        _rectTr.UChangeScale(0.2f, Vector3.one * 1.05f);
    }

    #endregion
}
