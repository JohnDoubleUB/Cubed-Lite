using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleLevelObject : MonoBehaviour
{
    public bool autoLoadPuzzle = true;


    public float CameraProjectionSize = 11;
    public int MaxScore = 2000;
    public int RetryScorePenalty = 250;
    public int MinimumScore = 1000;

    //PuzzleObjectManager
    [Header("Puzzle Object Manager")]
    public List<PickupableItem> PickupableObjects = new List<PickupableItem>();
    public List<PuzzleBlock> LockBlockObjects = new List<PuzzleBlock>();
    public List<CubeCloner> Cloners = new List<CubeCloner>();
    public List<GoalTrigger> Goals = new List<GoalTrigger>();
    public List<ToggleButtonTrigger> Toggles = new List<ToggleButtonTrigger>();

    //PuzzleManager
    [Header("Puzzle Manager")]
    public bool AnyGoalsCountAsWin;

    //CubeManager
    [Header("Cube Manager")]
    public List<CubeCharacter> ActiveCubes;

    [Header("Solution")]
    public List<ECubeAction> Solution;


    private void Start()
    {
        if (autoLoadPuzzle) GameManager.current.InitializeLevel(this);
    }


    private void Reset()
    {
        PickupableObjects = gameObject.GetComponentsInChildren<PickupableItem>().ToList();
        LockBlockObjects = gameObject.GetComponentsInChildren<PuzzleBlock>().ToList();
        Cloners = GetComponentsInChildren<CubeCloner>().ToList();
        Goals = GetComponentsInChildren<GoalTrigger>().ToList();
        ActiveCubes = GetComponentsInChildren<CubeCharacter>().ToList();
        Toggles = GetComponentsInChildren<ToggleButtonTrigger>().ToList();
    }
}
