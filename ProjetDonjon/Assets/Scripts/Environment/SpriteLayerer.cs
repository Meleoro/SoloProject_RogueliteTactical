using UnityEngine;

public class SpriteLayerer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int offset;
    [SerializeField] private bool autoInitialise = true;

    [Header("Public Infos")]
    [HideInInspector] public int PublicOffset;

    [Header("Private Infos")]
    private Transform referenceTr;
    private bool isInitialised;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
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

        if(_heroParentTr is not null && referenceTr == _heroParentTr)
        {
            _spriteRenderer.sortingOrder = 100;

            isInitialised = false;
        }
    }


    private void Update()
    {
        if (!isInitialised) return;

        _spriteRenderer.sortingOrder = Mathf.Clamp(100 - (int)((transform.position.y - referenceTr.position.y) * 5) + offset + PublicOffset, 1, 200);
    }
}
