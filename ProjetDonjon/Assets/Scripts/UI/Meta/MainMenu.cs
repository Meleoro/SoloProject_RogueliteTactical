using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    enum CurrentScreen
    {
        Main,
        Save
    }

    [Header("Parameters")]
    [SerializeField] private Color baseButtonTextColor;
    [SerializeField] private Color highlightButtonTextColor;

    [Header("Private Infos")]
    private bool isDisplayed;
    private bool isCreatingNewSave;
    private CurrentScreen currentScreen;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Image[] _mainButtons;
    [SerializeField] private TextMeshProUGUI[] _mainButtonsText;
    [SerializeField] private SaveFileButton[] _saveFileButtons;


    private void Start()
    {
        for(int i = 0; i < _saveFileButtons.Length; i++)
        {
            _saveFileButtons[i].OnButtonClick += ClickSave;
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
        _mainButtonsText[index].DOColor(highlightButtonTextColor, 0.2f).SetEase(Ease.OutBack);
        _mainButtons[index].rectTransform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnhoverButton(int index)
    {
        _mainButtonsText[index].DOColor(baseButtonTextColor, 0.2f).SetEase(Ease.OutBack);
        _mainButtons[index].rectTransform.DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.OutBack);
    }


    public void ClickNewGame()
    {
        DisplaySaves(true);
    }

    public void ClickContinue()
    {
        DisplaySaves(false);
    }

    public void ClickOptions()
    {

    }

    public void ClickQuit()
    {
        Application.Quit();
    }

    #endregion


    #region Save Files

    public void DisplaySaves(bool newSave)
    {
        isCreatingNewSave = newSave;

        currentScreen = CurrentScreen.Save;

        _animator.SetBool("SavesOpened", true);
    }

    public void ClickSave(int index)
    {
        if(isCreatingNewSave)
        {
            SaveManager.Instance.NewGame(index);
        }

        SaveManager.Instance.LoadGame(index);

        HideMenu(true);
        StartCoroutine(GameManager.Instance.StartGameCoroutine());
    }

    public void HideSaves()
    {
        currentScreen = CurrentScreen.Main;
        _animator.SetBool("SavesOpened", false);
    }

    #endregion
}
