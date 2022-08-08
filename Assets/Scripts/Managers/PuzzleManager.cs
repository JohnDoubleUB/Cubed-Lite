using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField]
    private GameObject HintButton;
    [SerializeField]
    private Text HintButtonText;
    [SerializeField]
    private int MaxSequenceLength = 32;

    public float soundEffectVolume = 0.5f;
    public AudioClip LevelCompleteSound;
    public AudioClip LevelFailedSound;


    public static PuzzleManager current;

    public delegate void OnButtonClickAction(ECubeAction action);
    public event OnButtonClickAction OnButtonClick;

    public delegate void OnGameRetryAction();
    public event OnGameRetryAction OnGameRetry;

    public bool AnyGoalsCountAsWin;

    //public CubeCharacter cubePrefab;

    //public CubeCharacter cube;

    public CubeActionObject actionObjectPrefab;

    public List<ImageAndAction> ImageAndAction = new List<ImageAndAction>();


    public List<ECubeAction> CurrentActions 
    {
        get 
        {
            List<ECubeAction> results = new List<ECubeAction>();
            foreach (CubeActionObject cAO in Actions) results.Add(cAO.Action);
            return results;
        }
    }

    [SerializeField]
    private List<CubeActionObject> Actions = new List<CubeActionObject>();

    [SerializeField]
    private GameObject actionParent;

    [SerializeField]
    private Button[] puzzleControlButtons;

    private Dictionary<ECubeAction, Sprite> _imageAndAction = new Dictionary<ECubeAction, Sprite>();

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;

        foreach (ImageAndAction IAAction in ImageAndAction)
        {
            if (!_imageAndAction.ContainsKey(IAAction.Action)) _imageAndAction.Add(IAAction.Action, IAAction.Sprite);
        }

        SetAllInteractableButtons(false);
    }

    public bool HintButtonActive 
    { 
        get { return HintButton.activeInHierarchy; }
        set 
        { 
            HintButton.SetActive(value);
            if (value) 
            {
                HintButtonText.text = "Click here for hint #" + (HintManager.current.HintLevel + 1) + " !";
            }
        }
    }

    public void AddAction(ECubeAction action, bool setDefaultColor = false)
    {
        if (Actions.Count < MaxSequenceLength)
        {
            CubeActionObject newAction = Instantiate(actionObjectPrefab);
            newAction.transform.SetParent(actionParent.transform, false);
            newAction.transform.position = Vector3.zero;
            newAction.ActionIndex = Actions.Count;
            newAction.gameObject.name = action.ToString();
            newAction.SetIcon(_imageAndAction[action]);
            newAction.Action = action;
            if (setDefaultColor) newAction.SetDefaultColor();
            Actions.Add(newAction);
        }
        else 
        {
            //Notify the user that this is the max length?
        }

        TriggerOnClickEvent(action);
    }

    private void TriggerOnClickEvent(ECubeAction action) 
    {
        OnButtonClick?.Invoke(action);
    }

    public void RemoveActionAtIndex(int index)
    {
        if (index < Actions.Count)
        {
            OnButtonClick?.Invoke(ECubeAction.RemovedDirection);
            Actions.RemoveAt(index);
            for (int i = 0; i < Actions.Count; i++) Actions[i].ActionIndex = i;
        }
    }

    public void AddAction(int actionEnum)
    {
        AddAction((ECubeAction)actionEnum);
    }

    public void ResetActionsAndPuzzleUI(bool SetButtonsActive = true)
    {
        //foreach (CubeActionObject actionObject in Actions) if (actionObject != null) Destroy(actionObject.gameObject);
        //Actions.Clear();
        RemoveAllActions();
        SetAllInteractableButtons(SetButtonsActive);
    }

    public void ExecuteBehaviour(bool withDelay = false)
    {
        
        if (Actions.Any()) 
        {
            if (withDelay)
            {
                ExecuteBehaviourInTurnWithDelay();
            }
            else
            {
                ExecuteBehaviourInTurn();
            }
        }
    }

    private async void ExecuteBehaviourInTurnWithDelay()
    {   
        if (Actions.Any())
        {
            SetAllInteractableButtons(false, true);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            ExecuteBehaviourInTurn();
        }
    }

    private void RemoveAllActions()
    {
        foreach (CubeActionObject actionObject in Actions) if (actionObject != null) Destroy(actionObject.gameObject);
        Actions.Clear();
    }

    private void SetAllInteractableButtons(bool enabled, bool alsoResetUI = false)
    {
        foreach (Button pB in puzzleControlButtons) pB.interactable = enabled;
        foreach (CubeActionObject cAO in Actions)
        {
            cAO.SetInteractable(enabled);
            if (alsoResetUI) cAO.ResetUI();
        }
    }


    private async void ExecuteBehaviourInTurn()
    {
        TriggerOnClickEvent(ECubeAction.Play);
        SetAllInteractableButtons(false, true);

        CubeActionObject lastCAO = null;

        foreach (CubeActionObject cAO in Actions)
        {
            if (lastCAO != null) lastCAO.ResetUI();
            cAO.MarkAsActive();
            lastCAO = cAO;

            if (cAO.Action == ECubeAction.TurnLeft)
            {
                CubeManager.current.RotateCubeY(-90);


                //cube.RotateCubeY(-90);
                while (CubeManager.current.RotationInProgress)
                {
                    await Task.Yield();
                }
            }

            else if (cAO.Action == ECubeAction.TurnRight)
            {
                CubeManager.current.RotateCubeY(90);

                while (CubeManager.current.RotationInProgress)
                {
                    await Task.Yield();
                }
            }


            if (cAO.Action == ECubeAction.Forward)
            {
                CubeManager.current.MoveCubeForward();

                while (CubeManager.current.ForwardMovementInProgress)
                {
                    await Task.Yield();
                }

                if (CubeManager.current.AllCubesHaveFallen)
                {
                    cAO.MarkAsFail();
                    break;
                }

            }
        }

        //if (lastCAO != null) lastCAO.ResetUI();

        //bool cubeFailed = !CubeManager.current.CubeHasReachedGoal;
        bool cubeFailed = AnyGoalsCountAsWin ? !PuzzleObjectManager.current.AnyGoalsCovered : !PuzzleObjectManager.current.AllGoalsCovered;

        if (!CubeManager.current.AllCubesHaveFallen)
        {
            if (cubeFailed) Actions[Actions.Count - 1].MarkAsFail();
            else Actions[Actions.Count - 1].MarkAsSuccess();
        }


        if (CubeManager.current.AllCubesHaveFallen || cubeFailed)
        {
            transform.PlayClipAtTransform(LevelFailedSound, false, soundEffectVolume, false);
            await Task.Delay(new System.TimeSpan(0, 0, 2));
            GameManager.current.DecreaseScore();
            Button_RetryPuzzle(false, false);
            OnGameRetry?.Invoke();
        }
        else
        {
            //Level completed!
            transform.PlayClipAtTransform(LevelCompleteSound, false, soundEffectVolume, false);
            await Task.Delay(new System.TimeSpan(0, 0, 1));
            //GameManager.current.SaveLevelAndScore();
            GameManager.current.SaveLevelAndScore();
            HintButtonActive = false;
            UIManager.current.SetActiveContexts(true, EUIContext.LevelCompleteMenu);
        }
    }

    public void Button_GiveSolution()
    {
        RemoveAllActions();
        foreach (ECubeAction action in GameManager.current.CurrentPuzzleSoution)
        {
            AddAction(action, true);
        }

        ExecuteBehaviour(true);
    }

    public void Button_RetryPuzzle(bool resetUI = false)
    {
        Button_RetryPuzzle(resetUI, true);
    }
    public void Button_RetryPuzzle(bool resetUI = false, bool playSound = true)
    {
        if (playSound) GameManager.current.PlayButtonSound(EButtonSound.GoBack);
        UIManager.current.SetActiveContexts(false, EUIContext.LevelCompleteMenu);
        if (resetUI)
        {
            HintButtonActive = false;
            ResetActionsAndPuzzleUI();
            GameManager.current.ResetLevelScore();
            HintManager.current.ResetHints();
        }
        else
        {
            SetAllInteractableButtons(true);
        }

        CubeManager.current.ResetCubes();
        PuzzleObjectManager.current.ResetPuzzleObjects();
    }

    public void Button_GoToNextPuzzle()
    {
        HintButtonActive = false;
        TutorialManager.current.CancelTutorial();
        HintManager.current.ResetHints();
        GameManager.current.PlayButtonSound(EButtonSound.LevelTransition);
        UIManager.current.SetActiveContexts(false, EUIContext.LevelCompleteMenu);
        GameManager.current.MarkLevelAsComplete();
    }

    public void Button_ReturnToMainMenu()
    {
        HintButtonActive = false;
        TutorialManager.current.CancelTutorial();
        HintManager.current.ResetHints();
        GameManager.current.PlayButtonSound(EButtonSound.GoBack);
        UIManager.current.SetActiveContexts(false, EUIContext.LevelCompleteMenu);
        ResetActionsAndPuzzleUI(false);
        GameManager.current.ReturnToMainMenu();
    }
}

[System.Serializable]
public struct ImageAndAction
{
    public ECubeAction Action;
    public Sprite Sprite;
}

public enum ECubeAction
{
    Forward,
    TurnRight,
    TurnLeft,
    Play,
    RemovedDirection
}