using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RayLight : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 0.8f;
    [SerializeField] private float changeIntensityDurationMin = 2.5f;
    [SerializeField] private float changeIntensityDurationMax = 3.5f;

    [Header("References")]
    [SerializeField] private Light2D _rayLight;


    private void Start()
    {
        StartCoroutine(DoRayLightEffectsCoroutine());
    }

    private IEnumerator DoRayLightEffectsCoroutine()
    {
        bool isOnMax = Random.Range(0, 2) == 1;

        while (true)
        {
            float pickedIntensity = isOnMax ? minIntensity : maxIntensity;
            float pickedDuration = Random.Range(changeIntensityDurationMin, changeIntensityDurationMax);
            float timer = 0;

            isOnMax = !isOnMax;

            float startIntensity = _rayLight.intensity;

            while (timer < pickedDuration)
            {
                timer += Time.deltaTime;
                _rayLight.intensity = Mathf.Lerp(startIntensity, pickedIntensity, timer / pickedDuration);

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.25f);
        }
    }
}
