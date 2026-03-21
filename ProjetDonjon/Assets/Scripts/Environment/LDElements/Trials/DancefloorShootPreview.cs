using DG.Tweening;
using System.Collections;
using UnityEngine;

public class DancefloorShootPreview : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color hiddenColor;
    [SerializeField] private Color shownColor;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void Start()
    {
        _spriteRenderer.color = hiddenColor;
    }


    public void DoPreview(float duration)
    {
        StartCoroutine(DoPreviewCoroutine(duration));
    }

    private IEnumerator DoPreviewCoroutine(float duration)
    {
        _spriteRenderer.DOColor(shownColor, duration);

        yield return new WaitForSeconds(duration);

        _spriteRenderer.DOColor(shownColor * 3f, 0.1f);

        yield return new WaitForSeconds(0.1f);

        _spriteRenderer.DOColor(hiddenColor, 0.4f);
    }
}
