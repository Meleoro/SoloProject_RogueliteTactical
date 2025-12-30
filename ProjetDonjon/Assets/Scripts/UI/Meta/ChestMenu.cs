using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ChestMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float distanceBetweenSlots;

    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Public Infos")]
    public Inventory ChestInventory { get {  return _chestInventory; } }

    [Header("Private Infos")]
    private Hero currentHero;
    private Inventory currentInventory;

    [Header("References")]
    [SerializeField] private MainMetaMenu _mainMetaMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private RectTransform _middlePosRectTr;
    [SerializeField] private RectTransform _rightPosRectTr;
    [SerializeField] private RectTransform _inventoryParent;
    [SerializeField] private RectTransform _chestSlotsParent;
    [SerializeField] private RectTransform _lootParent;
    [SerializeField] private Inventory _chestInventory;


    private void Start()
    {
        _chestInventory.SetupPosition(null, null, _lootParent);
        _chestInventory.InitialiseInventory(null);
    }


    private void LoadInventory()
    {
        if (currentInventory is not null)
            currentInventory.RebootPosition();

        currentInventory = currentHero.Inventory;

        currentInventory.RectTransform.SetParent(_inventoryParent);
        currentInventory.RectTransform.localPosition = Vector3.zero;

        currentInventory.LootParent.SetParent(_inventoryParent);
        currentInventory.LootParent.localPosition = Vector3.zero;
    }


    #region Show / Hide

    public void Show()
    {
        OnStartTransition.Invoke();
        OnShow.Invoke();

        currentHero = HeroesManager.Instance.AllHeroes[0];
        LoadInventory();

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        yield return new WaitForSeconds(0.3f);

        _mainRectTr.DOMove(_middlePosRectTr.position + Vector3.left * 0.4f, 0.3f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.3f);

        _mainRectTr.DOMove(_middlePosRectTr.position, 0.3f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.3f);

        OnEndTransition.Invoke();
    }

    public void Hide()
    {
        OnStartTransition.Invoke();
        OnHide.Invoke();

        _mainMetaMenu.Show(false);

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.DOMove(_middlePosRectTr.position + Vector3.left * 0.4f, 0.3f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.3f);

        _mainRectTr.DOMove(_rightPosRectTr.position, 0.3f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.6f);

        OnEndTransition.Invoke();
    }

    #endregion
}
