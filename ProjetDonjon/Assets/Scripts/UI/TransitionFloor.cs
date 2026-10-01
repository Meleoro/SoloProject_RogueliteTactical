using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionFloor : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color highlightFadeColor;
    [SerializeField] private Color unhighlightFadeColor;
    [SerializeField] private Sprite[] possibleHeroSprites;

    [Header("Private Infos")]
    private EnviroData data;
    private int floorIndex;
    private Coroutine nextCoroutine;
    private bool isInitialised;
    private Vector3[] saveEnemiesLocalPos = new Vector3[0];
    private Vector3[] saveHeroesLocalPos = new Vector3[0];

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelText;
    [SerializeField] private TextMeshProUGUI _clearedText;
    [SerializeField] private TextMeshProUGUI _nextText;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _bossIcon;
    [SerializeField] private Image[] _heroImages;
    [SerializeField] private Image[] _enemyImages;
    [SerializeField] private Image _topImage;
    [SerializeField] private Image _bottomImage;
    [SerializeField] private Image _topBetweenImage;


    public void Initialise(EnviroData data, int floorIndex)
    {
        if (isInitialised) return;
        isInitialised = true;

        saveEnemiesLocalPos = new Vector3[_enemyImages.Length];
        for(int i = 0; i < _enemyImages.Length; i++)
        {
            saveEnemiesLocalPos[i] = _enemyImages[i].transform.localPosition;
        }

        saveHeroesLocalPos = new Vector3[_heroImages.Length];
        for(int i = 0; i < _heroImages.Length; i++)
        {
            saveHeroesLocalPos[i] = _heroImages[i].transform.localPosition;
        }

        this.data = data;
        this.floorIndex = floorIndex;

        _floorText.text = "FLOOR " + (floorIndex + 1);
        _recommandedLevelText.text = "LEVEL " + data.recommandedLevels[floorIndex];

        if(floorIndex == 2 || floorIndex == 5) _bossIcon.enabled = true;
        else _bossIcon.enabled = false;
        _bossIcon.enabled = false;

        _fadeImage.color = unhighlightFadeColor;

        if(floorIndex == 0)
        {
            _topBetweenImage.enabled = false;
            _bottomImage.enabled = false;
            _topImage.enabled = true;
        }
        else if (floorIndex == 5)
        {
            _topBetweenImage.enabled = true;
            _bottomImage.enabled = true;
            _topImage.enabled = false;
        }
        else
        {
            _topBetweenImage.enabled = true;
            _bottomImage.enabled = false;
            _topImage.enabled = false;
        }

        for(int i = 0; i < _heroImages.Length; i++)
        {
            _heroImages[i].DOFade(0, 0);
        } 
    }


    public void Highlight()
    {
        _fadeImage.DOColor(highlightFadeColor, 0.4f);

        if (floorIndex != 0) return;
        
        for (int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
        {
            _heroImages[i].sprite = possibleHeroSprites[HeroesManager.Instance.Heroes[i].HeroIndex];
            _heroImages[i].SetNativeSize();
            _heroImages[i].DOFade(1, 0f).SetEase(Ease.OutBack);
            _heroImages[i].rectTransform.DOLocalMoveX(_heroImages[i].rectTransform.localPosition.x + 30, 0f).SetEase(Ease.OutBack);
        }
        
    }

    public void Unhighlight()
    {
        _fadeImage.DOColor(unhighlightFadeColor, 0.4f);
    }

    public void ResetVisuals()
    {
        for (int i = 0; i < saveHeroesLocalPos.Length; i++)
        {
            _heroImages[i].rectTransform.localPosition = saveHeroesLocalPos[i];
            _heroImages[i].DOFade(0, 0);
        }

        for (int i = 0; i < saveEnemiesLocalPos.Length; i++)
        {
            _enemyImages[i].rectTransform.localPosition = saveEnemiesLocalPos[i];
            _enemyImages[i].DOFade(1, 0);
        }

        _clearedText.DOFade(0, 0);
    }

    #region Enter / Exit Effects

    public void DoEnterFloorEffects()
    {
        StartCoroutine(HeroAppearCoroutine());
        StartCoroutine(EnemiesHighlightCoroutine());
    }

    public void DoExitEffects()
    {
        StartCoroutine(HeroDisappearCoroutine());
    }

    private IEnumerator HeroAppearCoroutine()
    {
        for(int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
        {
            _heroImages[i].sprite = possibleHeroSprites[HeroesManager.Instance.Heroes[i].HeroIndex];
            _heroImages[i].SetNativeSize();
            _heroImages[i].DOFade(1, 0.5f).SetEase(Ease.OutBack);
            _heroImages[i].rectTransform.DOLocalMoveX(_heroImages[i].rectTransform.localPosition.x + 30, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator HeroDisappearCoroutine()
    {
        for (int i = 0; i < HeroesManager.Instance.Heroes.Length; i++)
        {
            _heroImages[i].DOFade(0, 0.5f).SetEase(Ease.OutBack);
            _heroImages[i].rectTransform.DOLocalMoveX(_heroImages[i].rectTransform.localPosition.x - 30, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator EnemiesHighlightCoroutine()
    {
        for (int i = 0; i < _enemyImages.Length; i++)
        {
            _enemyImages[i].DOFade(1, 0.4f).SetEase(Ease.OutBack);
            _enemyImages[i].rectTransform.DOLocalMoveX(_enemyImages[i].rectTransform.localPosition.x - 20, 0.4f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator EnemiesUnhighlightCoroutine()
    {
        for (int i = 0; i < _enemyImages.Length; i++)
        {
            _enemyImages[i].DOFade(0.7f, 0.4f).SetEase(Ease.OutBack);
            _enemyImages[i].rectTransform.DOLocalMoveX(_enemyImages[i].rectTransform.localPosition.x + 20, 0.4f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.15f);
        }
    }

    #endregion


    #region Next / Clear / Death Effects

    public IEnumerator DoClearedEffectCoroutine()
    {
        _clearedText.rectTransform.localScale = new Vector3(2, 2, 2);
        _clearedText.text = "CLEARED";

        StartCoroutine(EnemiesUnhighlightCoroutine());

        _clearedText.DOFade(1, 0.35f).SetEase(Ease.InOutCubic);
        _clearedText.rectTransform.DOScale(0.9f, 0.45f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.35f);

        _clearedText.DOFade(0.35f, 0.3f).SetEase(Ease.InOutCubic);
    }

    public IEnumerator DoDeadEffectCoroutine()
    {
        _clearedText.rectTransform.localScale = new Vector3(2, 2, 2);
        _clearedText.text = "DEAD";

        StartCoroutine(HeroDisappearCoroutine());
        //StartCoroutine(EnemiesUnhighlightCoroutine());

        _clearedText.DOFade(1, 0.35f).SetEase(Ease.InOutCubic);
        _clearedText.rectTransform.DOScale(1f, 0.45f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.35f);

        _clearedText.DOFade(0.5f, 0.3f).SetEase(Ease.InOutCubic);
    }


    public void StartNextEffect()
    {
        nextCoroutine = StartCoroutine(DoNextEffectCoroutine());
    }

    public void EndNextEffect()
    {
        if(nextCoroutine != null)
            StopCoroutine(nextCoroutine);

        _nextText.DOKill();
        _nextText.DOFade(0f, 0.5f).SetEase(Ease.InOutSine);
    }

    private IEnumerator DoNextEffectCoroutine()
    {
        yield return new WaitForSeconds(1.2f);

        while (true)
        {
            _nextText.DOFade(0.5f, 1).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1.2f);

            _nextText.DOFade(0f, 1).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1.2f);
        }
    }

    #endregion
}
