using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Utilities;

public class CollectionRelic : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color hiddenColor;
    [SerializeField] private Color[] colorPerRarity;

    [Header("Private Infos")]
    private RelicData relicData;
    private bool isPossessed;

    [Header("References")]
    [SerializeField] private Image _relicImage;
    [SerializeField] private Image _secondaryImage;
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _rarityText;

    

    public void Setup(RelicData relicData, bool isPossessed)
    {
        if (relicData == null)
        {
            _relicImage.gameObject.SetActive(false);
            _rarityText.enabled = false;
            _nameText.enabled = false;
            return;
        }

        _relicImage.gameObject.SetActive(true);
        _rarityText.enabled = true;
        _nameText.enabled = true;

        _relicImage.sprite = relicData.icon;
        _secondaryImage.sprite = relicData.icon;

        _relicImage.SetNativeSize();
        _secondaryImage.SetNativeSize();

        _rarityText.text = relicData.rarityType.ToString().ToUpper();
        _rarityText.color = colorPerRarity[(int)relicData.rarityType];

        this.relicData = relicData;
        this.isPossessed = isPossessed;

        if (isPossessed)
        {
            _relicImage.color = Color.white;
            _nameText.text = relicData.relicName;
        }
        else
        {
            _relicImage.color = hiddenColor;
            _nameText.text = "????";
        }
    }

    public IEnumerator NewRelicEffectCoroutine()
    {
        _animator.SetTrigger("GetNewRelic");

        //_rectTr.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(0.1f);

        Setup(relicData, true);

        //_rectTr.DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.InOutCubic);
    }


    #region Mouse Functions

    public void Hover()
    {
        _rectTr.UChangeScale(0.2f, Vector3.one * 1.2f, CurveType.EaseOutSin);
    }

    public void Unhover()
    {
        _rectTr.UChangeScale(0.2f, Vector3.one, CurveType.EaseOutSin);
    }

    #endregion
}
