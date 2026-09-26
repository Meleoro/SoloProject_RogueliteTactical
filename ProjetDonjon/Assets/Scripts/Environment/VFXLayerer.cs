using UnityEngine;

public class VFXLayerer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int offset;
    [SerializeField] private bool autoInitialise = true;

    [Header("Public Infos")]
    [HideInInspector] public int PublicOffset;

    [Header("Private Infos")]
    private Transform referenceTr;
    private bool isInitialised;
    private Renderer[] renderers;

    [Header("References")]
    [SerializeField] private ParticleSystem[] _particleSystems;
    [SerializeField] private Transform _heroParentTr;   // Needed to avoid to apply the update on the reference sprite


    private void Start()
    {
        if (autoInitialise) return;

        Initialise(HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].transform);
    }

    public void Initialise(Transform referenceTr)
    {
        this.referenceTr = referenceTr;
        isInitialised = true;

        if (_heroParentTr is not null && referenceTr == _heroParentTr)
        {
            renderers = new Renderer[_particleSystems.Length];
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                renderers[i] = _particleSystems[i].GetComponent<Renderer>();
                renderers[i].sortingOrder = 100;
            }

            isInitialised = false;
        }
    }


    private void Update()
    {
        if (!isInitialised) return;

        for (int i = 0; i < _particleSystems.Length; i++)
        {
            _particleSystems[i].GetComponent<Renderer>().sortingOrder = 
                Mathf.Clamp(100 - (int)((_particleSystems[i].transform.position.y - referenceTr.position.y) * 5) + offset + PublicOffset, 1, 200);
        }
    }
}
