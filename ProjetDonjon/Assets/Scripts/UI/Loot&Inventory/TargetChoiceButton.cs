using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class TargetChoiceButton : MonoBehaviour
{
    [Header("Actions")]
    public Action OnSizeChange;
    public Action<int> OnClickTarget;
    public Action<bool> OnClickReturn;

    [Header("Private Infos")]
    private int heroIndex;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private Image _spImage;
    [SerializeField] private Image _hearthImage;
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _secondaryText;


    public void ActualiseInfos(LootData loot, Hero hero)
    {
        _mainText.text = hero.HeroData.unitName;

        _secondaryText.enabled = false;
        _hearthImage.enabled = false;
        _spImage.enabled = false;

        heroIndex = hero.HeroIndex; 

        if (loot.consumableType == ConsumableType.Heal)
        {
            _secondaryText.enabled = true;
            _secondaryText.text = hero.CurrentHealth + "/" + hero.CurrentMaxHealth;

            _hearthImage.enabled = true;
        }
        else if(loot.consumableType == ConsumableType.Focus)
        {
            _secondaryText.enabled = true;
            _secondaryText.text = hero.CurrentSkillPoints + "/" + hero.CurrentMaxSkillPoints;

            _spImage.enabled = true;
        }
    }

    public void HoverButton()
    {
        _mainRectTr.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutBack);
        _mainRectTr.sizeDelta = new Vector2(_mainRectTr.sizeDelta.x, 42);

        OnSizeChange.Invoke();

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);
    }

    public void UnhoverButton()
    {
        _mainRectTr.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        _mainRectTr.sizeDelta = new Vector2(_mainRectTr.sizeDelta.x, 35);

        OnSizeChange.Invoke();
    }

    public void ClickTarget()
    {
        OnClickTarget.Invoke(heroIndex);
    }

    public void ClickReturn()
    {
        OnClickReturn(true);
    }
}
