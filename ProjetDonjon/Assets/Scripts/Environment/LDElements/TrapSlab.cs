using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

public class TrapSlab : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private ParticleSystem dustVFXPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private int arrowDamages;
    [SerializeField] private float cooldownDuration;
    [SerializeField] private float activatePosOffset;
    
    [Header("Private Infos")]
    private bool isOnCooldown;
    private bool isDeactivated;
    private Vector3 originalPos;

    [Header("References")]
    [SerializeField] Transform[] _triggeredArrowDispensers;
    [SerializeField] Light2D _highlightLight;
    [SerializeField] AudioSource _audioSource;
    private ParticleSystem[] _dustParticleSystems;



    private void Start()
    {
        _dustParticleSystems = new ParticleSystem[_triggeredArrowDispensers.Length];

        originalPos = transform.position;

        for (int i = 0; i < _triggeredArrowDispensers.Length; i++)
        {
            _dustParticleSystems[i] = 
                Instantiate(dustVFXPrefab, _triggeredArrowDispensers[i].transform.position, _triggeredArrowDispensers[i].transform.rotation, transform);
        }
    }


    private IEnumerator TriggerTrapCoroutine()
    {
        isOnCooldown = true;
        transform.UChangePosition(0.2f, originalPos + Vector3.down * activatePosOffset, CurveType.EaseOutCubic);

        AudioManager.Instance.PlaySoundOneShot(1, 6, 0, _audioSource);

        for(int i = 0; i < _triggeredArrowDispensers.Length; i++)
        {
            if (isDeactivated) break;

            StartCoroutine(LaunchArrowCoroutine(Random.Range(0.0f, 0.1f), _triggeredArrowDispensers[i], _dustParticleSystems[i]));
        }

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
        transform.UChangePosition(0.2f, originalPos, CurveType.EaseOutCubic);
    }

    private IEnumerator LaunchArrowCoroutine(float delay, Transform dispenserTr, ParticleSystem dustVFX)
    {
        yield return new WaitForSeconds(delay);

        Arrow newArrow = Instantiate(arrowPrefab, dispenserTr.position, dispenserTr.rotation);
        newArrow.InitialiseArrow(arrowSpeed, arrowDamages);

        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 1, 7, 0);

        dustVFX.Play();
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isOnCooldown) return;
        if (!collision.CompareTag("Hero")) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Jump) return;

        StartCoroutine(TriggerTrapCoroutine());
    }


    #region Checkboard Room

    public void Highlight()
    {
        _highlightLight.ULerpIntensity(0.2f, 1.5f);
    }

    public void StopHighlight()
    {
        _highlightLight.ULerpIntensity(0.2f, 0);
    }

    public void Deactivate()
    {
        isDeactivated = true;
    }

    #endregion
}
