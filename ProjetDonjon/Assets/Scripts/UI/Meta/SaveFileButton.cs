using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class SaveFileButton : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int buttonIndex;

    [Header("Actions")]
    public Action<int> OnButtonClick;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private TextMeshProUGUI _saveNameText;
    [SerializeField] private TextMeshProUGUI _relicsCountText;
    [SerializeField] private TextMeshProUGUI _campLevelText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _emptyText;


    private void Start()
    {
        ActualiseInfos();
    }


    public void ActualiseInfos()
    {
        if (!SaveManager.Instance.HasSaveFile[buttonIndex])
        {
            _relicsCountText.enabled = false;
            _campLevelText.enabled = false;
            _goldText.enabled = false;
            _emptyText.enabled = true;

            return;
        }

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


    public void Hover()
    {
        _mainRectTr.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Unhover()
    {
        _mainRectTr.DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Click()
    {
        OnButtonClick.Invoke(buttonIndex);
    }
}
