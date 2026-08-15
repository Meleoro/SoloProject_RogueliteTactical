using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class SkillTreeNode : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color possessedColor;
    [SerializeField] private Color reachableColor;
    [SerializeField] private Color baseColor;
    [SerializeField] private Sprite[] skillSprites;
    [SerializeField] private Sprite[] passiveSprites;
    [SerializeField] private Sprite[] statSprites;

    [Header("Outline Parameters")]
    [SerializeField] private Color outlineBaseColor;
    [SerializeField] private Color outlineReachableColor;
    [SerializeField] private Color outlineReachableColor2;
    [SerializeField] private Color outlinePossessedColor;

    [Header("Line Renderer Parameters")]
    [SerializeField] private Color lineRendererBaseColor;
    [SerializeField] private Color lineRendererReachableColor;

    [Header("Actions")]
    public Action<SkillTreeNode> OnHover;
    public Action<SkillTreeNode> OnUnhover;
    public Action<SkillTreeNode> OnClickValid;
    public Action<SkillTreeNode> OnClickNotValid;

    [Header("Private Infos")]
    private SkillTreeNodeData data;
    private Hero associatedHero;
    private SkillTreeNode[] linkedNodes;
    private bool isPossessed, isReachable, isSetuped;  
    private Color currentColor;
    private Color currentOutlineColor;
    private Vector3 currentScale;
    private Coroutine hoverCoroutine;
    private Coroutine availableCoroutine;
    private RectTransform currentLeftTr;
    private RectTransform currentRightTr;

    [Header("Public Infos")]
    public RectTransform LeftRectTr { get { return currentLeftTr; } }
    public RectTransform RightRectTr { get { return currentRightTr; } }
    public RectTransform RectTr { get { return _rectTr; } }
    public SkillTreeNodeData Data { get { return data; } }
    public bool IsPossessed { get { return isPossessed; } }

    [Header("References")]
    [SerializeField] private RectTransform _rectTr;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _outlineImage;
    [SerializeField] private Image _mainImage;
    [SerializeField] private Image _shadowImage;

    [Header("References Left Right")]
    [SerializeField] private RectTransform _leftRectSkillTr;
    [SerializeField] private RectTransform _rightRectSkillTr;
    [SerializeField] private RectTransform _leftRectPassiveTr;
    [SerializeField] private RectTransform _rightRectPassiveTr;
    [SerializeField] private RectTransform _leftRectStatTr;
    [SerializeField] private RectTransform _rightRectStatTr;


    private void Start()
    {
        _rectTr.localScale = Vector3.zero;
        isSetuped = false;
    }


    public void SetupNode(Vector3 position, SkillTreeNodeData data, Hero associatedHero)
    {
        _rectTr.localPosition = _rectTr.parent.InverseTransformPoint(position);
        
        this.data = data;
        this.associatedHero = associatedHero;

        if(data.skillData is not null)
        {
            _mainImage.sprite = skillSprites[0];
            _outlineImage.sprite = skillSprites[1];
            currentLeftTr = _leftRectSkillTr;
            currentRightTr = _rightRectSkillTr;
        }
        else if(data.passiveData is not null)
        {
            _mainImage.sprite = passiveSprites[0];
            _outlineImage.sprite = passiveSprites[1];
            currentLeftTr = _leftRectPassiveTr;
            currentRightTr = _rightRectPassiveTr;
        }
        else
        {
            _mainImage.sprite = statSprites[0];
            _outlineImage.sprite = statSprites[1];
            currentLeftTr = _leftRectStatTr;
            currentRightTr = _rightRectStatTr;
        }

        _iconImage.sprite = data.icon;

        _mainImage.SetNativeSize();
        _iconImage.SetNativeSize();
        _shadowImage.SetNativeSize();
        _outlineImage.SetNativeSize();

        ActualiseNodeState(false, false);
    }


    public void SetupLinks(SkillTreeNode[] linkedNodes)
    {
        this.linkedNodes = linkedNodes;

        Vector3[] positions = new Vector3[linkedNodes.Length * 2];

        for (int i = 0; i < linkedNodes.Length; i++)
        {
            positions[i * 2] = transform.position;
            positions[i * 2 + 1] = linkedNodes[i].transform.position;
        }

        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);

        isSetuped = true;
    }


    private void Update()
    {
        if (!isSetuped) return;

        Vector3[] positions = new Vector3[linkedNodes.Length * 2];

        for (int i = 0; i < linkedNodes.Length; i++)
        {
            positions[i * 2] = RightRectTr.position;
            positions[i * 2 + 1] = linkedNodes[i].LeftRectTr.position;
        }

        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);
    }


    public void VerifyNodeState(bool previousPossessed)
    {
        if (isPossessed)
        {
            for(int i = 0; i < linkedNodes.Length; i++)
            {
                linkedNodes[i].VerifyNodeState(true);
            }
            return;
        }

        if (previousPossessed)
        {
            ActualiseNodeState(false, true);
        }

        for (int i = 0; i < linkedNodes.Length; i++)
        {
            linkedNodes[i].VerifyNodeState(false);
        }
    }

    public void ActualiseNodeState(bool isPossessed, bool isReachable)
    {
        this.isPossessed = isPossessed;
        this.isReachable = isReachable;

        ActualiseVisuals();
    }


    #region GameFeel

    public void ActualiseVisuals()
    {
        if (availableCoroutine != null) StopCoroutine(availableCoroutine);
        _rectTr.DOComplete();
        _outlineImage.DOComplete();
        _mainImage.DOComplete();
        _iconImage.DOComplete();

        if (isPossessed)
        {
            currentColor = possessedColor;
            currentOutlineColor = outlinePossessedColor;
            currentScale = Vector3.one * 0.85f;

            _outlineImage.DOColor(currentOutlineColor, 0.2f);
            _lineRenderer.startColor = lineRendererReachableColor;
            _lineRenderer.endColor = lineRendererReachableColor;

            _iconImage.DOColor(currentOutlineColor, 0.2f);
        }
        else if (isReachable)
        {
            currentColor = reachableColor;
            currentScale = Vector3.one * 0.85f;
            currentOutlineColor = outlineReachableColor;

            _lineRenderer.startColor = lineRendererBaseColor;
            _lineRenderer.endColor = lineRendererBaseColor;
            _outlineImage.DOColor(currentOutlineColor, 0.2f);
            availableCoroutine = StartCoroutine(AvailableEffectCoroutine());

            _iconImage.DOColor(currentOutlineColor, 0.2f);
        }
        else
        {
            currentColor = baseColor;
            currentScale = Vector3.one * 0.8f;
            currentOutlineColor = outlineBaseColor;

            _outlineImage.DOColor(currentOutlineColor, 0.2f);
            _lineRenderer.startColor = lineRendererBaseColor;
            _lineRenderer.endColor = lineRendererBaseColor;

            _iconImage.DOColor(currentOutlineColor, 0.2f);
        }

        _mainImage.color = currentColor;

        _rectTr.UStopChangeScale();
        _rectTr.localScale = currentScale;
    }


    private IEnumerator HoverNodeCoroutine()
    {
        if(availableCoroutine != null) StopCoroutine(availableCoroutine);

        _outlineImage.DOComplete();
        _iconImage.DOComplete();
        _mainImage.DOComplete();

        _mainImage.DOColor(currentColor + Color.white * 0.2f, 0.2f).SetEase(Ease.OutCubic);
        _iconImage.DOColor(outlinePossessedColor, 0.2f).SetEase(Ease.OutCubic);
        _outlineImage.DOColor(outlinePossessedColor, 0.2f).SetEase(Ease.OutCubic);

        _rectTr.UChangeScale(0.1f, currentScale + Vector3.one * 0.25f, CurveType.EaseOutSin);

        yield return new WaitForSeconds(0.1f);

        _rectTr.UChangeScale(0.2f, currentScale + Vector3.one * 0.15f, CurveType.EaseInOutSin);
    }


    private IEnumerator ClickNodeEffectCoroutine()
    {
        _rectTr.UChangeScale(0.1f, currentScale - Vector3.one * 0.15f, CurveType.EaseOutSin);

        yield return new WaitForSeconds(0.1f);

        _rectTr.UChangeScale(0.2f, currentScale, CurveType.EaseInOutSin);
    }


    public IEnumerator AvailableEffectCoroutine(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        while(true)
        {
            _rectTr.UChangeScale(0.75f, currentScale * 0.93f, CurveType.EaseInOutSin);
            _outlineImage.DOColor(outlineReachableColor2, 0.75f).SetEase(Ease.InOutSine);
            _mainImage.DOColor(reachableColor + Color.white * 0.05f, 0.75f).SetEase(Ease.InOutSine);
            _iconImage.DOColor(outlineReachableColor2, 0.75f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1f);

            _rectTr.UChangeScale(0.75f, currentScale, CurveType.EaseInOutSin);
            _outlineImage.DOColor(outlineReachableColor, 0.75f).SetEase(Ease.InOutSine);
            _mainImage.DOColor(reachableColor, 0.75f).SetEase(Ease.InOutSine);
            _iconImage.DOColor(outlineReachableColor, 0.75f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(0.9f);
        }
    }

    #endregion


    #region Mouse Inputs

    public void HoverNode()
    {
        OnHover.Invoke(this);

        AudioManager.Instance.PlaySoundOneShot(0, 0);

        hoverCoroutine = StartCoroutine(HoverNodeCoroutine());
    }


    public void UnhoverNode()
    {
        OnUnhover.Invoke(this);

        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        if (isReachable && !isPossessed) availableCoroutine = StartCoroutine(AvailableEffectCoroutine(0.2f));

        _outlineImage.DOComplete();
        _iconImage.DOComplete();
        _mainImage.DOComplete();

        _mainImage.DOColor(currentColor, 0.2f).SetEase(Ease.OutCubic);
        _iconImage.DOColor(currentOutlineColor, 0.2f).SetEase(Ease.OutCubic);
        _outlineImage.DOColor(currentOutlineColor, 0.2f).SetEase(Ease.OutCubic);

        _rectTr.UChangeScale(0.2f, currentScale, CurveType.EaseInOutSin);
    }

    public void ClickNode()
    {
        if (!isReachable || isPossessed) return;
        if (associatedHero.CurrentSkillTreePoints == 0) return;

        AudioManager.Instance.PlaySoundOneShot(0, 1);

        OnClickValid.Invoke(this);

        StartCoroutine(ClickNodeEffectCoroutine());
    }

    #endregion
}
