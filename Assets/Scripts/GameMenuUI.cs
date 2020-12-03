using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : MonoBehaviour
{
    [SerializeField] Transform mainMenuTransform = default;
    [SerializeField] Transform tutorialObject = default;
    [SerializeField] Button[] menuButtons = new Button[3];
    [SerializeField] Color defaultButtonColor = Color.white;
    [SerializeField] Color selectedButtonColor = Color.green;
    int currentMenuButtonIndex = 0;
    public static bool isUIActive { get; private set; }
    bool firstTimeIn = false;

    // Start is called before the first frame update
    void Start()
    {
        isUIActive = true;
        tutorialObject.gameObject.SetActive(false);
        menuButtons[currentMenuButtonIndex].GetComponent<Image>().color = selectedButtonColor;

#if UNITY_EDITOR
        //PlayerPrefs.DeleteKey("EnteredPreviously");
        isUIActive = false;
        gameObject.SetActive(false);
#endif

        firstTimeIn = (PlayerPrefs.GetInt("EnteredPreviously", 0) == 0);
        if(firstTimeIn)
        {
            ToggleTutorialVisibility();
            PlayerPrefs.SetInt("EnteredPreviously", 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialObject.gameObject.activeSelf)
        {
            if((Input.GetKeyDown(KeyCode.Escape) && !firstTimeIn) || (Input.GetKeyDown(KeyCode.Return) && firstTimeIn))
            {
                if(firstTimeIn)
                {
                    firstTimeIn = false;
                    Text tutorialObjectText = tutorialObject.GetComponentInChildren<Text>(true);
                    tutorialObjectText.text = tutorialObjectText.text.Replace("[Enter]", "[Esc]");
                }
                ToggleTutorialVisibility();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                menuButtons[currentMenuButtonIndex].GetComponent<Image>().color = defaultButtonColor;
                currentMenuButtonIndex--;
                if (currentMenuButtonIndex < 0) currentMenuButtonIndex = 0;
                menuButtons[currentMenuButtonIndex].GetComponent<Image>().color = selectedButtonColor;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                menuButtons[currentMenuButtonIndex].GetComponent<Image>().color = defaultButtonColor;
                currentMenuButtonIndex++;
                if (currentMenuButtonIndex > 2) currentMenuButtonIndex = 2;
                menuButtons[currentMenuButtonIndex].GetComponent<Image>().color = selectedButtonColor;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                ExecuteAssignedCurrentButtonCommand();
            }
        }
    }

    /// <summary>
    /// Executes the task it is supposed to based on the currently selected button's index.
    /// </summary>
    private void ExecuteAssignedCurrentButtonCommand()
    {
        switch(currentMenuButtonIndex)
        {
            case 0:
                OnPlayButtonPress();
                break;
            case 1:
                ToggleTutorialVisibility();
                break;
            case 2:
                Application.Quit();
                break;
        }
    }

    private void OnPlayButtonPress()
    {
        isUIActive = false;
        mainMenuTransform.gameObject.SetActive(false);
    }

    private void ToggleTutorialVisibility()
    {
        bool isMainMenuNowActive = mainMenuTransform.gameObject.activeSelf;
        mainMenuTransform.gameObject.SetActive(!isMainMenuNowActive);
        tutorialObject.gameObject.SetActive(isMainMenuNowActive);
    }
}
