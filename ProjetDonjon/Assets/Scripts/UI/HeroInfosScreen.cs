using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

public class HeroInfosScreen : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float openDuration;
    [SerializeField] private float closeDuration;

    [Header("Actions")]
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private HeroData heroData;
    private Hero hero;
    private Inventory currentInventory;
    private bool isOpenning;
    private int currentHeroIndex;
    private Color baseTextColor;

    [Header("References")]
    [SerializeField] private HeroesManager _heroesManager;
    [SerializeField] private TextMeshProUGUI[] _statsTexts;
    [SerializeField] private TextMeshProUGUI _heroName;
    [SerializeField] private RectTransform _shownInfoScreenPosition;
    [SerializeField] private RectTransform _hiddenInfoScreenPosition;
    [SerializeField] private RectTransform _shownInventoryPosition;
    [SerializeField] private RectTransform _hiddenInventoryPosition;
    [SerializeField] private RectTransform _mainRectParent;
    [SerializeField] private EquipmentSlot[] _equipmentSlots;

    [Header("References Buttons")]
    [SerializeField] private RectTransform _leftButton;
    [SerializeField] private RectTransform _rightButton;
    [SerializeField] private RectTransform _shownLeftButtonPosition;
    [SerializeField] private RectTransform _shownRightButtonPosition;


    private void Start()
    {
        for(int i = 0; i < _equipmentSlots.Length; i++)
        {
            _equipmentSlots[i].OnEquipmentAdd += AddEquipment;
            _equipmentSlots[i].OnEquipmentRemove += RemoveEquipment;
        }

        UIManager.Instance.OnStartDrag += StartDrag;
        UIManager.Instance.OnStopDrag += StopDrag;

        baseTextColor = _statsTexts[0].color;
    }


    public void ChangeHero(bool left)
    {
        if(left) currentHeroIndex = (--currentHeroIndex) % _heroesManager.Heroes.Length;
        else currentHeroIndex = (++currentHeroIndex) % _heroesManager.Heroes.Length;

        if (currentHeroIndex < 0) currentHeroIndex += _heroesManager.Heroes.Length;

        StartCoroutine(ChangeHeroCoroutine(left));

        //ActualiseInfoScreen(_heroesManager.Heroes[currentHeroIndex]);
        //ActualiseInventory();
    }

    private IEnumerator ChangeHeroCoroutine(bool goLeft)
    {
        isOpenning = true;

        Vector3 pos1 = goLeft ? new Vector3(-800, 0, 0) : new Vector3(800, 0, 0);
        Quaternion rot1 = goLeft ? Quaternion.Euler(0, 0, 15) : Quaternion.Euler(0, 0, -15);
        currentInventory.RectTransform.DOLocalMove(pos1, 0.2f).SetEase(Ease.InCubic);
        currentInventory.RectTransform.DOLocalRotate(rot1.eulerAngles, 0.2f).SetEase(Ease.InCubic);
        currentInventory.LootParent.DOLocalMove(pos1, 0.2f).SetEase(Ease.InCubic);
        currentInventory.LootParent.DOLocalRotate(rot1.eulerAngles, 0.2f).SetEase(Ease.InCubic);
        _mainRectParent.DOLocalMove(pos1, 0.2f).SetEase(Ease.InCubic);
        _mainRectParent.DOLocalRotate(rot1.eulerAngles, 0.2f).SetEase(Ease.InCubic);

        yield return new WaitForSeconds(0.2f);

        currentInventory.RectTransform.localRotation = Quaternion.Euler(0, 0, 0);
        currentInventory.LootParent.localRotation = Quaternion.Euler(0, 0, 0);
        _mainRectParent.localRotation = Quaternion.Euler(0, 0, 0);

        ActualiseInfoScreen(_heroesManager.Heroes[currentHeroIndex], false);
        ActualiseInventory();

        currentInventory.RectTransform.localRotation = rot1;
        currentInventory.LootParent.localRotation = rot1;

        currentInventory.RectTransform.localPosition = -pos1;
        currentInventory.RectTransform.UChangeLocalPosition(0.2f, _shownInventoryPosition.localPosition, CurveType.EaseOutCubic);
        currentInventory.RectTransform.UChangeLocalRotation(0.2f, Quaternion.Euler(0, 0, 0), CurveType.EaseOutCubic);

        currentInventory.LootParent.localPosition = -pos1;
        currentInventory.LootParent.UChangeLocalPosition(0.2f, _shownInventoryPosition.localPosition, CurveType.EaseOutCubic);
        currentInventory.LootParent.UChangeLocalRotation(0.2f, Quaternion.Euler(0, 0, 0), CurveType.EaseOutCubic);

        _mainRectParent.localRotation = rot1;

        _mainRectParent.localPosition = -pos1;
        _mainRectParent.UChangeLocalPosition(0.2f, _shownInfoScreenPosition.localPosition, CurveType.EaseOutCubic);
        _mainRectParent.UChangeLocalRotation(0.2f, Quaternion.Euler(0, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.2f);

        isOpenning = false;
    }


    #region Open / Close Functions

    public IEnumerator OpenInfosScreenCoroutine()
    {
        ActualiseInfoScreen(_heroesManager.Heroes[_heroesManager.CurrentHeroIndex], false);
        currentHeroIndex = _heroesManager.CurrentHeroIndex;

        HeroesManager.Instance.Heroes[currentHeroIndex].Controller.StopControl();

        OnShow.Invoke();

        isOpenning = true;
        currentInventory = hero.Inventory;

        currentInventory.RectTransform.position = _hiddenInventoryPosition.position;
        currentInventory.LootParent.position = _hiddenInventoryPosition.position;
        _mainRectParent.UChangePosition(openDuration, _shownInfoScreenPosition.position, CurveType.EaseOutCubic);
        currentInventory.RectTransform.UChangePosition(openDuration, _shownInventoryPosition.position, CurveType.EaseOutCubic);
        currentInventory.LootParent.UChangePosition(openDuration, _shownInventoryPosition.position, CurveType.EaseOutCubic);

        _leftButton.UChangePosition(openDuration, _shownLeftButtonPosition.position, CurveType.EaseOutCubic);
        _rightButton.UChangePosition(openDuration, _shownRightButtonPosition.position, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(openDuration);

        isOpenning = false;
    }

    public IEnumerator CloseInfosScreenCoroutine()
    {
        isOpenning = true;

        InventoriesManager.Instance.InventoryActionPanel.ClosePanel();
        HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].Controller.RestartControl();

        OnHide.Invoke();

        _mainRectParent.UChangePosition(closeDuration, _hiddenInfoScreenPosition.position, CurveType.EaseOutCubic);
        currentInventory.RectTransform.UChangePosition(closeDuration, _hiddenInventoryPosition.position, CurveType.EaseOutCubic);
        currentInventory.LootParent.UChangePosition(closeDuration, _hiddenInventoryPosition.position, CurveType.EaseOutCubic);

        _leftButton.UChangePosition(openDuration, _shownLeftButtonPosition.position + new Vector3(-3f, 0, 0), CurveType.EaseOutCubic);
        _rightButton.UChangePosition(openDuration, _shownRightButtonPosition.position + new Vector3(3f, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(closeDuration);

        currentInventory.RebootPosition();

        isOpenning = false;
    }

    public bool VerifyCanOpenOrCloseHeroInfos()
    {
        return !isOpenning;
    }

    #endregion


    #region Manage Equipment

    // Called on the hero info screen
    private void AddEquipment(Loot equipment, int slotIndex)
    {
        hero.AddEquipment(equipment, slotIndex);

        hero.ActualiseUnitInfos(0, 0, 0, 0, 0, 0);
    }

    // Called from inventory
    private void AddEquipment(Loot equipment, int slotIndex, Hero hero)
    {
        hero.AddEquipment(equipment, slotIndex);

        hero.ActualiseUnitInfos(0, 0, 0, 0, 0, 0);
    }

    // Called on the hero info screen
    private void RemoveEquipment(Loot equipment, int slotIndex)
    {
        hero.RemoveEquipment(equipment, slotIndex);

        hero.ActualiseUnitInfos(0, 0, 0, 0, 0, 0);
    }

    // Called from inventory
    private void RemoveEquipment(Loot equipment, int slotIndex, Hero hero)
    {
        hero.RemoveEquipment(equipment, slotIndex);

        hero.ActualiseUnitInfos(0, 0, 0, 0, 0, 0);
    }

    #endregion


    #region Drag Functions

    private void StartDrag()
    {
        Loot draggedLoot = UIManager.Instance.DraggedLoot;

        if (draggedLoot.LootData.lootType != LootType.Equipment) return;

        for(int i = 0; i < _equipmentSlots.Length; i++)
        {
            if (draggedLoot.LootData.equipmentType == _equipmentSlots[i].EquipmentType)
            {
                _equipmentSlots[i].HightlightSlot();
            }
            else
            {
                _equipmentSlots[i].HideSlot();
            }
        }
    }

    private void StopDrag()
    {
        for (int i = 0; i < _equipmentSlots.Length; i++)
        {
            _equipmentSlots[i].GetBackToNormal();
        }
    }

    #endregion


    #region Actualise Functions

    public EquipmentSlot GetAppropriateEquipmentSlot(EquipmentType type)
    {
        EquipmentSlot result = null;

        for (int i = 0; i < _equipmentSlots.Length; i++)
        {
            if (_equipmentSlots[i].EquipmentType == type && _equipmentSlots[i].EquipedLoot is null)
            {
                return _equipmentSlots[i];
            }
            else if(_equipmentSlots[i].EquipmentType == type)
            {
                result = _equipmentSlots[i];
            }
        }

        return result;
    }


    public void ActualiseInfoScreen(Hero hero, bool doEffect = true)
    {
        this.heroData = hero.HeroData;
        this.hero = hero;   

        int currentHealth = 0;
        int currentStrength = 0;
        int currentSpeed = 0;
        int currentLuck = 0;
        int currentMovePoints = 0;
        int currentMaxSP = 0;

        for (int i = 0; i < hero.EquippedLoot.Length; i++)
        {
            _equipmentSlots[i].RemoveEquipment(false, hero);

            if (hero.EquippedLoot[i] == null) continue;

            //currentHealth += hero.EquippedLoot[i].LootData.healthUpgrade;
            //currentStrength += hero.EquippedLoot[i].LootData.strengthUpgrade;
            //currentSpeed += hero.EquippedLoot[i].LootData.speedUpgrade;
            //currentLuck += hero.EquippedLoot[i].LootData.luckUpgrade;
            //currentMovePoints += hero.EquippedLoot[i].LootData.mpUpgrade;
            //currentMaxSP += hero.EquippedLoot[i].LootData.spUpgrade;

            _equipmentSlots[i].AddEquipment(hero.EquippedLoot[i], false, hero);
        }

        _heroName.text = heroData.unitName;

        if (doEffect) 
            VerifyValueChangeEffects(currentHealth, currentStrength, currentSpeed, currentLuck, currentMovePoints, currentMaxSP);

        _statsTexts[0].text = hero.CurrentHealth.ToString();
        _statsTexts[1].text = hero.CurrentStrength.ToString();
        _statsTexts[2].text = hero.CurrentSpeed.ToString();
        _statsTexts[3].text = hero.CurrentLuck.ToString();
        _statsTexts[4].text = hero.CurrentMovePoints.ToString();
        _statsTexts[5].text = hero.CurrentMaxSkillPoints.ToString();
    }



    private void VerifyValueChangeEffects(int newMaxHealth, int newStrength, int newSpeed, int newLuck, int newMovePoints, int maxSP)
    {
        if(newMaxHealth != int.Parse(_statsTexts[0].text)) StartCoroutine(ValueChangeEffectCoroutine(_statsTexts[0]));
        if(newStrength != int.Parse(_statsTexts[1].text)) StartCoroutine(ValueChangeEffectCoroutine(_statsTexts[1]));
        if(newSpeed != int.Parse(_statsTexts[2].text)) StartCoroutine(ValueChangeEffectCoroutine(_statsTexts[2]));
        if(newLuck != int.Parse(_statsTexts[3].text)) StartCoroutine(ValueChangeEffectCoroutine(_statsTexts[3]));
        if(newMovePoints != int.Parse(_statsTexts[4].text)) StartCoroutine(ValueChangeEffectCoroutine(_statsTexts[4]));
        if(maxSP != int.Parse(_statsTexts[5].text)) StartCoroutine(ValueChangeEffectCoroutine(_statsTexts[5]));
    }

    private IEnumerator ValueChangeEffectCoroutine(TextMeshProUGUI text)
    {
        text.DOColor(Color.white, 0.2f).SetEase(Ease.InOutCubic);
        text.rectTransform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.2f);

        text.DOColor(baseTextColor, 0.2f).SetEase(Ease.InOutCubic);
        text.rectTransform.DOScale(Vector3.one , 0.2f).SetEase(Ease.InOutCubic);
    }

    private void ActualiseInventory()
    {
        currentInventory.RebootPosition();

        currentInventory = hero.Inventory;

        currentInventory.RectTransform.position = _shownInventoryPosition.position;
        currentInventory.LootParent.position = _shownInventoryPosition.position;
    }
    
    #endregion
}
