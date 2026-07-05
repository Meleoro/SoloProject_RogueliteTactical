using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float goToAimedForce;
    [SerializeField] private float startEjectForce;
    [SerializeField] private float delayBeforeGoToAim;
    [SerializeField] private float randomDelayOffset;
    [SerializeField] private Sprite[] possibleCoinSprites;

    [Header("Private Infos")]
    private float timer; 

    [Header("References")]
    [SerializeField] private RectTransform _aimedTr;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Image _image;


    private void Start()
    {
        _aimedTr = UIManager.Instance.CoinUI.CoinAimedTr;
        transform.localScale = Vector3.one;

        _rb.AddForce(new Vector2(Random.Range(-0.75f, 0.75f), Random.Range(0.6f, 1f)).normalized * startEjectForce, ForceMode2D.Impulse);
        _image.sprite = possibleCoinSprites[Random.Range(0, possibleCoinSprites.Length)];
        _image.SetNativeSize();

        timer = Random.Range(-randomDelayOffset, randomDelayOffset);
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
