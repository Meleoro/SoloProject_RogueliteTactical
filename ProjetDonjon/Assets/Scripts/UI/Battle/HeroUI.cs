using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;


public class UnitUI : MonoBehaviour
{
    private enum HeartState
    {
        Normal,
        Skull,
        Shield
    }

    [Header("Parameters")]
    [SerializeField] private float appearEffectSpeed;

    [Header("Actions")]
    public Action HoverAction;
    public Action UnHoverAction;
    public Action ClickAction;

    [Header("Private Infos")]
    private float currentAimedRatio;
    private int currentHealth;
    private Alteration[] currentAlterations;
    private Vector3 saveDamageTextPos;
    private Vector3 saveLevelUpTextPos;
    private Unit attachedUnit;
    private HeartState currentHeartState;

    [Header("Public Infos")]
    public RectTransform XPPointsParent { get { return _xpPointsParent; } }
    public Canvas Canvas { get { return _canvas; } }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _changePaternText;
    [SerializeField] private Image _healthFillableImage;
    [SerializeField] private Image _healthFillableImage2;
    [SerializeField] private Image _hearthImage;
    [SerializeField] private RectTransform _parentTr;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    [SerializeField] private Animator _animator;
    [SerializeField] private RectTransform _alterationsParent;
    [SerializeField] private Alteration[] _alterations;
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private Canvas _canvas;

    [Header("XP References")]
    [SerializeField] private Image _xpFillableImage;
    [SerializeField] private Image _xpOutlineImage;
    [SerializeField] private Image _xpBackImage;
    [SerializeField] private RectTransform _xpBarParent;
    [SerializeField] private RectTransform _xpPointsParent;
    [SerializeField] private RectTransform _levelUpTextTr;



    private void Start()
    {
        currentHeartState = HeartState.Normal;

        //HideUnitUI();
        ResetXPProgress();

        saveLevelUpTextPos = _levelUpTextTr.localPosition;
        _levelUpTextTr.localScale = Vector3.zero;
        saveDamageTextPos = _damageText.rectTransform.localPosition;
        _damageText.rectTransform.localScale = Vector3.zero;

        _xpOutlineImage.color = new Color(_xpOutlineImage.color.r, _xpOutlineImage.color.g, _xpOutlineImage.color.b, 0);
        _xpFillableImage.color = new Color(_xpFillableImage.color.r, _xpFillableImage.color.g, _xpFillableImage.color.b, 0);
        _xpBackImage.color = new Color(_xpBackImage.color.r, _xpBackImage.color.g, _xpBackImage.color.b, 0);

        _changePaternText.rectTransform.localScale = Vector3.zero;
        currentAimedRatio = 1;

        attachedUnit = GetComponentInParent<Unit>();
    }

    private void Update()
    {
        _healthFillableImage.fillAmount = Mathf.Lerp(_healthFillableImage.fillAmount, currentAimedRatio, Time.deltaTime * 5f);
        _healthFillableImage2.fillAmount = Mathf.Lerp(_healthFillableImage2.fillAmount, _healthFillableImage.fillAmount, Time.deltaTime * 5f);
    }


    #region XP Bar

    public void GainXP(float newRatio)
    {
        newRatio = Mathf.Clamp(newRatio, 0, 1);
        StartCoroutine(GainXPCoroutine(newRatio));
    }

    private IEnumerator GainXPCoroutine(float newRatio)
    {
        _xpOutlineImage.UFadeImage(0.1f, 1f);
        _xpFillableImage.UFadeImage(0.1f, 1f);
        _xpBackImage.UFadeImage(0.1f, 1f);

        yield return new WaitForSeconds(0.1f);

        _xpFillableImage.ULerpFillAmount(0.6f, newRatio, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.6f);

        if (newRatio == 1)
        {
            _xpBarParent.UChangeScale(0.1f, Vector3.one * 1.2f);

            yield return new WaitForSeconds(0.1f);

            _xpBarParent.UChangeScale(0.2f, Vector3.one);
        }
        else
        {
            _xpOutlineImage.UFadeImage(0.2f, 0f);
            _xpFillableImage.UFadeImage(0.2f, 0f);
            _xpBackImage.UFadeImage(0.2f, 0f);
        }
    }

