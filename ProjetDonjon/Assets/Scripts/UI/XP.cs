using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class XP : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float goToAimedForce;
    [SerializeField] private float startEjectForce;
    [SerializeField] private float delayBeforeGoToAim;

    [Header("Private Infos")]
    private float timer;
    private int xpValue;

    [Header("References")]
    [SerializeField] private Hero _aimedHero;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Rigidbody2D _rb;


    public void Initialise(Hero aimedHero, int value)
    {
        _aimedHero = aimedHero;
        _rectTr.SetParent(aimedHero.UI.XPPointsParent);

        xpValue = value;    
        transform.localScale = Vector3.one;

        _rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(0f, 1f)).normalized * startEjectForce, ForceMode2D.Impulse);
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer < delayBeforeGoToAim) return;

        Vector2 dir = Vector3.zero - _rectTr.localPosition;
        _rb.AddForce(dir.normalized * goToAimedForce);

        if (40f > Mathf.Abs(dir.y) && 40f > Mathf.Abs(dir.x))
        {
            _aimedHero.GainXP(xpValue);
            Destroy(gameObject);
        }
    }
}
