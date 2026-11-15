using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class SpikeGroup : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float turnedOffDuration;
    [SerializeField] private float turnedOnDuration;
    [SerializeField] private float startOffsetDuration;

    [Header("Private Infos")]
    private bool isActivated;
    private bool isColliderActivated;
    private float timer;

    [Header("References")]
    [SerializeField] private Spike[] _spikes;
    [SerializeField] private BoxCollider2D _groupCollider;



    private void Start()
    {
        timer = -startOffsetDuration;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(isActivated)
        {
            if(timer >= turnedOnDuration)
            {
                SetIsActivated(false);
                timer = 0;
            }
        }
        else
        {
            if (timer >= turnedOffDuration)
            {
                SetIsActivated(true);
                timer = 0;
            }
        }
    }

    private void SetIsActivated(bool isActivated)
    {
        this.isActivated = isActivated;

        foreach(Spike spike in _spikes)
        {
            spike.SetIsActivated(isActivated);
        }

        StartCoroutine(SetColliderStateWithDelayCoroutine(0.1f, isActivated));
    }

    private IEnumerator SetColliderStateWithDelayCoroutine(float delay, bool activated)
    {
        yield return new WaitForSeconds(delay);

        isColliderActivated = activated;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isColliderActivated) return;
        if (!collision.CompareTag("Hero")) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Jump) return;

        HeroesManager.Instance.TakeDamage(1);
    }
}
