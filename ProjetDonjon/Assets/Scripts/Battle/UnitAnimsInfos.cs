using UnityEngine;
using Utilities;

public class UnitAnimsInfos : MonoBehaviour
{
    [Header("Play Sound Parameters")]
    [SerializeField] private int soundCategoryIndex;
    [SerializeField] private int minSoundIndex;
    [SerializeField] private int maxSoundIndex;

    [Header("Public Infos")]
    public bool PlaySkillEffect;
    public bool PlayVFX;
    public bool PlayCritVFX;
    public bool PlaySound;

    [Header("Private Infos")]
    private bool isPlayingVFX;
    private bool isPlayingCritVFX;
    private bool isPlayingSound;

    [Header("References")]
    [SerializeField] private ParticleSystem _vfxToPlay;
    [SerializeField] private Animator _critVFXAnim;



    private void Update()
    {
        if(!isPlayingVFX && PlayVFX)
        {
            isPlayingVFX = true;
            _vfxToPlay.Play();
        }
        else if(isPlayingVFX && !PlayVFX)
        {
            _vfxToPlay.Stop();
            isPlayingVFX = false; 
        }

        if (PlayCritVFX && !isPlayingCritVFX)
        {
            isPlayingCritVFX = true;
            _critVFXAnim.SetTrigger("PlayVFX");

            AudioManager.Instance.PlaySoundOneShot(2, 16);
        }
        else if(!PlayCritVFX && isPlayingCritVFX)
        {
            isPlayingCritVFX = false;
        }

        if(PlaySound && !isPlayingSound)
        {
            isPlayingSound = true;
            AudioManager.Instance.PlaySoundOneShot(soundCategoryIndex, Random.Range(minSoundIndex, maxSoundIndex + 1));
        }
        else if(!PlaySound && isPlayingSound)
        {
            isPlayingSound = false;
        }
    }
}
