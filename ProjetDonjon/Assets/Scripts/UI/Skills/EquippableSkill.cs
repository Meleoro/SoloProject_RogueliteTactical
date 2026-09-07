using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class EquippableSkill : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color hoveredColor;
    [SerializeField] private Color unhoveredColor;
    [SerializeField] private Color hoveredTextColor;
    [SerializeField] private Color unhoveredTextColor;
    [SerializeField] private Color highlightIconColor;
    [SerializeField] private int normalHeight;
    [SerializeField] private int hoveredHeight;

    [Header("Actions")]
    public Action<EquippableSkill> OnClick;
    public Action<EquippableSkill> OnHover;
    public Action OnUnhover;

    [Header("Private Infos")]
    private SkillData skillData;
    private PassiveData passiveData;
    private bool isEquipped;
    private Coroutine dragCoroutine;

    [Header("Public Infos")]
    public SkillData SkillData { get { return skillData; } }
    public PassiveData PassiveData { get { return passiveData; } }
    public bool IsEquipped { get { return isEquipped; } }

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _mainImage;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image[] _skillPointsIcons;
    private VerticalLayoutGroup _verticalLayoutGroup;

    private void Start()
    {
        _verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
    }


    private void ActualiseDetails()
    {
        if(skillData is not null)
        {
            _buttonText.text = skillData.skillName;
            _skillIcon.sprite = skillData.skillIcon;

            for (int i = 0; i < _skillPointsIcons.Length; i++)
            {
                if(skillData.skillPointCost <= i)
                {
                    _skillPointsIcons[i].gameObject.SetActive(false);
                    continue;
                }

                _skillPointsIcons[i].gameObject.SetActive(true);
            }
        }
        else if(passiveData is not null) 
        {
            _buttonText.text = passiveData.passiveName;
            _skillIcon.sprite = passiveData.passiveIcon;

            for(int i = 0; i < _skillPointsIcons.Length; i++)
            {
                _skillPointsIcons[i].gameObject.SetActive(false);
            }
        }
    }


    public void Equip(bool playSound = false)
    {
        isEquipped = true;

        if (skillData is not null) _skillIcon.sprite = skillData.skillHighlightIcon;
        else _skillIcon.sprite = passiveData.passiveHighlightIcon;

        _skillIcon.DOColor(highlightIconColor, 0.15f).SetEase(Ease.OutCubic);
        _skillIcon.rectTransform.DOScale(Vector3.one * 1.05f, 0.15f).SetEase(Ease.OutElastic);

        if (playSound) AudioManager.Instance.PlaySoundOneShot(0, 4);
    }

    public void Unequip(bool playSound = false)
    {
        isEquipped = false;

        if (skillData is not null) _skillIcon.sprite = skillData.skillIcon;
        else if (passiveData is not null) _skillIcon.sprite = passiveData.passiveIcon;

        _skillIcon.DOComplete();
        _skillIcon.color = Color.white;
        _skillIcon.rectTransform.localScale = Vector3.one;

        if(playSound) AudioManager.Instance.PlaySoundOneShot(0, 5);
    }


    #region Inputs

    public void HoverButton()
    {
        OnHover.Invoke(this);

        _mainRectTr.DOScale(Vector3.one * 1.05f, 0.15f).SetEase(Ease.OutCubic);
        _mainImage.DOColor(hoveredColor, 0.2f).SetEase(Ease.OutCubic);
        _buttonText.DOColor(hoveredTextColor, 0.2f).SetEase(Ease.OutCubic);

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);

        StartCoroutine(AdaptHeightCoroutine(normalHeight, hoveredHeight, 0.15f));
    }

    private IEnumerator AdaptHeightCoroutine(float startHeight, float endHeight, float duration)
    {
        float timer = 0;

        while(timer < duration)
        {
            timer += Time.deltaTime;

            _mainRectTr.sizeDelta = new Vector2(_mainRectTr.sizeDelta.x, Mathf.Lerp(startHeight, endHeight, timer / duration));
            _verticalLayoutGroup.enabled = false;
            _verticalLayoutGroup.enabled = true;

            yield return new WaitForEndOfFrame();
        }
    }

    public void UnhoverButton()
    {
        OnUnhover.Invoke();

        _mainRectTr.DOScale(Vector3.one * 0.95f, 0.15f).SetEase(Ease.OutCubic);
        _mainImage.DOColor(unhoveredColor, 0.2f).SetEase(Ease.OutCubic);
        _buttonText.DOColor(unhoveredTextColor, 0.2f).SetEase(Ease.OutCubic);

        StartCoroutine(AdaptHeightCoroutine(hoveredHeight, normalHeight, 0.15f));
    }

    public void ClickButton()
    {
        OnClick.Invoke(this);
    }

    public void StartDrag()
    {
        //OnStartDrag?.Invoke(this);
    }

    public void EndDrag()
    {
        //OnEndDrag?.Invoke();
    }

    #endregion


    #region Hide / Show

    public void Show(SkillData data)
    {
        _mainRectTr.gameObject.SetActive(true);

        passiveData = null;
        skillData = data;

        ActualiseDetails();
    }

    public void Show(PassiveData data)
    {
        _mainRectTr.gameObject.SetActive(true);

        skillData = null;
        passiveData = data;

        ActualiseDetails();
    }

    public void Hide()
    {
        _mainRectTr.gameObject.SetActive(false);
    }

    #endregion


    #region Drag Preview 

    public void ShowDragPreview(SkillData data)
    {
        gameObject.SetActive(true);

        passiveData = null;
        skillData = data;
        ActualiseDetails();

        Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        _mainRectTr.position = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);

        dragCoroutine = StartCoroutine(DragCoroutine());
    }

    public void ShowDragPreview(PassiveData data)
    {
        gameObject.SetActive(true);

        passiveData = data;
        skillData = null;
        ActualiseDetails();

        Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        _mainRectTr.position = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);

        dragCoroutine = StartCoroutine(DragCoroutine());
    }

    private IEnumerator DragCoroutine()
    {
        _mainImage.raycastTarget = false;
        _mainImage.color = new Color(1, 1, 1, 0.5f);
        _skillIcon.color = new Color(1, 1, 1, 0.5f);
        _buttonText.color = new Color(1, 1, 1, 0.5f);

        Vector3 dragWantedPos;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            Vector2 mousePos = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
            dragWantedPos = new Vector3(mousePos.x, mousePos.y, CameraManager.Instance.transform.position.z + 9);

            _mainRectTr.position = Vector3.Lerp(_mainRectTr.position, dragWantedPos, Time.deltaTime * 10f);
        }
    }

    public void HideDragPreview()
    {
        StopCoroutine(dragCoroutine);

        gameObject.SetActive(false);
    }

    #endregion 
}
