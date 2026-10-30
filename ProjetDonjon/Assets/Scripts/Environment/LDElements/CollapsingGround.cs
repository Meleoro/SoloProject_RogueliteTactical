using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CollapsingGround : MonoBehaviour
{
    private enum CollapsingGroundState
    {
        Normal,
        Collasping,
        Collapsed,
        Reappearing
    }

    [Header("Parameters")]
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeStrength;
    [SerializeField] private float timeBeforeRebuild;
    [SerializeField] private float rebuildDuration;

    [Header("Private Infos")]
    private CollapsingGroundState currentState;

    [Header("References")]
    [SerializeField] private SpriteRenderer _mainSprite;
    [SerializeField] private Animator _animator;


    private IEnumerator GroundCollapseCoroutine()
    {
        currentState = CollapsingGroundState.Collasping;
        transform.DOShakePosition(shakeDuration, shakeStrength, 30);

        yield return new WaitForSeconds(shakeDuration);

        currentState = CollapsingGroundState.Collapsed;
        _animator.SetBool("IsCollapsed", true);

        yield return new WaitForSeconds(timeBeforeRebuild);

        currentState = CollapsingGroundState.Reappearing;
        _animator.SetBool("IsCollapsed", false);

        yield return new WaitForSeconds(rebuildDuration);

        currentState = CollapsingGroundState.Normal;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Hero")) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Jump) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Fall) return;
        if (BattleManager.Instance.IsInBattle) return;

        if (collision.CompareTag("Hero"))
        {
            if (currentState == CollapsingGroundState.Normal)
            {
                StartCoroutine(GroundCollapseCoroutine());
            }
            else if(currentState == CollapsingGroundState.Collapsed)
            {
                StartCoroutine(collision.GetComponent<HeroController>().FallCoroutine());
            }
        }
    }
}
