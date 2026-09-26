using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public enum MenuType
{
    BaseActions,
    Move,
    Skills,
    LaunchSkill,
    UseItems
}


public class PlayerActionsMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Color impossibleActionRemovedColor;
    [SerializeField] private Color overlayActionAddedColor;
    [SerializeField] private Color textHoverColor;
    [SerializeField] private Color textUnhoverColor;
    [SerializeField] private Sprite filledActionPointSprite;
    [SerializeField] private Sprite emptyActionPointSprite;
    [SerializeField] private Sprite filledSkillPointSprite;
    [SerializeField] private Sprite emptySkillPointSprite;

    [Header("Actions")]
    public Action<int> OnMoveAction;
    public Action<int> OnSkillAction;

    [Header("Private Infos")]
    private Hero currentHero;
    private Color[] colorSaves;
    private bool isOpened;
    private MenuType currentMenu;
    private Vector3 saveScaleButton;
    private bool isInTransition;

    [Header("Public Infos")]
    public MenuType CurrentMenu { get { return currentMenu; } }
    public Image[] ActionPointImages { get { return _actionPointHighlightImages; } }
    public Image[] SkillPointImages { get { return _skillPointHighlightImages; } }
    public Image[] ButtonImages { get { return _buttonImages; } }
    public SkillsPanel SkillsPanel { get { return _skillsPanel; } }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Image[] _buttonImages;
    [SerializeField] private SkillsPanel _skillsPanel;
    [SerializeField] private Image[] _skillPointImages;
    [SerializeField] private Image[] _actionPointImages;
    [SerializeField] private Image[] _skillPointHighlightImages;
    [SerializeField] private Image[] _actionPointHighlightImages;
    [SerializeField] private TextMeshProUGUI[] _buttonTexts;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;


    private void Start()
    {
        colorSaves = new Color[_buttonImages.Length];
        for (int i = 0; i < _buttonImages.Length; i++)
        {
            colorSaves[i] = _buttonImages[i].color;
        }

        _skillsPanel.OnLaunchSkill += StartLaunchSkillAction;
    }

    private void Update()
    {
        if (!BattleManager.Instance.IsInBattle) return;
        if (BattleManager.Instance.CurrentUnit is null) return;
        if (BattleManager.Instance.CurrentUnit.GetType() != typeof(Hero)) return;

        if (InputManager.wantsToReturn && !TutoManager.Instance.IsDisplayingTuto && !isInTransition)
        {
            ReturnPreviousBattleMenu();
        }
    }


    public void SetupHeroActionsUI(Hero currentHero)
    {
        this.currentHero = currentHero;

        _nameText.text = currentHero.HeroData.unitName;

        OpenActionsMenu();
    }

    public void OpenActionsMenu()
    {
        if (currentHero.CurrentActionPoints == 0) return;
        if (!BattleManager.Instance.IsInBattle) return;

        CameraManager.Instance.FocusOnTransform(currentHero.transform, 5f);
        CameraManager.Instance.OnCameraMouseInput += CloseActionsMenu;
        currentHero.OnClickUnit -= OpenActionsMenu;
        if (currentMenu == MenuType.Move) currentHero.OnClickUnit -= ReturnPreviousBattleMenu;

        currentHero.EndTurnOutline(false);

        StartCoroutine(AdaptHeightCoroutine(null, 0, 0, 0.5f));

        currentMenu = MenuType.BaseActions;
        isOpened = true;

        transform.position = currentHero.transform.position + positionOffset;
        _animator.SetBool("IsOpenned", true);

        if(currentHero.IsHindered) _buttonImages[0].ULerpImageColor(0.1f, colorSaves[0] - impossibleActionRemovedColor);

        for (int i = 0; i < 4; i++)
        {
            QuitOverlayButtonInstant(i);
        }

        for(int i = 0; i < _skillPointImages.Length; i++)
        {
            if (i >= currentHero.CurrentMaxSkillPoints)
            {
                _skillPointImages[i].gameObject.SetActive(false);
                continue;
            }

            _skillPointImages[i].gameObject.SetActive(true);
            _skillPointImages[i].sprite = currentHero.CurrentSkillPoints > i ? filledSkillPointSprite : emptySkillPointSprite;
        }

        for (int i = 0; i < _actionPointImages.Length; i++)
        {
            _actionPointImages[i].sprite = currentHero.CurrentActionPoints > i ? filledActionPointSprite : emptyActionPointSprite;
        }
    }

    public void CloseActionsMenu()
    {
        CameraManager.Instance.OnCameraMouseInput -= CloseActionsMenu;

        StartCoroutine(AdaptHeightCoroutine(null, 0, 0, 0.5f));

        if (currentMenu != MenuType.Skills && currentMenu != MenuType.Move && currentMenu != MenuType.UseItems)
        { 
            currentHero.OnClickUnit += OpenActionsMenu;
            currentHero.StartTurnOutline();
        }
        //else if (currentMenu == MenuType.Move) currentHero.OnClickUnit += ReturnPreviousBattleMenu;

        _animator.SetBool("IsOpenned", false);
        isOpened = false;
    }

    private void EndTransition()
    {
        isInTransition = false;

        if(currentMenu == MenuType.UseItems)
        {
            currentHero.Inventory.OnTransitionEnd -= EndTransition;
        }
    }


    #region Button Effects

    public void StartMoveAction()
    {
        if (currentHero.IsHindered) return;
        if (isInTransition || currentMenu != MenuType.BaseActions) return;

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        OnMoveAction?.Invoke(0);

        currentMenu = MenuType.Move;
        BattleManager.Instance.TilesManager.DisplayPossibleMoveTiles(currentHero, true);

        CloseActionsMenu();
    }

    public void StartSkillsAction()
    {
        if (isInTransition || (currentMenu != MenuType.BaseActions && currentMenu != MenuType.LaunchSkill)) return;

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        currentMenu = MenuType.Skills;
        _skillsPanel.OpenSkillsPanel(currentHero);

        OnSkillAction?.Invoke(0);

        CloseActionsMenu();
    }

    public void StartLaunchSkillAction()
    {
        if (isInTransition) return;

        currentMenu = MenuType.LaunchSkill;

        currentHero.OnClickUnit -= OpenActionsMenu;

        _skillsPanel.CloseSkillsPanel();
    }

    public void StartUseObjectAction()
    {
        if (isInTransition || currentMenu != MenuType.BaseActions) return;

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        currentMenu = MenuType.UseItems;

        StartCoroutine(InventoriesManager.Instance.OpenInventory(currentHero.Inventory));
        CloseActionsMenu();

        currentHero.Inventory.OnTransitionEnd += EndTransition;
    }

    public void EndTurnAction()
    {
        if (isInTransition || currentMenu != MenuType.BaseActions) return;

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        currentHero.EndTurn(0.5f);

        BattleManager.Instance.TilesManager.UnhoverAll();
        CloseActionsMenu();
    }

    #endregion


    #region Overlay / Click Effects

    public void OverlayButton(int buttonIndex)
    {
        if (isInTransition) return;
        if (buttonIndex == 0 && currentHero.IsHindered) return;

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);

        _buttonImages[buttonIndex].DOComplete();
        _buttonImages[buttonIndex].rectTransform.DOComplete();

        saveScaleButton = _buttonImages[buttonIndex].rectTransform.localScale;

        _buttonImages[buttonIndex].DOColor(colorSaves[buttonIndex] + overlayActionAddedColor, 0.15f).SetEase(Ease.OutCubic);
        _buttonImages[buttonIndex].rectTransform.DOScale(saveScaleButton * 1.05f, 0.15f).SetEase(Ease.OutCubic);

        _buttonTexts[buttonIndex].DOComplete();
        _buttonTexts[buttonIndex].DOColor(textHoverColor, 0.1f);

        StartCoroutine(AdaptHeightCoroutine(_buttonImages[buttonIndex].rectTransform.parent.GetComponent<RectTransform>(), 25, 30, 0.1f));
    }

    public void QuitOverlayButton(int buttonIndex)
    {
        if (buttonIndex == 0 && currentHero.IsHindered) return;

        _buttonImages[buttonIndex].DOKill();
        _buttonImages[buttonIndex].rectTransform.DOKill();

        _buttonImages[buttonIndex].DOColor(colorSaves[buttonIndex], 0.15f).SetEase(Ease.OutCubic);
        _buttonImages[buttonIndex].rectTransform.DOScale(saveScaleButton, 0.15f).SetEase(Ease.OutCubic);

        _buttonTexts[buttonIndex].DOComplete();
        _buttonTexts[buttonIndex].DOColor(textUnhoverColor, 0.1f);

        StartCoroutine(AdaptHeightCoroutine(_buttonImages[buttonIndex].rectTransform.parent.GetComponent<RectTransform>(), 30, 25, 0.1f));
    }

    public void QuitOverlayButtonInstant(int buttonIndex)
    {
        if (buttonIndex == 0 && currentHero.IsHindered) return;

        _buttonImages[buttonIndex].color = colorSaves[buttonIndex];
        _buttonImages[buttonIndex].rectTransform.localScale = Vector3.one;

        _buttonTexts[buttonIndex].DOComplete();
        _buttonTexts[buttonIndex].color = textUnhoverColor;

        _buttonImages[buttonIndex].rectTransform.parent.GetComponent<RectTransform>().sizeDelta = 
            new Vector2(_buttonImages[buttonIndex].rectTransform.parent.GetComponent<RectTransform>().sizeDelta.x, 25);
        _verticalLayoutGroup.enabled = false;
        _verticalLayoutGroup.enabled = true;
    }

    private IEnumerator AdaptHeightCoroutine(RectTransform rectTr, float startHeight, float endHeight, float duration)
    {
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if(rectTr)
            {
                rectTr.sizeDelta = new Vector2(rectTr.sizeDelta.x, Mathf.Lerp(startHeight, endHeight, timer / duration));
            }

            _verticalLayoutGroup.enabled = false;
            _verticalLayoutGroup.enabled = true;

            yield return new WaitForEndOfFrame();
        }
    }

    #endregion


    #region Others

    private void ReturnPreviousBattleMenu()
    {
        switch (currentMenu)
        {
            case MenuType.Move:
                OpenActionsMenu();
                CameraManager.Instance.FocusOnTransform(currentHero.transform, 5.5f);
                BattleManager.Instance.TilesManager.ResetTiles();
                break;

            case MenuType.Skills:
                OpenActionsMenu();
                CameraManager.Instance.FocusOnTransform(currentHero.transform, 5.5f);
                BattleManager.Instance.TilesManager.ResetTiles();
                _skillsPanel.CloseSkillsPanel();
                break;

            case MenuType.LaunchSkill:
                BattleManager.Instance.TilesManager.ResetTiles();
                StartSkillsAction();
                break;

            case MenuType.BaseActions:
                if (!isOpened)
                {
                    OpenActionsMenu();
                }
                break;

            case MenuType.UseItems:
                OpenActionsMenu();
                StartCoroutine(InventoriesManager.Instance.CloseInventory(currentHero.Inventory));
                break;
        }
    }

    #endregion
}
