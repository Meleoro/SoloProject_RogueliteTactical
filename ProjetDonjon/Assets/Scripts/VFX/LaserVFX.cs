using DG.Tweening;
using System.Collections;
using UnityEngine;

public class LaserVFX : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float chargeDuration;
    [SerializeField] private float delayBetweenChargeAndPlay;
    [SerializeField] private float laserPlayDuration;
    [SerializeField] private Vector2 laserEndOffset;

    [Header("References")]
    [SerializeField] private ParticleSystem[] _laserPreparingVFXs;
    [SerializeField] private ParticleSystem[] _laserPlayingVFXs;
    [SerializeField] private Transform _endBeamTransform;
    [SerializeField] private LineRenderer _laserLineRenderer;


    public void StartLaserVFX(Vector2 middle, Vector2 end)
    {
        _laserLineRenderer.SetPosition(0, (Vector2)transform.position);
        _laserLineRenderer.SetPosition(1, end + laserEndOffset);
        _laserLineRenderer.enabled = false;

        foreach (ParticleSystem VFX in _laserPreparingVFXs)
        {
            VFX.Play();
        }

        _laserLineRenderer.enabled = false;
        foreach (ParticleSystem VFX in _laserPlayingVFXs)
        {
            VFX.Stop();
        }

        StartCoroutine(DoLaserVFXCoroutine());
    }

    private IEnumerator DoLaserVFXCoroutine()
    {
        yield return new WaitForSeconds(chargeDuration);

        foreach (ParticleSystem VFX in _laserPreparingVFXs)
        {
            VFX.Stop();
        }

        yield return new WaitForSeconds(delayBetweenChargeAndPlay);

        _laserLineRenderer.enabled = true;
        foreach (ParticleSystem VFX in _laserPlayingVFXs)
        {
            VFX.Play();
        }

        yield return new WaitForSeconds(laserPlayDuration);

        _laserLineRenderer.enabled = false;
        foreach (ParticleSystem VFX in _laserPlayingVFXs)
        {
            VFX.Stop();
        }
    }
}
