using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class HUDExploration : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float overlayEffectDuration;

    [Header("Private Infos")]
    private bool isDisplayed;

    [Header("References")]
    [SerializeField] private Image[] _buttonsImages;
    [SerializeField] private Animator _animator;
    [SerializeField] private InventoriesManager _inventoryManager;
    [SerializeField] private HeroInfosScreen _heroInfosScreen;
    [SerializeField] private SkillTreeManager _skillTreeScreen;
    [SerializeField] private SkillsMenu _skillsMenu;


    private void Start()
    {
        _heroInfosScreen.OnShow += Hide;
        _heroInfosScreen.OnHide += Show;

        _inventoryManager.OnInventoryOpen += Hide;
        _inventoryManager.OnInventoryClose += Show;

        _skillTreeScreen.OnShow += Hide;
        _skillTreeScreen.OnHide += Show;

        BattleManager.Instance.OnBattleStart += Hide;
        BattleManager.Instance.OnBattleEnd += Show;

        _skillsMenu.OnShow += Hide;
        _skillsMenu.OnHide += Show;

        foreach (var button in _buttonsImages)
        {
            Material material = button.material;
            Material material2 = new Material(material);

            button.material = material2;
        }
    }


    private void Show()
    {
        if (BattleManager.Instance.IsInBattle) return;
        if (isDisplayed) return;
        isDisplayed = true;

        _animator.SetBool("IsDisplayed", true);
    }

    private void Hide()
    {
        if (!isDisplayed) return;
        isDisplayed = false;

        _animator.SetBool("IsDisplayed", false);
    }


    #region Buttons Functions

    public void OverlayButton(int index)
    {
        _buttonsImages[index].material.ULerpMaterialFloat(overlayEffectDuration, 1, "_OutlineSize");
        _buttonsImages[index].rectTransform.UChangeScale(overlayEffectDuration, Vector3.one * 1.1f, CurveType.EaseOutCubic);
    }

    public void QuitOverlayButton(int index)
    {
        _buttonsImages[index].material.ULerpMaterialFloat(overlayEffectDuration, 0, "_OutlineSize");
        _buttonsImages[index].rectTransform.UChangeScale(overlayEffectDuration, Vector3.one * 1f, CurveType.EaseOutCubic);
    }

    public void ClickInventory()
    {
        if (!isDisplayed) return;

        _inventoryManager.OpenInventories();
    }

    public void ClickEquipment()
    {
        if (!isDisplayed) return;

        StartCoroutine(_heroInfosScreen.OpenInfosScreenCoroutine());
    }

    public void ClickSkill()
    {
        if (!isDisplayed) return;

        _skillTreeScreen.Show();
    }

    public void ClickSkillTree()
    {
        if (!isDisplayed) return;

        _skillsMenu.Show();
    }

    #endregion
}
