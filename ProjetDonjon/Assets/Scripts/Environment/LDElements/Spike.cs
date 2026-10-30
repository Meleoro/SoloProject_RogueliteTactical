using DG.Tweening;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float activatedLocalY;
    [SerializeField] private float deactivatedLocalY;
    [SerializeField] private float moveDuration;
    [SerializeField] private float maskLocalYSave;
    [SerializeField] private float maskScaleSaveY;

    [Header("Private Infos")]
    private bool isActivated;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _maskTransform;


    private void Start()
    {
        _spriteRenderer.transform.localPosition = Vector3.zero + Vector3.down * deactivatedLocalY;
    }

    public void SetIsActivated(bool activivated)
    {
        if(isActivated == activivated)
        {
            return;
        }

        isActivated = activivated;

        if(isActivated)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    private void Activate()
    {
        _spriteRenderer.transform.DOLocalMoveY(activatedLocalY, moveDuration).SetEase(Ease.OutBack);

        _maskTransform.DOLocalMoveY(maskLocalYSave, moveDuration).SetEase(Ease.OutBack);
        _maskTransform.DOScaleY(maskScaleSaveY, moveDuration).SetEase(Ease.OutBack);
    }

    private void Deactivate()
    {
        _spriteRenderer.transform.DOLocalMoveY(deactivatedLocalY, moveDuration).SetEase(Ease.OutBack);

        _maskTransform.DOLocalMoveY(0, moveDuration).SetEase(Ease.OutBack);
        _maskTransform.DOScaleY(0, moveDuration).SetEase(Ease.OutBack);
    }
}
