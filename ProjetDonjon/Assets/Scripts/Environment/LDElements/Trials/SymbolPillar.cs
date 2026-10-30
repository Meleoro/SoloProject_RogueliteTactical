using DG.Tweening;
using System;
using UnityEngine;

public class SymbolPillar : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    [Header("Private Infos")]
    private int categoryIndex;
    private int symbolIndex;
    private bool canBeInteractedWith;

    [Header("Actions")]
    public Action<int, int> OnInteract;

    [Header("Public Infos")]
    public SpriteRenderer SymbolSpriteRenderer { get { return _symbolSpriteRenderer; } }

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _symbolSpriteRenderer;


    public void SetupPillar(int categoryIndex, int symbolIndex)
    {
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));

        this.categoryIndex = categoryIndex;
        this.symbolIndex = symbolIndex;

        canBeInteractedWith = true;
    }

    public void ActualisePillarVisuals(bool isValid)
    {
        _symbolSpriteRenderer.color = isValid ? validColor : invalidColor;
    }

    public void DisablePillar()
    {
        canBeInteractedWith = false;
        CannotBePicked();
    }


    #region Interface Functions

    public void CanBePicked()
    {
        if (!canBeInteractedWith) return;
        _spriteRenderer.material.DOFloat(1f, "_OutlineSize", 0.1f);
    }

    public void CannotBePicked()
    {
        _spriteRenderer.material.DOFloat(0f, "_OutlineSize", 0.1f);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        if (!canBeInteractedWith) return;

        CannotBePicked();
        canBeInteractedWith = false;

        OnInteract.Invoke(categoryIndex, symbolIndex);
    }

    #endregion
}
