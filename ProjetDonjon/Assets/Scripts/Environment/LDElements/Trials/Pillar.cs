using DG.Tweening;
using System;
using UnityEngine;

public class Pillar : MonoBehaviour, IInteractible
{
    [Header("Actions")]
    public Action OnChangeState;

    [Header("Public Infos")]
    public bool IsOn { get { return isOn; } }

    [Header("Private Infos")]
    private bool isActivated = true;
    private bool isOn;
    private Pillar[] neighborPillars;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _turnedOnSpriteRenderer;



    public void Initialise(bool isOn, Pillar[] neighbors)
    {
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));

        neighborPillars = neighbors;
        SetIsOn(isOn, false);
    }

    public void SetIsOn(bool isOn, bool fromPlayer)
    {
        this.isOn = isOn;
        _turnedOnSpriteRenderer.enabled = isOn;

        if (!fromPlayer) return;

        foreach(Pillar neighbor in neighborPillars)
        {
            neighbor.SetIsOn(!neighbor.isOn, false);
        }

        OnChangeState?.Invoke();   // Verifies if the puzzle is solved
    }

    // Called at the end of the puzzle
    public void DeactivatePuzzle()
    {
        isActivated = false;

        CannotBePicked();
    }


    #region Interactible Interface

    public void CanBePicked()
    {
        if (!isActivated) return;
        _spriteRenderer.material.DOFloat(1f, "_OutlineSize", 0.1f).SetEase(Ease.OutBack);
    }

    public void CannotBePicked()
    {
        _spriteRenderer.material.DOFloat(0f, "_OutlineSize", 0.1f).SetEase(Ease.OutBack);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        if (!isActivated) return;

        SetIsOn(!isOn, true);
    }

    #endregion
}
