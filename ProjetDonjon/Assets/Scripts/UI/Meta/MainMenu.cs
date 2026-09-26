using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class MainMenu : MonoBehaviour
{
    enum CurrentScreen
    {
        Main,
        Save,
        DeleteSave
    }

    [Header("Parameters")]
    [SerializeField] private Color baseButtonTextColor;
    [SerializeField] private Color highlightButtonTextColor;

    [Header("Private Infos")]
    private bool isDisplayed;
    private bool isCreatingNewSave;
    private int currentSaveIndex;
    private CurrentScreen currentScreen;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Image[] _mainButtons;
    [SerializeField] private Image[] _outlineMainButtons;
    [SerializeField] private TextMeshProUGUI[] _mainButtonsText;
    [SerializeField] private SaveFileButton[] _saveFileButtons;

    [Header("Save Delete References")]
    [SerializeField] private RectTransform _saveDeleteMainTr;
    [SerializeField] private Image _saveDeleteBackImage;
    [SerializeField] private Image[] _saveDeleteButtons;
    [SerializeField] private Image[] _outlineSaveDeleteButtons;


    private void Start()
    {
        for(int i = 0; i < _saveFileButtons.Length; i++)
        {
            _saveFileButtons[i].OnButtonClickWithoutSave += ClickEmptySave;
            _saveFileButtons[i].OnButtonClickWithSave += ClickNotEmptySave;
        }

        if(GameManager.Instance.StartGameInMainMenu)
        {
            UIManager.Instance.Transition.FadeScreen(0, 0);
            ShowMenu();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if (!isDisplayed) return;

        if(InputManager.wantsToReturn && currentScreen == CurrentScreen.Save)
        {
            HideSaves();
        }
        else if(InputManager.wantsToReturn && currentScreen == CurrentScreen.DeleteSave)
        {
            HideDeleteSaveWarning();
        }
    }


    public void ShowMenu()
    {
        isDisplayed = true;
        _animator.SetBool("IsOpened", true);

        currentScreen = CurrentScreen.Main;

        ActualiseDisplayedButtons();
    }

    public void HideMenu(bool launchGame)
    {
        _animator.SetBool("SavesOpened", false);
        _animator.SetBool("IsOpened", false);

        if (launchGame)
            isDisplayed = false;
    }

    public void ActualiseDisplayedButtons()
    {
        bool hasSave = false;
        foreach(bool hasSaveFile in SaveManager.Instance.HasSaveFile) {
            if(hasSaveFile)
            {
                hasSave = true;
            }
        }

        const int continueButtonIndex = 1;
        if(hasSave) 
        {
            _mainButtons[continueButtonIndex].gameObject.SetActive(true);

            // We set the correct positions for the buttons under the continue
            float yOffset = _mainButtons[1].rectTransform.position.y - _mainButtons[0].rectTransform.position.y;

            for(int i = continueButtonIndex + 1; i < _mainButtons.Length; i++)
            {
                _mainButtons[i].rectTransform.position = _mainButtons[i - 1].rectTransform.position + new Vector3(0, yOffset, 0);
            }
        }
        else
        {
            _mainButtons[continueButtonIndex].gameObject.SetActive(false);

            // We set the correct positions for the buttons under the continue
            float yOffset = _mainButtons[1].rectTransform.position.y - _mainButtons[0].rectTransform.position.y;

            _mainButtons[continueButtonIndex + 1].rectTransform.position = _mainButtons[continueButtonIndex - 1].rectTransform.position 
                + new Vector3(0, yOffset, 0);

            for (int i = continueButtonIndex + 2; i < _mainButtons.Length; i++)
            {
                _mainButtons[i].rectTransform.position = _mainButtons[i - 1].rectTransform.position + new Vector3(0, yOffset, 0);
            }
        }
    }


    #region Main Buttons

    public void HoverButton(int index)
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);

        //_mainButtonsText[index].DOColor(highlightButtonTextColor, 0.2f).SetEase(Ease.OutBack);
        _mainButtons[index].rectTransform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);

        _outlineMainButtons[index].DOFade(1.0f, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnhoverButton(int index)
    {
        //_mainButtonsText[index].DOColor(baseButtonTextColor, 0.2f).SetEase(Ease.OutBack);
        _mainButtons[index].rectTransform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutBack);

        _outlineMainButtons[index].DOFade(0.0f, 0.2f).SetEase(Ease.OutBack);
    }


    public void ClickNewGame()
    {
        DisplaySaves(true);
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);
    }

    public void ClickContinue()
    {
        DisplaySaves(false);
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);
    }

    public void ClickOptions()
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);
    }

    public void ClickQuit()
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);
        Application.Quit();
    }

    #endregion


    #region Save Files

    public void DisplaySaves(bool newSave)
    {
        isCreatingNewSave = newSave;

        currentScreen = CurrentScreen.Save;

        _animator.SetBool("SavesOpened", true);

        for(int i = 0; i < _saveFileButtons.Length; i++)
        {
            _saveFileButtons[i].ActualiseInfos(!newSave);
        }
    }

    public void ClickEmptySave(int index)
    {
        currentSaveIndex = index;

        if (isCreatingNewSave)
        {
            SaveManager.Instance.NewGame(index);
        }

        SaveManager.Instance.LoadGame(index);

        HideMenu(true);
        StartCoroutine(GameManager.Instance.StartGameCoroutine());
    }  

    public void ClickNotEmptySave(int index)
    {
        currentSaveIndex = index;

        if (isCreatingNewSave)
        {
            ShowDeleteSaveWarning();
        }
        else
        {
            SaveManager.Instance.LoadGame(index);

            HideMenu(true);
            StartCoroutine(GameManager.Instance.StartGameCoroutine());
        }
    } 

    public void HideSaves()
    {
        currentScreen = CurrentScreen.Main;
        _animator.SetBool("SavesOpened", false);
    }

    #endregion


    #region Delete Save Warning

    private void ShowDeleteSaveWarning()
    {
        currentScreen = CurrentScreen.DeleteSave;

        _saveDeleteMainTr.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        _saveDeleteBackImage.raycastTarget = true;
        _saveDeleteBackImage.DOFade(0.6f, 0.3f).SetEase(Ease.OutCubic);
    }

    private void HideDeleteSaveWarning()
    {
        currentScreen = CurrentScreen.Save;

        _saveDeleteMainTr.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);

        _saveDeleteBackImage.raycastTarget = false;
        _saveDeleteBackImage.DOFade(0.0f, 0.3f).SetEase(Ease.OutCubic);
    }

    public void HoverDeleteSaveButton(int index)
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 0);

        _saveDeleteButtons[index].rectTransform.DOScale(Vector3.one * 1.1f, 0.15f).SetEase(Ease.OutBack);
        _outlineSaveDeleteButtons[index].DOFade(1.0f, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnhoverDeleteSaveButton(int index)
    {
        _saveDeleteButtons[index].rectTransform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
        _outlineSaveDeleteButtons[index].DOFade(0.0f, 0.2f).SetEase(Ease.OutBack);
    }

    public void ClickDeleteSave()
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        SaveManager.Instance.NewGame(currentSaveIndex);
        SaveManager.Instance.LoadGame(currentSaveIndex);

        HideMenu(true);
        StartCoroutine(GameManager.Instance.StartGameCoroutine());
    }

    public void ClickCancelDeleteSave()
    {
        AudioManager.Instance.PlaySoundOneShotRandomPitch(0.9f, 1.1f, 0, 1);

        HideDeleteSaveWarning();
    }

    #endregion
}
