using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Text VersionText;

    public Dictionary<int, LevelData> saveData;
    private System.DateTime lastSaveDateTimeUTC;

    [SerializeField]
    private double totalDaysSinceLastSave;

    public int levelOverrideOption = 7;

    [SerializeField]
    private AudioClip[] ButtonSounds;

    [SerializeField]
    private Camera mainCamera;

    public double TotalDaysSinceLastSave { get { return totalDaysSinceLastSave; } }
    public int CurrentLevelScore { get { return currentLevelScore; } }
    public int LatestLevelInSaveData { get { return latestLevelInSaveData; } }

    [SerializeField]
    private int currentLevelScore;

    private PuzzleLevelObject CurrentPuzzleLevelObject;


    public int LevelToLoad { get { return levelToLoad; } }

    [SerializeField]
    private int levelToLoad = 1;

    [SerializeField]
    private int latestLevelInSaveData;

    private string LevelName = "Level_";

    public static GameManager current;

    [SerializeField]
    private Image LoadingBar;

    [Header("UI Level Based Text")]
    [SerializeField]
    private Text LevelText;
    [SerializeField]
    private string levelTextString;
    [SerializeField]
    private Text HighestScoreText;
    [SerializeField]
    private string highestScoreTextString;
    [SerializeField]
    private Text AttemptsText;
    [SerializeField]
    private string attemptsTextString;
    [SerializeField]
    private Text HintRecordText;
    [SerializeField]
    private string HintRecordString;

    private int attemptCount;

    public List<ECubeAction> CurrentPuzzleSoution
    {
        get
        {
            if (CurrentPuzzleLevelObject != null) return CurrentPuzzleLevelObject.Solution;
            else return new List<ECubeAction>();
        }
    }

    [Header("Testing Features")]
    public bool TestingFeaturesEnabled = true;
    public GameObject[] TestingRelatedFeatures;
    [Header("Debug Features")]
    public bool DebugFeaturesEnabled = true;
    public GameObject[] DebugRelatedFeatures;


    private void Awake()
    {
        EnableTestingFeatures(TestingFeaturesEnabled);
        EnableDebugFeatures(DebugFeaturesEnabled);
        //foreach (GameObject TestingFeature in TestingRelatedFeatures) TestingFeature.SetActive(TestingFeaturesEnabled);

        if (VersionText != null) VersionText.text = "Version " + Application.version + ".";

        Input.multiTouchEnabled = false;

        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        if (LoadingBar != null)
        {
            LoadingBar.fillAmount = 0;
            LoadingBar.enabled = false;
        }
    }

    public void EnableTestingFeatures(bool enable)
    {
        foreach (GameObject TestingFeature in TestingRelatedFeatures) TestingFeature.SetActive(enable);
    }

    public void EnableDebugFeatures(bool enable)
    {
        foreach (GameObject TestingFeature in DebugRelatedFeatures) TestingFeature.SetActive(enable);
    }

    void Start()
    {
        //New implementation
        if (SaveSystem.TryLoadGameWithMetaData(out SerializedXmlWithMetaData<SerializableDictionary<int, LevelData>> loadedData2) && loadedData2.SerializedObject != null)
        {
            saveData = loadedData2.SerializedObject.Deserialize();
            latestLevelInSaveData = GetLatestLevelInSaveData(); //This changed
            levelToLoad = latestLevelInSaveData + 1;
            lastSaveDateTimeUTC = loadedData2.SaveDateTimeUTC;
            totalDaysSinceLastSave = System.Math.Abs((lastSaveDateTimeUTC - System.DateTime.UtcNow).TotalDays);
        }
        else 
        {
            saveData = new Dictionary<int, LevelData>();
            lastSaveDateTimeUTC = System.DateTime.UtcNow;
            totalDaysSinceLastSave = 0d;
        }
    }

    private int GetLatestLevelInSaveData()
    {
        KeyValuePair<int, LevelData> latestLevel = new KeyValuePair<int, LevelData>(0, new LevelData());

        foreach (KeyValuePair<int, LevelData> level in saveData)
        {
            if (level.Key > latestLevel.Key) latestLevel = level;
        }

        return latestLevel.Key;
    }

    public void Button_PlayGame()
    {
        PlayButtonSound((EButtonSound)1);
        UIManager.current.SetActiveContexts(false, EUIContext.MainMenu, EUIContext.LevelMenu);
        if (!LoadLevel()) LoadLevel(latestLevelInSaveData);
        PuzzleManager.current.ResetActionsAndPuzzleUI();
    }

    public void Button_ExitGame()
    {
        Application.Quit();
    }

    public void Button_PlayOverride()
    {
        if (LoadLevel(levelOverrideOption))
        {
            UIManager.current.SetActiveContexts(false, EUIContext.MainMenu, EUIContext.LevelMenu);
            PuzzleManager.current.ResetActionsAndPuzzleUI();
        }
    }


    public void Button_GotoLevelMenu()
    {
        PlayButtonSound((EButtonSound)1);
        UIManager.current.SetActiveContexts(true, EUIContext.LevelMenu);
    }

    public void Button_LeaveLevelMenu()
    {
        PlayButtonSound((EButtonSound)2);
        UIManager.current.SetActiveContexts(false, EUIContext.LevelMenu);
    }

    public void Button_LeaveCreditsMenu()
    {
        PlayButtonSound((EButtonSound)2);
        UIManager.current.SetActiveContexts(false, EUIContext.CreditsMenu);
    }

    public void Button_LoadLevel(int levelToLoad)
    {
        PlayButtonSound((EButtonSound)0);
        UIManager.current.SetActiveContexts(false, EUIContext.MainMenu, EUIContext.LevelMenu);
        LoadLevel(levelToLoad);
        PuzzleManager.current.ResetActionsAndPuzzleUI();
    }


    public void InitializeLevel(PuzzleLevelObject LevelObject)
    {
        PuzzleObjectManager.current.InitializePuzzleObjects(LevelObject, true);
        CubeManager.current.InitializePuzzleCubes(LevelObject, true);
        PuzzleManager.current.AnyGoalsCountAsWin = LevelObject.AnyGoalsCountAsWin;
        CurrentPuzzleLevelObject = LevelObject;
        currentLevelScore = 1;/*CurrentPuzzleLevelObject.MaxScore;*/
        mainCamera.orthographicSize = LevelObject.CameraProjectionSize;
    }

    public void MarkLevelAsComplete()
    {
        print("Level has been completed!");
        UnloadLevel();
        GoToNextLevel();
    }

    public void ReturnToMainMenu()
    {
        UIManager.current.SetActiveContexts(true, EUIContext.MainMenu);
        UnloadLevel();
    }

    private void GoToNextLevel()
    {
        if (!LoadLevel(levelToLoad + 1))
        {
            //Load the first level again
            LoadLevel(1);
        }

        PuzzleManager.current.ResetActionsAndPuzzleUI();
    }

    private void UnloadLevel()
    {
        SceneManager.UnloadSceneAsync(LevelName + levelToLoad);
        CubeManager.current.DestroyAllExisting();
        PuzzleObjectManager.current.DestroyAllExisting();
    }

    //if level doesn't exist, will return false and not change the levelToLoad value
    private bool LoadLevel(int? levelToLoad = null)
    {
        if (levelToLoad == null)
        {
            levelToLoad = this.levelToLoad;
        }

        string levelToChangeTo = LevelName + levelToLoad;

        if (SceneUtility.GetBuildIndexByScenePath(levelToChangeTo) != -1)
        {
            //ResetLevelScore();
            ShowLoadingBarForProgress(SceneManager.LoadSceneAsync(levelToChangeTo, LoadSceneMode.Additive));
            this.levelToLoad = levelToLoad.Value;
            SetLevelStats();


            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetLevelStats()
    {
        attemptCount = 0;
        AttemptsText.text = attemptsTextString + attemptCount;
        LevelText.text = levelTextString + levelToLoad;
        if (saveData.ContainsKey(levelToLoad))
        {
            LevelData currentLevelData = saveData[levelToLoad];
            HighestScoreText.gameObject.SetActive(true);
            HighestScoreText.text = highestScoreTextString + currentLevelData.Attempts;

            if (currentLevelData.HintsUsed > 0)
            {
                HintRecordText.gameObject.SetActive(true);
                HintRecordText.text = HintRecordString + currentLevelData.HintsUsed;
            }
            else 
            {
                HintRecordText.gameObject.SetActive(false);
            }

        }
        else
        {
            HintRecordText.gameObject.SetActive(false);
            HighestScoreText.gameObject.SetActive(false);
        }
    }

    public void DecreaseScore()
    {
        attemptCount++;
        AttemptsText.text = attemptsTextString + attemptCount;
        currentLevelScore++;
        if (!PuzzleManager.current.HintButtonActive && currentLevelScore > 5) PuzzleManager.current.HintButtonActive = true;
        if (currentLevelScore == 11)
        {
            HintManager.current.IncreaseHintLevel();
            PuzzleManager.current.HintButtonActive = false;
            PuzzleManager.current.HintButtonActive = true;
        }
        //if (currentLevelScore != CurrentPuzzleLevelObject.MinimumScore) currentLevelScore = Mathf.Max(CurrentPuzzleLevelObject.MinimumScore, currentLevelScore - CurrentPuzzleLevelObject.RetryScorePenalty);
    }

    public void ResetLevelScore()
    {
        currentLevelScore = 1/*CurrentPuzzleLevelObject.MaxScore*/;
        SetLevelStats();
    }

    public void UpdateSave()
    {
        SaveSystem.SaveGameNew(saveData);
        totalDaysSinceLastSave = 0d;
        lastSaveDateTimeUTC = System.DateTime.UtcNow;
    }

    public void SaveLevelAndScore()
    {
        if (saveData.ContainsKey(levelToLoad))
        {
            LevelData currentLevelData = saveData[levelToLoad];

            if (currentLevelData.Attempts >= currentLevelScore)
            {
                currentLevelData.Attempts = currentLevelScore;
                //Add in hints used

                //This should make it so if the amount of attempts matches then we only store the lower amount of hints used
                currentLevelData.HintsUsed = currentLevelData.Attempts == currentLevelScore ? Mathf.Min(currentLevelData.HintsUsed, HintManager.current.HintsUsed) : HintManager.current.HintsUsed;
                //Update the date of the attempt
                currentLevelData.DateOfAttemptUTC = System.DateTime.UtcNow;
                SaveSystem.SaveGameNew(saveData);
            }
        }
        else
        {
            saveData.Add(levelToLoad, new LevelData(currentLevelScore, HintManager.current.HintsUsed));
            SaveSystem.SaveGameNew(saveData);
        }

        totalDaysSinceLastSave = 0d;
        lastSaveDateTimeUTC = System.DateTime.UtcNow;

        if (levelToLoad > latestLevelInSaveData) latestLevelInSaveData = levelToLoad;
    }

    private async void ShowLoadingBarForProgress(AsyncOperation loadingProcess)
    {
        LoadingBar.fillAmount = 0;
        LoadingBar.enabled = true;

        while (!loadingProcess.isDone)
        {
            LoadingBar.fillAmount = loadingProcess.progress;
            await Task.Yield();
        }

        LoadingBar.fillAmount = 0;
        LoadingBar.enabled = false;
    }

    //2 go back, 0 load level/Change level, 1 transition to another menu

    public void PlayButtonSound(EButtonSound soundIndex)
    {
        transform.PlayClipAtTransform(ButtonSounds[(int)soundIndex % ButtonSounds.Length], false, 0.6f, false);
    }
}

public enum EButtonSound
{
    LevelTransition,
    MenuTransition,
    GoBack
}
