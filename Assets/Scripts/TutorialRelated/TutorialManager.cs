using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager current;

    private bool nextSection = false;
    private bool tutorialInProgress = false;
    private bool tutorialCanceled = false;

    private bool gameReset = false;

    [SerializeField]
    private GameObject ArrowPrefab;

    [SerializeField]
    private TutorialButtonCover TutorialButtonPrefab;

    private List<TutorialButtonCover> instantiatedButtonPrefabs = new List<TutorialButtonCover>();

    private List<GameObject> instantiatedPrefabs = new List<GameObject>();

    [SerializeField]
    private GameObject BehaviourOrderTutorialObject;

    [SerializeField]
    private Text TutorialText;

    [SerializeField]
    private Text TutorialButtonText;

    [SerializeField]
    private ControlButton[] Buttons;
    private Dictionary<ECubeAction, ControlButton> ButtonsDict;

    private Dictionary<ECubeAction, bool> ButtonActions = new Dictionary<ECubeAction, bool>();

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        ButtonsDict = new Dictionary<ECubeAction, ControlButton>();
        foreach (ControlButton cB in Buttons)
        {
            cB.SetInteractableAndHightlighted();
            ButtonsDict.Add(cB.ButtonType, cB);
        }

        foreach (ECubeAction val in Enum.GetValues(typeof(ECubeAction)))
        {
            ButtonActions.Add(val, false);
        }
    }

    private void Start()
    {
        //SetTutorial(ETutorial.Intro);
        PuzzleManager.current.OnButtonClick += Event_OnButtonClick;
        PuzzleManager.current.OnGameRetry += Event_OnGameRetry;
    }

    private void Event_OnGameRetry()
    {
        if (tutorialInProgress) 
        {
            gameReset = true;
        }
    }

    private void Event_OnButtonClick(ECubeAction action)
    {
        if (tutorialInProgress) 
        {
            ButtonActions[action] = true;
        }
    }

    private void ResetAllButtons() 
    {
        foreach (KeyValuePair<ECubeAction, ControlButton> buttonKVP in ButtonsDict) buttonKVP.Value.SetInteractableAndHightlighted();
        TutorialButtonText.gameObject.SetActive(true);
        TutorialButtonText.text = "Next";
    }

    private int AddTutorialButton() 
    {
        int newIndex = instantiatedButtonPrefabs.Count;
        TutorialButtonCover newButton = Instantiate(TutorialButtonPrefab, BehaviourOrderTutorialObject.transform);
        newButton.transform.SetParent(BehaviourOrderTutorialObject.transform);
        newButton.transform.position = Vector3.zero;
        instantiatedButtonPrefabs.Add(newButton);

        return newIndex;
    }

    private void ClearAllTutorialButtons() 
    {
        foreach (TutorialButtonCover tButton in instantiatedButtonPrefabs) if(tButton != null) Destroy(tButton.gameObject);
        instantiatedButtonPrefabs.Clear();
    }

    public void SetTutorial(ETutorial tutorial)
    {
        if (!tutorialInProgress)
        {
            tutorialInProgress = true;
            tutorialCanceled = false;
            switch (tutorial)
            {
                case ETutorial.Intro:
                    Tutorial_Intro();
                    break;
                default:
                    tutorialInProgress = false;
                    break;
            }
        }
    }

    public void CancelTutorial() 
    {
        tutorialCanceled = true;
        tutorialInProgress = false;
        foreach (GameObject gObj in instantiatedPrefabs) Destroy(gObj);
        instantiatedPrefabs.Clear();
        ClearAllTutorialButtons();
        UIManager.current.SetActiveContexts(false, EUIContext.TutorialMenu);
    }

    private async void Tutorial_Intro()
    {
        UIManager.current.SetActiveContexts(true, EUIContext.TutorialMenu);
        ResetAllButtons();
        TutorialText.text = "Welcome to Cube-ular!";

        if (!await WaitForButton_Next()) return;
        TutorialText.text = "In Cube-ular,\n the goal is to get the cube(s) to cover all the green squares!";

        foreach (GoalTrigger goal in PuzzleObjectManager.current.AllCurrentGoals) 
        {
            GameObject newArrow = Instantiate(ArrowPrefab);
            newArrow.transform.position = goal.transform.position;
            instantiatedPrefabs.Add(newArrow);
        }

        if (!await WaitForButton_Next()) return;

        TutorialText.text = "Use the directional arrow buttons below to make a sequence that reaches the goal(s)!";

        if (!await WaitForButton_Next()) return;
        
        TutorialButtonText.gameObject.SetActive(false);

        //Find out what the first button needed for this puzzle is
        ECubeAction FirstAction = GameManager.current.CurrentPuzzleSoution[0];
        ButtonsDict[FirstAction].SetInteractableAndHightlighted(true, true);
        TutorialText.text = "The first action for this puzzle is " + FirstAction.ToString() + "! \n Go ahead and press that now.";


        if (!await WaitForButtonToBePressed(FirstAction)) return;

        AddTutorialButton();

        ButtonsDict[FirstAction].SetInteractableAndHightlighted(false, false);

        TutorialText.text = "Nice one! \n Depending on the puzzle you may need to use other directional arrows. \n Try pressing one now.";

        foreach (ECubeAction cubeAction in new ECubeAction[] { ECubeAction.TurnLeft, ECubeAction.TurnRight }) 
        {
            ButtonsDict[cubeAction].SetInteractableAndHightlighted(true, true);
        }


        if (!await WaitForButtonsToBePressed(ECubeAction.TurnLeft, ECubeAction.TurnRight)) return;

        int indexOfSecondAction = AddTutorialButton();

        foreach (ECubeAction cubeAction in new ECubeAction[] { ECubeAction.TurnLeft, ECubeAction.TurnRight })
        {
            ButtonsDict[cubeAction].SetInteractableAndHightlighted(false, false);
        }

        TutorialText.text = "Great! \n Now lets test this out by pressing the Play Sequence button!";

        ButtonsDict[ECubeAction.Play].SetInteractableAndHightlighted(true, true);

        

        if (!await WaitForButtonToBePressed(ECubeAction.Play)) return;

        ButtonsDict[ECubeAction.Play].SetInteractableAndHightlighted(false, false);

        UIManager.current.SetActiveContexts(false, EUIContext.TutorialMenu);

        if (!await WaitForGameReset()) return;

        UIManager.current.SetActiveContexts(true, EUIContext.TutorialMenu);

        TutorialButtonText.gameObject.SetActive(true);

        TutorialText.text = "Oh dear! \n The cube didn't go far enough and it didn't need to turn!";

        if (!await WaitForButton_Next()) return;

        TutorialButtonText.gameObject.SetActive(false);

        TutorialText.text = "The game will mark bad inputs. \n Luckily we can remove this input by clicking on it. \n Try doing that now.";

        TutorialButtonCover buttonToEnable = instantiatedButtonPrefabs[indexOfSecondAction];

        buttonToEnable.SetInteractableAndIndicator(true, true);

        if (!await WaitForButtonsToBePressed(ECubeAction.RemovedDirection)) return;

        Destroy(buttonToEnable.gameObject);

        TutorialText.text = "Nice! \n If you ever get a sequence wrong you can remove any item by clicking on it.";

        TutorialButtonText.gameObject.SetActive(true);

        if (!await WaitForButton_Next()) return;

        TutorialText.text = "You should now understand the basics of Cube-ular! \n Go ahead and finish this level. \n You got this!";

        TutorialButtonText.text = "Close";

        if (!await WaitForButton_Next()) return;

        tutorialInProgress = false;

        ClearAllTutorialButtons();
        UIManager.current.SetActiveContexts(false, EUIContext.TutorialMenu);
        GameManager.current.UpdateSave();

    }

    private async Task<bool> WaitForButton_Next()
    {
        nextSection = false;
        while (!nextSection && !tutorialCanceled) 
        {
            await Task.Yield();
        }

        return !tutorialCanceled;
    }

    private async Task<bool> WaitForButtonToBePressed(ECubeAction button) 
    {
        ButtonActions[button] = false;
        while (!ButtonActions[button] & !tutorialCanceled) 
        {
            await Task.Yield();
        }

        return !tutorialCanceled;
    }

    private async Task<bool> WaitForButtonsToBePressed(params ECubeAction[] buttons)
    {
        foreach (ECubeAction action in buttons)
        {
            ButtonActions[action] = false;
        }

        while (!AnyAreTrue(buttons) & !tutorialCanceled)
        {
            await Task.Yield();
        }

        bool AnyAreTrue(ECubeAction[] actions) 
        {
            foreach (ECubeAction action in actions) 
            {
                if (ButtonActions[action]) return true;
            }
            return false;
        }

        return !tutorialCanceled;
    }

    private async Task<bool> WaitForGameReset() 
    {
        gameReset = false;

        while (!gameReset & !tutorialCanceled)
        {
            await Task.Yield();
        }

        return !tutorialCanceled;
    }

    public void Button_Next()
    {
        nextSection = true;
    }
}

[System.Serializable]
public class ControlButton
{
    public ECubeAction ButtonType;

    [SerializeField]
    private Image fadedOutImage;
    [SerializeField]
    private GameObject highlightImage;

    public bool Interactable
    {
        get { return !fadedOutImage.enabled; }
        set { fadedOutImage.enabled = !value; }
    }

    public bool Highlighted
    {
        get { return highlightImage.activeInHierarchy; }
        set { highlightImage.SetActive(value); }
    }

    public void SetInteractableAndHightlighted(bool isInteracable = false, bool isHightlighted = false)
    {
        Interactable = isInteracable;
        Highlighted = isHightlighted;
    }
}

public enum ETutorial
{
    Intro
}