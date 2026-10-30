using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MemoryPillar : MonoBehaviour, IInteractible
{
    [Header("Actions")]
    public Action<int> OnInteraction;    
 
    [Header("Private Infos")]
    private bool canBeInteractedWith;
    private int pillarIndex;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _goodSpriteRenderer;
    [SerializeField] private SpriteRenderer _wrongSpriteRenderer;
    [SerializeField] private Light2D _goodLight;
    [SerializeField] private Light2D _wrongLight;



    public void SetupPillar(int pillarIndex)
    {
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
        this.pillarIndex = pillarIndex;

        canBeInteractedWith = false;

        _goodSpriteRenderer.enabled = false;
        _wrongSpriteRenderer.enabled = false;
    }


    public void DisableInteractions()
    {
        canBeInteractedWith = false;
        CannotBePicked();
    }

    public void EnableInteractions()
    {
        canBeInteractedWith = true;
    }

    public void DoFlashPillar(float flashDuration, bool positiveFlash)
    {
        if(positiveFlash)
        {
            StartCoroutine(DoPositiveFlashCoroutine(flashDuration));
        }
        else
        {
            StartCoroutine(DoNegativeFlashCoroutine(flashDuration));
        }
    }

    private IEnumerator DoPositiveFlashCoroutine(float flashDuration)
    {
        _goodSpriteRenderer.enabled = true;
        _goodLight.enabled = true;

        yield return new WaitForSeconds(flashDuration);

        _goodSpriteRenderer.enabled = false;
        _goodLight.enabled = false;
    }

    private IEnumerator DoNegativeFlashCoroutine(float flashDuration)
    {
        _wrongSpriteRenderer.enabled = true;
        _wrongLight.enabled = true;

        yield return new WaitForSeconds(flashDuration);

        _wrongSpriteRenderer.enabled = false;
        _wrongLight.enabled = false;
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

        OnInteraction.Invoke(pillarIndex);
    }

    #endregion
}
