using DG.Tweening;
using UnityEngine;

public class TransparentColldier : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float transparentFadeValue = 0.3f;

    [Header("Private Infos")]
    private int inTriggerCount = 0;

    [Header("References")]
    [SerializeField] private SpriteRenderer _linkedSpriteRenderer;


    private void FadeIn()
    {
        _linkedSpriteRenderer.DOFade(transparentFadeValue, 0.5f).SetEase(Ease.InOutSine);
    }

    private void FadeOut() 
    {
        _linkedSpriteRenderer.DOFade(1, 0.5f).SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        inTriggerCount++;

        if(inTriggerCount == 1)
        {
            FadeIn();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        inTriggerCount--;

        if(inTriggerCount == 0)
        {
            FadeOut();
        }
    }
}
