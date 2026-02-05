using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionFloor : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color highlightFadeColor;
    [SerializeField] private Color unhighlightFadeColor;

    [Header("Private Infos")]
    private EnviroData data;
    private int floorIndex;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _recommandedLevelText;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _bossIcon;


    public void Initialise(EnviroData data, int floorIndex)
    {
        this.data = data;
        this.floorIndex = floorIndex;

        _floorText.text = "FLOOR " + floorIndex;
        _recommandedLevelText.text = "RECOMMANDED LEVEL : " + data.recommandedLevels[floorIndex];

        if(floorIndex == 2 || floorIndex == 5) _bossIcon.enabled = true;
        else _bossIcon.enabled = false;

        _fadeImage.color = unhighlightFadeColor;
    }


    public void Highlight()
    {
        _fadeImage.DOColor(highlightFadeColor, 0.4f);
    }

    public void Unhighlight()
    {
        _fadeImage.DOColor(unhighlightFadeColor, 0.4f);
    }
}
