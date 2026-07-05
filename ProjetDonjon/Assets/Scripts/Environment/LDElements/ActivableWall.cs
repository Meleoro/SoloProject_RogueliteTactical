using UnityEngine;

public class ActivableWall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private ParticleSystem _groundParticles;




    public void ActivateWall()
    {
        _anim.SetBool("IsActivated", true);
        _collider.enabled = true;
    }

    public void DeactivateWall()
    {
        _anim.SetBool("IsActivated", false);
        _collider.enabled = false;
    }
}
