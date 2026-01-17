using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class Relic : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private Color[] colorAccordingToRarity;

    [Header("Private Infos")]
    private RelicData relicData;
    private bool hovered;
    private bool isInSpawnAnim;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _secondaryText;
    [SerializeField] private SpriteRenderer _rays1SpriteRenderer;
    [SerializeField] private SpriteRenderer _rays2SpriteRenderer;
    [SerializeField] private Animator _animator;


    public void Initialise(RelicData relicData)
    {
        this.relicData = relicData;

        _nameText.text = relicData.relicName;
        _spriteRenderer.sprite = relicData.icon;

        StartCoroutine(DoSpawnAnim());
    }
    
    private IEnumerator DoSpawnAnim()
    {
        isInSpawnAnim = true;

        yield return new WaitForSeconds(0.25f / 0.6f);

        isInSpawnAnim = false;
        if(hovered) CanBePicked();
    }


    #region Interface Functions

    public void CanBePicked()
    {
        hovered = true;
        if (isInSpawnAnim) return;

        transform.DOScale(Vector3.one * 1.1f, 0.1f);
        _spriteRenderer.material.DOFloat(1f, "_OutlineSize", 0.1f);
        _rays1SpriteRenderer.material.DOFloat(0.5f, "_MaxDistance", 0.1f);
        _rays2SpriteRenderer.material.DOFloat(0.4f, "_MaxDistance", 0.1f);

        _nameText.enabled = true;
        _nameText.text = relicData.relicName;
        _secondaryText.enabled = true;
        _secondaryText.text = relicData.rarityType.ToString().ToUpper() + " RELIC";

        _secondaryText.color = colorAccordingToRarity[(int)relicData.rarityType];
    }

    public void CannotBePicked()
    {
        hovered = false;
        if (isInSpawnAnim) return;

        transform.DOScale(Vector3.one, 0.1f);
        _spriteRenderer.material.DOFloat(0f, "_OutlineSize", 0.1f);
        _rays1SpriteRenderer.material.DOFloat(0.4f, "_MaxDistance", 0.1f);
        _rays2SpriteRenderer.material.DOFloat(0.3f, "_MaxDistance", 0.1f);

        _nameText.enabled = false;
        _secondaryText.enabled = false;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        RelicsManager.Instance.ObtainNewRelic(relicData);

        Destroy(gameObject);
    }

    #endregion
}
