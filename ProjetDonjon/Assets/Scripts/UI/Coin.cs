using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float goToAimedForce;
    [SerializeField] private float startEjectForce;
    [SerializeField] private float delayBeforeGoToAim;

    [Header("Private Infos")]
    private float timer; 

    [Header("References")]
    [SerializeField] private RectTransform _aimedTr;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Rigidbody2D _rb;


    private void Start()
    {
        _aimedTr = UIManager.Instance.CoinUI.CoinAimedTr;
        transform.localScale = Vector3.one;

        _rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(0f, 1f)).normalized * startEjectForce, ForceMode2D.Impulse);
    }


    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer < delayBeforeGoToAim) return;

        Vector2 dir = _aimedTr.localPosition - _rectTr.localPosition;
        _rb.AddForce(dir.normalized * goToAimedForce);

        if(_aimedTr.rect.height * 0.5f > Mathf.Abs(dir.y) && _aimedTr.rect.width * 0.5f > Mathf.Abs(dir.x))
        {
            InventoriesManager.Instance.AddCoins(1);

            Destroy(gameObject);
        }
    }
}
