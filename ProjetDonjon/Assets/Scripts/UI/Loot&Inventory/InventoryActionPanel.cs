using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class InventoryActionPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private Sprite[] possibleMainActionsBackImage;

    [Header("Private Infos")]
    private Loot currentLoot;
    private bool isOpened;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private Image _mainActionsBackImage;
    [SerializeField] private RectTransform[] _buttonsRectTr;
    [SerializeField] private TargetChoiceButton[] _targetChoiceButtons;
    [SerializeField] private TextMeshProUGUI[] _buttonsTexts;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
    [SerializeField] private Animator _animator;


    private void Start()
    {
        for (int i = 0; i < _targetChoiceButtons.Length; i++)
        {
            _targetChoiceButtons[i].OnSizeChange += ActualiseVerticalGroupScale;

            if(i < _targetChoiceButtons.Length - 1)
            {
                _targetChoiceButtons[i].OnClickTarget += ValidateTarget;
            }
            else
            {
                _targetChoiceButtons[i].OnClickReturn += CloseChoseTarget;
            }
        }
    }


    #region Main Panel

    public void OpenPanel(Loot associatedLoot)
    {
        if (isOpened && (associatedLoot == currentLoot))
        {
            ClosePanel();
            return;
        }

        int buttonCount = 1;

        currentLoot = associatedLoot;
        _animator.SetBool("IsOpened", true);
        isOpened = true;

        _buttonsRectTr[0].gameObject.SetActive(false);
        switch (associatedLoot.LootData.lootType)
        {
            case LootType.Equipment:
                if (!currentLoot.GetCanBeEquipped()) break;

                _buttonsRectTr[0].gameObject.SetActive(true);
                if(associatedLoot.IsEquipped) _buttonsTexts[0].text = "UNEQUIP";
                else _buttonsTexts[0].text = "EQUIP";

                buttonCount++;
                break;

            case LootType.Consumable:
                if (!BattleManager.Instance.IsInBattle && !associatedLoot.LootData.usableOutsideBattle) break;

                _buttonsRectTr[0].gameObject.SetActive(true);
                _buttonsTexts[0].text = "USE";

                buttonCount++;
                break;
        }

        _mainRectTr.position = associatedLoot.Image.rectTransform.position + offset;

        // Sell Button
        if (GameManager.Instance.IsInExplo)
        {
            _buttonsRectTr[2].gameObject.SetActive(false);
        }
        else 
        {
            _buttonsRectTr[2].gameObject.SetActive(true);
            _buttonsTexts[2].text = "SELL (" + (int)(associatedLoot.LootData.value * 0.5f) + ")";

            buttonCount++;
        }

        _mainActionsBackImage.sprite = possibleMainActionsBackImage[buttonCount - 1];
        _mainActionsBackImage.SetNativeSize();
    }

    public void ClosePanel(bool closeAlsoTarget = false)
    {
        _animator.SetBool("IsOpened", false);
        isOpened = false;

        if(closeAlsoTarget)
        {
            CloseChoseTarget(false);
        }
    }


    public void HoverButton(int index)
    {
        _buttonsRectTr[index].UChangeScale(0.1f, Vector3.one * 1.1f);
        _buttonsRectTr[index].sizeDelta = new Vector2(_buttonsRectTr[index].sizeDelta.x, 36);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalLayoutGroup.transform as RectTransform);

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);
    }

    public void UnhoverButton(int index)
    {
        _buttonsRectTr[index].UChangeScale(0.1f, Vector3.one);
        _buttonsRectTr[index].sizeDelta = new Vector2(_buttonsRectTr[index].sizeDelta.x, 30);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalLayoutGroup.transform as RectTransform);
    }


    public void PressUse()
    {
        switch (currentLoot.LootData.lootType)
        {
            case LootType.Equipment:
                if (currentLoot.IsEquipped) currentLoot.Unequip();
                else currentLoot.Equip(null);
                break;

            case LootType.Consumable:
                OpenChooseTarget();
                return;
        }

        ClosePanel();
    }

    public void PressThrow()
    {
        currentLoot.BecomeWorldItem();

        ClosePanel();
    }

    public void PressSell()
    {
        InventoriesManager.Instance.AddCoins((int)(currentLoot.LootData.value * 0.5f));

        currentLoot.DestroyItem();

        ClosePanel();
    }

    #endregion


    #region Choose Target Panel

    private void ActualiseVerticalGroupScale()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalLayoutGroup.transform as RectTransform);
    }

    private void OpenChooseTarget()
    {
        ClosePanel();
        _animator.SetBool("ChooseTargetOpened", true);

        Hero[] heroes = HeroesManager.Instance.Heroes;

        for(int i = 0; i < 3; i++)
        {
            if(i < heroes.Length && (heroes[i].CurrentHealth >= 0 || !BattleManager.Instance.IsInBattle))
            {
                _targetChoiceButtons[i].gameObject.SetActive(true);
                _targetChoiceButtons[i].ActualiseInfos(currentLoot.LootData, heroes[i]);
            }
            else
            {
                _targetChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void CloseChoseTarget(bool openMain)
    {
        _animator.SetBool("ChooseTargetOpened", false);

        if (openMain)
        {
            OpenPanel(currentLoot);
        }
    }

    public void ValidateTarget(int index)
    {
        Hero concernedHero = HeroesManager.Instance.Heroes[index];

        concernedHero.UseItem(currentLoot.LootData);
        currentLoot.DestroyItem();

        CloseChoseTarget(false);
    }

    #endregion
}
