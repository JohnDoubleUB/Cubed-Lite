using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    public float ExplicitHintAmount_1 = 0.25f;
    public float ExplicitHintAmount_2 = 0.5f;

    [SerializeField]
    private Text hintCountText;

    [SerializeField]
    private Text explicitHintButtonText;

    private bool hintLevelJustIncreased = false;

    [SerializeField]
    private int maxAdsShownPerLevel = 2;
    private int adsShownThisLevel = 0;

    [SerializeField]
    private Button viewAdButton;
    [SerializeField]
    private Button noThanksButton;

    public int HintsUsed
    {
        get
        {
            return hintsUsed;
        }
        set
        {
            hintsUsed = value;

            if (hintsUsed > 0)
            {
                hintCountText.gameObject.SetActive(true);
                hintCountText.text = "Hints Used: " + hintsUsed;
            }
            else
            {
                hintCountText.gameObject.SetActive(false);
            }
        }
    }

    [SerializeField]
    private int hintsUsed = 0;

    private List<ECubeAction> lastUserSequence = null;

    [SerializeField]
    private Color CorrectColor;
    [SerializeField]
    private Color IncorrectColor;

    [SerializeField]
    private Sprite ForwardIcon;
    [SerializeField]
    private Sprite RightIcon;
    [SerializeField]
    private Sprite LeftIcon;

    [SerializeField]
    private Image ImageUiObjectPrefab;

    [SerializeField]
    private Image[] ExplicitHintObjects;

    private int ExplicitHintObjectCount;


    public static HintManager current;

    private int hintLevel = 0;

    public int HintLevel
    {
        get
        {
            return hintLevel;
        }
    }

    private float ExplicitHintModifier
    {
        get
        {
            switch (hintLevel)
            {
                case 0:
                    return ExplicitHintAmount_1;
                case 1:
                default:
                    return ExplicitHintAmount_2;
            }
        }
    }

    [SerializeField]
    private GameObject BehaviourOrderHints_Implicit;
    private List<Image> BehaviourOrderList_Implicit = new List<Image>();

    [SerializeField]
    private GameObject BehaviourOrderHints_Explicit;

    [SerializeField]
    private Text HintText;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
        ExplicitHintObjectCount = ExplicitHintObjects.Length;
    }

    private void Start()
    {
        //Test_QuickAndDirtyTest();
    }

    public void ShowHint()
    {
        //Check if we should show an ad
        List<ECubeAction> currentSequence = PuzzleManager.current.CurrentActions;
        if (hintLevel > 0 && adsShownThisLevel < maxAdsShownPerLevel && CubeularAdMobManager.current != null && CubeularAdMobManager.current.CanShowRewardAd && (CheckIfUserSequenceIsNew(currentSequence, false) || hintLevelJustIncreased))
        {
            UIManager.current.SetActiveContexts(true, EUIContext.AdPermissionMenu);
            viewAdButton.interactable = true;
            noThanksButton.interactable = true;
        }
        else
        {
            UIManager.current.SetActiveContexts(true, EUIContext.HintMenu);
            UIManager.current.SetActiveContexts(false, EUIContext.ExplicitHint);
            GenerateHint(currentSequence);
        }
    }

    public void CloseAdPermissionMenu()
    {
        UIManager.current.SetActiveContexts(false, EUIContext.AdPermissionMenu);
    }

    public void TryAndViewAd()
    {
        if (CubeularAdMobManager.current != null)
        {
            viewAdButton.interactable = false;
            noThanksButton.interactable = false;

            CubeularAdMobManager.current.RequestLoadAndShowRewardedInterstitialAd(
                (result) =>
                {
                    if (result)
                    {
                        //Ad shown successfully!
                        UIManager.current.SetActiveContexts(false, EUIContext.AdPermissionMenu, EUIContext.ExplicitHint);
                        UIManager.current.SetActiveContexts(true, EUIContext.HintMenu);
                        GenerateHint(PuzzleManager.current.CurrentActions);
                        adsShownThisLevel++;
                        viewAdButton.interactable = true;
                        noThanksButton.interactable = true;
                    }
                    else
                    {
                        //Ad failed!
                        InGameNotifcationManager.current.Notify("Ad failed to load! \n Please make sure you are connected to the internet!");
                        viewAdButton.interactable = true;
                        noThanksButton.interactable = true;
                    }
                });
        }
        else
        {
            InGameNotifcationManager.current.Notify("Ad failed to load! \n Please make sure you are connected to the internet!");
        }
    }

    public void CloseHint()
    {
        UIManager.current.SetActiveContexts(false, EUIContext.HintMenu);
        foreach (Image img in BehaviourOrderList_Implicit) Destroy(img.gameObject);
        BehaviourOrderList_Implicit.Clear();
        foreach (Image img in ExplicitHintObjects) img.gameObject.SetActive(false);
    }

    public void IncreaseHintLevel()
    {
        hintLevel++;
        if (hintLevel > 0) hintLevelJustIncreased = true;
    }

    public void ResetHints()
    {
        CloseHint();
        hintLevel = 0;
        HintsUsed = 0;
        adsShownThisLevel = 0;
        hintLevelJustIncreased = false;
        lastUserSequence = null;
        foreach (Image img in BehaviourOrderList_Implicit) Destroy(img.gameObject);
        BehaviourOrderList_Implicit.Clear();
        foreach (Image img in ExplicitHintObjects) img.gameObject.SetActive(false);

    }

    private Sprite GetSpriteByAction(ECubeAction action)
    {
        switch (action)
        {
            case ECubeAction.TurnLeft:
                return LeftIcon;
            case ECubeAction.TurnRight:
                return RightIcon;

            case ECubeAction.Forward:
            default:
                return ForwardIcon;
        }
    }

    private bool CheckIfUserSequenceIsNew(List<ECubeAction> newUserSequence, bool updateLastUserSequence = true)
    {
        if (lastUserSequence == null)
        {
            if (updateLastUserSequence) lastUserSequence = newUserSequence;
            return true;
        }

        if (newUserSequence.Count != lastUserSequence.Count)
        {
            if (updateLastUserSequence) lastUserSequence = newUserSequence;
            return true;
        }

        for (int i = 0; i < newUserSequence.Count; i++)
        {
            if (newUserSequence[i] != lastUserSequence[i])
            {
                if (updateLastUserSequence) lastUserSequence = newUserSequence;
                return true;
            }
        }

        return false;
    }

    public void DisplayExplicitHint() 
    {
        explicitHintButtonText.gameObject.SetActive(false);
        UIManager.current.SetActiveContexts(true, EUIContext.ExplicitHint);
    }

    private void GenerateHint(List<ECubeAction> currentSequence)
    {
        //Get the correct sequence
        List<ECubeAction> correctSequence = GameManager.current.CurrentPuzzleSoution;

        //Determine how many explicit hints we need
        int explicitHintCount = (int)System.Math.Ceiling(correctSequence.Count * ExplicitHintModifier);

        explicitHintButtonText.gameObject.SetActive(true);
        explicitHintButtonText.text = $"Press here to view the first {explicitHintCount} directions in the sequence!";


        //Get the current sequence the player has entered
        //List<ECubeAction> currentSequence = PuzzleManager.current.CurrentActions;

        int currentSequenceLength = currentSequence.Count;

        //Check if this counts as a hint use (i.e. if the user has changed something since the last hint use or this is the first time using a hint)
        if (CheckIfUserSequenceIsNew(currentSequence) || hintLevelJustIncreased)
        {
            HintsUsed++;
            hintLevelJustIncreased = false;
        }



        bool explicitHintCountReached = false;
        bool implicitHitCountReached = false;

        int i;

        ECubeAction? lastAction = null;
        //bool overrideCorrectDirection = false;

        //Iterate over the correct sequence
        for (i = 0; i < correctSequence.Count; i++)
        {
            //Store correct direcction
            ECubeAction correctDirection = correctSequence[i];

            //Show explicit hint object
            if (i < explicitHintCount && i < ExplicitHintObjectCount)
            {
                Image explicitHintObject = ExplicitHintObjects[i];
                explicitHintObject.gameObject.SetActive(true);
                explicitHintObject.sprite = GetSpriteByAction(correctDirection);
            }
            else
            {
                explicitHintCountReached = true;
            }


            //Create an implict hint object
            if (i < currentSequenceLength)
            {
                ECubeAction currentDirection = currentSequence[i];
                Image newImplicitHintObject = Instantiate(ImageUiObjectPrefab, BehaviourOrderHints_Implicit.transform);

                //Commented out so I can use a new helper method
                ////Check if we are dealing with a direction (current and correct)
                //bool correctDirectionIsLeftOrRight = correctDirection == ECubeAction.TurnRight || correctDirection == ECubeAction.TurnLeft;
                //bool currentDirectionIsLeftOrRight = currentDirection == ECubeAction.TurnRight || currentDirection == ECubeAction.TurnLeft;

                //if (correctDirectionIsLeftOrRight && currentDirectionIsLeftOrRight)
                //{
                //    //Out of all the code, I think this might be the most likely to need testing
                //    //If there wasn't a last direction, there is another item in the correct sequence (after this one) and that item matches the current correct direction
                //    if (lastAction == null && i + 1 < correctSequence.Count && correctDirection == correctSequence[i + 1])
                //    {
                //        lastAction = currentDirection; //Set this as the last direction (we need this to match the last one)

                //        //Force this direction to always be correct!!!!!
                //        newImplicitHintObject.color = CorrectColor;

                //    }
                //    else if (lastAction != null) //If there is a last action
                //    {
                //        //Check this against the current one and see if they match, if they do then it should be correct!
                //        newImplicitHintObject.color = lastAction == currentDirection ? CorrectColor : IncorrectColor;
                //        lastAction = null; //Null this as its the end of the 2 part sequence
                //    }
                //    else //If there isn't a last action then we do the normal check
                //    {
                //        //Behave normally!!!!!
                //        newImplicitHintObject.color = currentDirection == correctDirection ? CorrectColor : IncorrectColor;
                //    }


                //}
                //else //If the direction isn't either of those we do the normal thing
                //{
                //    lastAction = null; //We also set this to null to break the sequence

                //    //Behave normally!!!!!
                //    newImplicitHintObject.color = currentDirection == correctDirection ? CorrectColor : IncorrectColor;
                //}

                //New separated method
                newImplicitHintObject.color = CheckIfSequenceItemMatches(currentSequence, correctSequence, i, ref lastAction) ? CorrectColor : IncorrectColor;
                //I kind of want to test this method so this will make that way less painful


                //If this code above behaves weird, just comment it out and use this line just below
                //newImplicitHintObject.color = currentDirection == correctDirection ? CorrectColor : IncorrectColor;




                newImplicitHintObject.transform.position = Vector3.zero;
                BehaviourOrderList_Implicit.Add(newImplicitHintObject);
            }
            else
            {
                implicitHitCountReached = true;
            }

            //No need to keep looping if we have displayed all the hints we need to
            if (implicitHitCountReached && explicitHintCountReached) break;

            //overrideCorrectDirection = false; //Set this back to false each time
        }

        //Incase the user input more values than the sequence is
        for (int j = i; j < currentSequenceLength; j++)
        {
            Image newImplicitHintObject = Instantiate(ImageUiObjectPrefab, BehaviourOrderHints_Implicit.transform);
            newImplicitHintObject.color = IncorrectColor;
            newImplicitHintObject.transform.position = Vector3.zero;
            BehaviourOrderList_Implicit.Add(newImplicitHintObject);
        }


    }


    private bool CheckIfSequenceItemMatches(List<ECubeAction> currentSequence, List<ECubeAction> correctSequence, int index, ref ECubeAction? lastAction) 
    {
        bool result = false;

        ECubeAction correctDirection = correctSequence[index];
        ECubeAction currentDirection = currentSequence[index];


        bool correctDirectionIsLeftOrRight = correctDirection == ECubeAction.TurnRight || correctDirection == ECubeAction.TurnLeft;
        bool currentDirectionIsLeftOrRight = currentDirection == ECubeAction.TurnRight || currentDirection == ECubeAction.TurnLeft;

        if (correctDirectionIsLeftOrRight && currentDirectionIsLeftOrRight)
        {
            //Out of all the code, I think this might be the most likely to need testing
            //If there wasn't a last direction, there is another item in the correct sequence (after this one) and that item matches the current correct direction
            if (lastAction == null && index + 1 < correctSequence.Count && correctDirection == correctSequence[index + 1])
            {
                lastAction = currentDirection; //Set this as the last direction (we need this to match the last one)

                //Force this direction to always be correct!!!!!
                result = true;

            }
            else if (lastAction != null) //If there is a last action
            {
                //Check this against the current one and see if they match, if they do then it should be correct!
                result = lastAction == currentDirection;
                lastAction = null; //Null this as its the end of the 2 part sequence
            }
            else //If there isn't a last action then we do the normal check
            {
                //Behave normally!!!!!
                result = currentDirection == correctDirection;
            }


        }
        else //If the direction isn't either of those we do the normal thing
        {
            lastAction = null; //We also set this to null to break the sequence

            //Behave normally!!!!!
            result = currentDirection == correctDirection;
        }

        return result;
    }


    private void Test_QuickAndDirtyTest() 
    {
        List<ECubeAction> correct1 = new List<ECubeAction>() { ECubeAction.Forward, ECubeAction.Forward, ECubeAction.TurnLeft, ECubeAction.TurnLeft, ECubeAction.TurnRight, ECubeAction.TurnLeft, ECubeAction.Forward, ECubeAction.TurnLeft, ECubeAction.TurnLeft, ECubeAction.TurnRight, ECubeAction.TurnRight, ECubeAction.TurnRight, ECubeAction.TurnRight };
        List<ECubeAction> current1 = new List<ECubeAction>() { ECubeAction.Forward, ECubeAction.TurnRight, ECubeAction.TurnRight, ECubeAction.TurnRight, ECubeAction.TurnRight, ECubeAction.TurnRight, ECubeAction.Forward, ECubeAction.TurnRight, ECubeAction.TurnLeft, ECubeAction.Forward, ECubeAction.TurnRight, ECubeAction.Forward, ECubeAction.TurnLeft };
        //true, false, true, true, true, false, true
        List<bool> expected = new List<bool> { true, false, true, true, true, false, true, true, false , false, true, false, false};
        List<bool> result = new List<bool>();

        ECubeAction? lastAction = null;

        for (int i = 0; i < correct1.Count; i++) 
        {
            result.Add(CheckIfSequenceItemMatches(current1, correct1, i, ref lastAction));
        }

        print("expected: " + string.Join(",", expected));
        print("actual: " + string.Join(",", result));

        Assert.IsTrue(Test_ListsMatch(expected, result), $"expected: {string.Join(",", expected)}; actual {string.Join(",", result)}");
    }


    private bool Test_ListsMatch(List<bool> list1, List<bool> list2) 
    {
        if (list1.Count != list2.Count) return false;

        for (int i = 0; i < list1.Count; i++) 
        {
            if (list1[i] != list2[i]) return false;
        }

        return true;
    }
}
