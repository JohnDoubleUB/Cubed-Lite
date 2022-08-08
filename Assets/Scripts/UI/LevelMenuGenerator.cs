using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenuGenerator : MonoBehaviour
{
    [SerializeField]
    private LevelButton[] levelButtons;

    [SerializeField]
    private Button nextPage;
    [SerializeField]
    private Button lastPage;

    [SerializeField]
    private int currentPage = 0;

    private int latestLevelUnlocked;
    private int totalLevelButtons;

    private void Awake()
    {
        totalLevelButtons = levelButtons.Length;
    }

    private void OnEnable()
    {
        latestLevelUnlocked = GameManager.current.LatestLevelInSaveData + 1;

        //set page to be the page with the GameManager.current.levelToLoad on
        currentPage = GetLevelToLoadLevelPage();
        UpdateLevelButtons();
        UpdateNavigationButtons();
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int currentLevel = i + 1 + (currentPage * totalLevelButtons);
            if (currentLevel < SceneManager.sceneCountInBuildSettings)
            {
                levelButtons[i].gameObject.SetActive(true);
                levelButtons[i].Level = currentLevel;
                levelButtons[i].IsUnlocked = currentLevel <= latestLevelUnlocked;
            }
            else
            {
                levelButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateNavigationButtons()
    {
        //Get the current page
        int currentlyDisplayedButtons = (currentPage + 1) * totalLevelButtons;

        //Check if next button should be enabled
        //If the latest level we have unlocked is greater than the currently displayed buttons
        nextPage.interactable = latestLevelUnlocked > currentlyDisplayedButtons;

        //Check if previous button should be enabled
        //If we are not on the first page (index 0)
        lastPage.interactable = currentPage != 0;
    }

    public void Button_ChangePage(int change)
    {
        currentPage += change;
        UpdateLevelButtons();
        UpdateNavigationButtons();
    }

    private int GetLevelToLoadLevelPage()
    {
        float pageToStartOn = (float)GameManager.current.LevelToLoad / totalLevelButtons;
        return pageToStartOn > Math.Floor(pageToStartOn) ? (int)pageToStartOn : Mathf.Min(0, (int)pageToStartOn - 1);
    }
}
