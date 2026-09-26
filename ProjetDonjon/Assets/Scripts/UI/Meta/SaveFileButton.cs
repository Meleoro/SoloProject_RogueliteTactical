using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class SaveFileButton : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int buttonIndex;

    [Header("Actions")]
    public Action<int> OnButtonClickWithSave;
    public Action<int> OnButtonClickWithoutSave;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _saveNameText;
    [SerializeField] private TextMeshProUGUI _relicsCountText;
    [SerializeField] private TextMeshProUGUI _campLevelText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _emptyText;
    [SerializeField] private Image _outlineImage;
    [SerializeField] private Image _notAvailableImage;


    private void Start()
    {
        ActualiseInfos(false);
    }


    public void ActualiseInfos(bool fromContinue)
    {
        if (!SaveManager.Instance.HasSaveFile[buttonIndex])
        {
            _relicsCountText.enabled = false;
            _campLevelText.enabled = false;
            _goldText.enabled = false;
            _emptyText.enabled = true;

            if(fromContinue)
            {
                SetIsAvailable(false);
            }
            else
            {
                SetIsAvailable(true);
            }

            return;
        }

        SetIsAvailable(true);

        _relicsCountText.enabled = true;
        _campLevelText.enabled = true;
        _goldText.enabled = true;
        _emptyText.enabled = false;

        GameData data = SaveManager.Instance.GameDatas[buttonIndex];
        int relicCount = 0;

        for (int i = 0; i < data.possessedRelicsIndexes.Length; i++)
        {
            if (!data.possessedRelicsIndexes[i]) continue;

            relicCount++;
        }

        _saveNameText.text = "SAVE N°" + (buttonIndex + 1);
        _relicsCountText.text = "RELICS : " + relicCount + "/" + data.possessedRelicsIndexes.Length;
        _campLevelText.text = "CAMP LVL : " + data.campLevel;
        _goldText.text = "GOLD : " + data.gold;
    }


    public void SetIsAvailable(bool isAvailable)
    {
        _notAvailableImage.enabled = !isAvailable;
    }


    public void Hover()
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);

        _mainRectTr.DOScale(Vector3.one * 1.05f, 0.15f).SetEase(Ease.OutBack);
        _outlineImage.DOFade(1.0f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Unhover()
    {
        _mainRectTr.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutBack);
        _outlineImage.DOFade(0.0f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Click()
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        if (!SaveManager.Instance.HasSaveFile[buttonIndex])
        {
            OnButtonClickWithoutSave.Invoke(buttonIndex);
        }
        else
        {
            OnButtonClickWithSave.Invoke(buttonIndex);
        }
    }
}
