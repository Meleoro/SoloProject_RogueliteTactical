using System;
using System.Collections;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;


[Serializable] public struct DancefloorTrapLine 
{ 
    public Transform[] ArrowDispensers; 
    public DancefloorShootPreview LinePreview; 
} 

public class DancefloorTrial : Trial
{
    [Header("Parameters")]
    [SerializeField] private float startFrequency;
    [SerializeField] private float endFrequency;
    [SerializeField] private float duration;
    [SerializeField] private float trapsPreviewDuration;
    [SerializeField] private float trapsCooldowns;
    [SerializeField] private float enableTwoPerTriggerTime;

    [Header("Arrow Parameters")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private int arrowDamages;

    [Header("Private Infos")]
    private bool[] _lineIsActive;

    [Header("References")]
    [SerializeField] private DancefloorTrapLine[] _lines;
    [SerializeField] private BoxCollider2D _exitCollider;


    public override void StartTrial()
    {
        _lineIsActive = new bool[_lines.Length];
        _exitCollider.enabled = true;

        StartCoroutine(DoDanceFloorTrialCoroutine());
    }

    public override void EndTrial()
    {
        _exitCollider.enabled = false;

        OnTrialEnd?.Invoke();
    }


    private IEnumerator DoDanceFloorTrialCoroutine()
    {
        float timer = duration;
        float triggerTimer = startFrequency;

        while(timer > 0)
        {
            timer -= Time.deltaTime;
            triggerTimer -= Time.deltaTime;

            if(triggerTimer < 0)
            {
                triggerTimer = endFrequency + (startFrequency - endFrequency) * (timer / duration);
                TriggerTrap(timer < enableTwoPerTriggerTime);
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1.0f);

        EndTrial();
    }

    private void TriggerTrap(bool doubleTrap)
    {
        int pickedIndex = GetTriggerLineIndex();
        if (pickedIndex == -1) return;   // No trap available

        _lineIsActive[pickedIndex] = true;
        StartCoroutine(TriggerTrapCoroutine(pickedIndex));
    }

    private IEnumerator TriggerTrapCoroutine(int index)
    {
        _lines[index].LinePreview.DoPreview(trapsPreviewDuration);

        yield return new WaitForSeconds(trapsPreviewDuration);

        for(int i = 0; i < _lines[index].ArrowDispensers.Length; i++)
        {
            Arrow newArrow = Instantiate(arrowPrefab, _lines[index].ArrowDispensers[i].position, _lines[index].ArrowDispensers[i].rotation);
            newArrow.InitialiseArrow(arrowSpeed, arrowDamages);

            AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 1, 7, 0);
        }

        yield return new WaitForSeconds(trapsCooldowns);

        _lineIsActive[index] = false;
    }

    private int GetTriggerLineIndex()
    {
        int antiCrash = 0;

        while(antiCrash++ < 500)
        {
            int randomIndex = Random.Range(0, _lineIsActive.Length);
            if (_lineIsActive[randomIndex]) continue;

            return randomIndex;
        }

        return -1;
    }
}
