using DG.Tweening;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class HUDExploration : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float overlayEffectDuration;

    [Header("Public Infos")]
    public Image EquipmentMenuImage { get { return _buttonsImages[1]; } }
    public Image SkillsMenu { get { return _buttonsImages[2]; } }
    public Image SkillTreeMenu { get { return _buttonsImages[3]; } }
    public Animator Animator { get { return _animator; } }
    public bool IsDisplayed { get { return isDisplayed; } }

    [Header("Private Infos")]
    private bool isDisplayed;
    private bool isBouncing;
    private Coroutine bounceCoroutine;

    [Header("References")]
    [SerializeField] private Image[] _buttonsImages;
    [SerializeField] private RectTransform[] _buttonsParent;
    [SerializeField] private Animator _animator;
    [SerializeField] private InventoriesManager _inventoryManager;
    [SerializeField] private HeroInfosScreen _heroInfosScreen;
    [SerializeField] private SkillTreeManager _skillTreeScreen;
    [SerializeField] private SkillsMenu _skillsMenu;
    [SerializeField] private CollectionMenu _collectionMenu;
    [SerializeField] private Minimap _minimap;


    private void Start()
    {
        _heroInfosScreen.OnShow += Hide;
        _heroInfosScreen.OnHide += Show;

        _inventoryManager.OnInventoryOpen += Hide;
        _inventoryManager.OnInventoryClose += Show;

        _skillTreeScreen.OnShow += Hide;
        _skillTreeScreen.OnHide += Show;

        BattleManager.Instance.OnBattleStart += HideEnterBattle;
        BattleManager.Instance.OnBattleEnd += Show;

        _skillsMenu.OnShow += Hide;
        _skillsMenu.OnHide += Show;

        _collectionMenu.OnShow += Hide;
        _collectionMenu.OnHide += Show;

        UIManager.Instance.OnExplorationStart += ShowWithDelay;
        UIManager.Instance.Transition.OnTransitionStart += Hide;
        UIManager.Instance.Transition.OnTransitionEnd += Show;

        HeroesManager.Instance.OnHeroLevelUp += DoBounceSkillTree;

        UIMetaManager.Instance.MainMetaMenu.OnShow += Show;
        UIMetaManager.Instance.MainMetaMenu.OnHide += Hide;

        foreach (var button in _buttonsImages)
        {
            Material material = button.material;
            Material material2 = new Material(material);

            button.material = material2;
        }
    }


    #region Show

    public void Show()
    {
        if (BattleManager.Instance.IsInBattle) return;
        if (isDisplayed) return;

        isDisplayed = true;
        _animator.SetBool("IsDisplayed", true);

        if(UIManager.Instance.Transition.IsActive || !GameManager.Instance.IsInExplo)
            _minimap.gameObject.SetActive(false);

        else 
            _minimap.gameObject.SetActive(true);

        ActualiseDisplayedButtons();
    }

    private void ShowWithDelay()
    {
        StartCoroutine(ShowWithDelayCoroutine());
    }

    private IEnumerator ShowWithDelayCoroutine()
    {
        yield return new WaitForSeconds(2.1f);

        Show();
    }

    #endregion


    #region Hide

    public void Hide()
    {
        if (!isDisplayed) return;

        isDisplayed = false;
        _animator.SetBool("IsDisplayed", false);

        UIManager.Instance.HideHeroInfosPanels();
    }

    private void HideEnterBattle()
    {
        if (!isDisplayed) return;

        isDisplayed = false;
        _animator.SetBool("IsDisplayed", false);
    }

    #endregion


    #region Buttons Functions

    public void OverlayButton(int index)
    {
        _buttonsImages[index].rectTransform.DOComplete();

        _buttonsImages[index].material.ULerpMaterialFloat(overlayEffectDuration, 1, "_OutlineSize");
        _buttonsImages[index].rectTransform.DOScale(Vector3.one * 1.15f, overlayEffectDuration).SetEase(Ease.OutBack);

        if (isBouncing && index == 3)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }
    }

    public void QuitOverlayButton(int index)
    {
        _buttonsImages[index].rectTransform.DOComplete();

        _buttonsImages[index].material.ULerpMaterialFloat(overlayEffectDuration, 0, "_OutlineSize");
        _buttonsImages[index].rectTransform.DOScale(Vector3.one, overlayEffectDuration).SetEase(Ease.OutBack);

        if (isBouncing && index == 3)
        {
            _buttonsImages[index].rectTransform.DOComplete();

            DoBounceSkillTree();
        }
    }

    public void ClickInventory()
    {
        if (!isDisplayed) return;

        _inventoryManager.ShowInventories();
    }

    public void ClickEquipment()
    {
        if (!isDisplayed) return;

        StartCoroutine(_heroInfosScreen.OpenInfosScreenCoroutine());
    }

    public void ClickSkillTree()
    {
        if (!isDisplayed) return;

        _skillTreeScreen.Show();

        if (isBouncing)
        {
            isBouncing = false;
        }
    }

    public void ClickSkills()
    {
        if (!isDisplayed) return;

        _skillsMenu.Show();
    }

    public void ClickCollection()
    {
        if (!isDisplayed) return;

        _collectionMenu.Show();
    }

    #endregion


    #region Others

    public void ActualiseDisplayedButtons()
    {
        //if(TutoManager.Instance.TutoDisabled)
        //    return;

        _buttonsParent[0].gameObject.SetActive(TutoManager.Instance.DidTutorialStep[3]);
        _buttonsParent[1].gameObject.SetActive(TutoManager.Instance.DidAdditionalTutorialStep[2]);
        _buttonsParent[2].gameObject.SetActive(TutoManager.Instance.DidAdditionalTutorialStep[3]);
        _buttonsParent[3].gameObject.SetActive(TutoManager.Instance.DidAdditionalTutorialStep[4]);
        _buttonsParent[4].gameObject.SetActive(TutoManager.Instance.DidTutorialStep[9]);
    }

    private void DoBounceSkillTree()
    {
        if (bounceCoroutine != null) return;

        isBouncing = true;
        bounceCoroutine = StartCoroutine(DoBounceSkillTreeCoroutine());
    }

    private IEnumerator DoBounceSkillTreeCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            _buttonsImages[3].rectTransform.DOScale(Vector3.one * 1.15f, 0.5f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(0.6f);

            _buttonsImages[3].rectTransform.DOScale(Vector3.one * 1f, 0.5f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(0.6f);
        }
    }

    #endregion
}