    public IEnumerator DoLevelUpEffectCoroutine(float duration)
    {
        _levelUpTextTr.localPosition = saveLevelUpTextPos;

        _levelUpTextTr.UChangeScale(duration * 0.1f, new Vector3(1.3f, 0.9f, 1.0f), CurveType.EaseInOutCubic);
        _levelUpTextTr.UChangeLocalPosition(duration, saveLevelUpTextPos + Vector3.up * 25f, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration * 0.1f);

        _levelUpTextTr.UChangeScale(duration * 0.2f, new Vector3(0.85f, 1.15f, 1.0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _levelUpTextTr.UChangeScale(duration * 0.1f, new Vector3(1f, 1f, 1.0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        _levelUpTextTr.UChangeScale(duration * 0.2f, new Vector3(0f, 0f, 0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _levelUpTextTr.localPosition = saveLevelUpTextPos;
    }

    public void ResetXPProgress()
    {
        _xpFillableImage.fillAmount = 0;
    }

    #endregion


    #region Hide / Show

    public void HideUnitUI()
    {
        _animator.SetBool("IsShowned", false);
    }

    public void ShowUnitUI()
    {
        _animator.SetBool("IsShowned", true);
    }

    #endregion


    #region Mouse Inputs

    public void HoverUnit()
    {
        HoverAction?.Invoke();
    }

    public void UnHoverUnit()
    {
        UnHoverAction?.Invoke();
    }

    public void ClickUnit() 
    {
        ClickAction?.Invoke();
    }

    #endregion


    #region Others

    public void ActualiseUI(float currentHealthRatio, int currentHealth, List<AlterationStruct> currentAlterations)
    {
        if(currentAimedRatio != currentHealthRatio && currentAimedRatio != 0)
        {
            StartCoroutine(DoDamageTextEffectCoroutine(this.currentHealth - currentHealth, 1f));

            _hearthImage.rectTransform.UBounceScale(0.15f, Vector3.one * 1.3f, 0.3f, Vector3.one, CurveType.EaseInOutCubic);
            _hearthImage.rectTransform.UShakeLocalPosition(0.3f, 10f, 0.03f, ShakeLockType.Z);
            _parentTr.UShakeLocalPosition(0.3f, 20f, 0.03f, ShakeLockType.Z);
        }

        currentAimedRatio = currentHealthRatio;
        _healthText.text = currentHealth.ToString();
        this.currentHealth = currentHealth;

        bool foundSkull = false;
        bool foundShield = false;

        for (int i = 0; i < _alterations.Length; i++)
        {
            if(i >= currentAlterations.Count)
            {
                _alterations[i].Disappear();
                continue;
            }
            _alterations[i].Appear(currentAlterations[i], attachedUnit);


            // Sield / Skull heart sprites changes
            if (currentAlterations[i].alteration.alterationType == AlterationType.Shield)
            {
                foundShield = true;

                if(currentHeartState == HeartState.Normal)
                {
                    currentHeartState = HeartState.Shield;

                }
            }
            else if (currentAlterations[i].alteration.alterationType == AlterationType.Skulled)
            {
                foundSkull = true;

                if (currentHeartState == HeartState.Normal)
                {
                    currentHeartState = HeartState.Skull;
                    _animator.SetTrigger("HeartSkull");
                }
            }
        }

        if(currentHeartState == HeartState.Shield && !foundShield)
        {
            currentHeartState = HeartState.Normal;

        }
        else if(currentHeartState == HeartState.Skull && !foundSkull)
        {
            currentHeartState = HeartState.Normal;
            _animator.SetTrigger("SkullHeart");
        }
    }

    private IEnumerator DoDamageTextEffectCoroutine(int damages, float duration)
    {
        _damageText.text = "-" + damages;
        _damageText.rectTransform.localPosition = saveDamageTextPos;

        _damageText.rectTransform.UChangeScale(duration * 0.1f, new Vector3(1.3f, 0.9f, 1.0f), CurveType.EaseInOutCubic);
        _damageText.rectTransform.UChangeLocalPosition(duration, saveDamageTextPos + Vector3.up * 35f, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration * 0.1f);

        _damageText.rectTransform.DOScale(new Vector3(0.85f, 1.15f, 1.0f), duration * 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _damageText.rectTransform.DOScale(new Vector3(1f, 1f, 1f), duration * 0.1f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        _damageText.rectTransform.DOScale(new Vector3(0f, 0f, 0f), duration * 0.2f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _damageText.rectTransform.localPosition = saveDamageTextPos;

    }

    public IEnumerator DoChangePaternEffectCoroutine(float duration)
    {
        _changePaternText.rectTransform.localPosition = saveDamageTextPos;

        _changePaternText.rectTransform.UChangeScale(duration * 0.1f, new Vector3(1.3f, 0.9f, 1.0f), CurveType.EaseInOutCubic);
        _changePaternText.rectTransform.UChangeLocalPosition(duration, saveDamageTextPos + Vector3.up * 25f, CurveType.EaseOutCubic);

        yield return new WaitForSeconds(duration * 0.1f);

        _changePaternText.rectTransform.UChangeScale(duration * 0.2f, new Vector3(0.85f, 1.15f, 1.0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _changePaternText.rectTransform.UChangeScale(duration * 0.1f, new Vector3(1f, 1f, 1.0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        _changePaternText.rectTransform.UChangeScale(duration * 0.2f, new Vector3(0f, 0f, 0f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        _changePaternText.rectTransform.localPosition = saveDamageTextPos;

    }

    #endregion
}
