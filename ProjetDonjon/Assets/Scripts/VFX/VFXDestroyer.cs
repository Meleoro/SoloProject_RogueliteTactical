using UnityEngine;
using Utilities;

public class VFXDestroyer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int soundIDToPlay = -1;
    [SerializeField] private float timeBeforeDestroy;


    private void Start()
    {
        Destroy(gameObject, timeBeforeDestroy);

        if(soundIDToPlay != -1)
            AudioManager.Instance.PlaySoundOneShot(2, soundIDToPlay);
    }
}
