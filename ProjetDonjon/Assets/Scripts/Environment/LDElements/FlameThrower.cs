using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

public class FlameThrower : MonoBehaviour
{
    [Header("Parameters Still")]
    [SerializeField] private float stillTurnedOnDuration = 1.0f;
    [SerializeField] private float stillTurnedOffDuration = 1.0f;

    [Header("Parameters Movable")]
    [SerializeField] private float waypointWaitDuration = 1.0f;
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private bool turnOffAtWaypoint = true;

    [Header("Private Infos")]
    private float timer = 0;
    private bool isMoving;
    private bool isTurnedOn;
    private bool isStill;
    private bool isGoingForward = true;
    private int nextWaypoint = 1;
    private int previousWaypoint = 0;
    private Vector3[] saveWaypointsPositions;
    private Coroutine currentChangeLightCoroutine;

    [Header("References")]
    [SerializeField] private Transform[] _waypoints;  // If no waypoints, is still
    [SerializeField] private ParticleSystem _fireVFX;  
    [SerializeField] private Light2D _light;  
    [SerializeField] private BoxCollider2D _collider;  
    [SerializeField] private AudioSource _audioSource;  


    private void Start()
    {
        saveWaypointsPositions = new Vector3[_waypoints.Length];
        for (int i = 0; i < _waypoints.Length; i++)
        {
            saveWaypointsPositions[i] = _waypoints[i].transform.position;
        }

        isStill = _waypoints.Length < 2;
        SetIsTurnedOn(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(isStill)
        {
            ManageStill(timer);
        }
        else
        {
            ManageMovement(timer);
        }
    }

    private void ManageStill(float currentTimer)
    {
        if(isTurnedOn)
        {
            if (timer >= stillTurnedOnDuration)
            {
                SetIsTurnedOn(false);
                timer = 0;
            }
        }
        else
        {
            if (timer >= stillTurnedOffDuration)
            {
                SetIsTurnedOn(true);
                timer = 0;
            }
        }
    }

    private void ManageMovement(float currentTimer)
    {
        if(isMoving)
        {
            transform.position = Vector3.Lerp(saveWaypointsPositions[previousWaypoint], saveWaypointsPositions[nextWaypoint], timer / moveDuration);

            if (timer < moveDuration) return;
            
            isMoving = false;
            transform.position = saveWaypointsPositions[nextWaypoint];

            timer = 0;
            SetIsTurnedOn(!turnOffAtWaypoint);

            previousWaypoint = nextWaypoint;
            if (isGoingForward)
            {
                if(nextWaypoint + 1 >= _waypoints.Length)  // We need to reverse
                {
                    isGoingForward = !isGoingForward;
                    nextWaypoint--;
                }
                else    // We continue forward
                {
                    nextWaypoint++;
                }
            }
            else
            {
                if (nextWaypoint - 1 < 0)  // We need to reverse
                {
                    isGoingForward = !isGoingForward;
                    nextWaypoint++;
                }
                else    // We continue forward
                {
                    nextWaypoint--;
                }
            }
        }
        else
        {
            if (timer > waypointWaitDuration)
            {
                timer = 0;
                isMoving = true;

                SetIsTurnedOn(true);
            }
        }
    }

    private void SetIsTurnedOn(bool turnedOn)
    {
        if (isTurnedOn == turnedOn) return;
        isTurnedOn = turnedOn;

        if(currentChangeLightCoroutine != null)
        {
            StopCoroutine(currentChangeLightCoroutine);
        }

        if (isTurnedOn)
        {
            _fireVFX.Play();
            currentChangeLightCoroutine = StartCoroutine(ChangeLightIntensityCoroutine(15));
            StartCoroutine(EnableColliderCoroutine());

            AudioManager.Instance.PlaySoundContinuous(1, 9, 0, _audioSource);
        }
        else
        {
            _fireVFX.Stop();
            currentChangeLightCoroutine = StartCoroutine(ChangeLightIntensityCoroutine(0));
            _audioSource.Stop();

            _collider.enabled = false;
        }
    }

    private IEnumerator ChangeLightIntensityCoroutine(float finalValue)
    {
        float lightTimer = 0;
        float saveStartValue = _light.intensity;

        if (_light.intensity == finalValue) yield break;

        while(lightTimer < 0.4f)
        {
            lightTimer += Time.deltaTime;

            _light.intensity = Mathf.Lerp(saveStartValue, finalValue, lightTimer / 0.4f);

            yield return new WaitForSeconds(Time.deltaTime);
        }

        _light.intensity = finalValue;
    }

    private IEnumerator EnableColliderCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        _collider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Hero")) return;

        HeroesManager.Instance.TakeDamage(1);
    }
}
