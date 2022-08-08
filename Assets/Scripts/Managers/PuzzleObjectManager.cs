using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleObjectManager : MonoBehaviour
{
    public static PuzzleObjectManager current;

    //[SerializeField]
    public PuzzleBlockObjectsData puzzleBlockObjectsData;
    //[SerializeField]
    public PickupableItemData keyObjectsData;

    [SerializeField]
    private List<PickupableItem> currentPickupableObjects = new List<PickupableItem>();
    private List<KeyAndTransformData> pickupableObjectTransforms = new List<KeyAndTransformData>();

    [SerializeField]
    private List<PuzzleBlock> currentLockBlockObjects = new List<PuzzleBlock>();
    private List<KeyAndTransformData> lockBlockObjectTransforms = new List<KeyAndTransformData>();

    public Dictionary<string, PuzzleBlock> AssociatedBlockObjects = new Dictionary<string, PuzzleBlock>();

    [SerializeField]
    private List<CubeCloner> Cloners = new List<CubeCloner>();

    [SerializeField]
    private List<GoalTrigger> Goals = new List<GoalTrigger>();

    [SerializeField]
    private List<ToggleButtonTrigger> Toggles = new List<ToggleButtonTrigger>();


    [SerializeField]
    private GameObject explosionPrefab;

    private Transform levelParent;
    public bool AnyGoalsCovered
    {
        get
        {
            foreach (GoalTrigger gT in Goals)
            {
                if (gT.GoalCovered) return true;
            }
            return false;
        }
    }

    public bool AllGoalsCovered
    {
        get
        {
            foreach (GoalTrigger gT in Goals)
            {
                if (!gT.GoalCovered) return false;
            }
            return true;
        }
    }

    public List<GoalTrigger> AllCurrentGoals 
    {
        get { return Goals; }
    }

    public void CreateExplosionAtLocation(Vector3 position)
    {
        if (explosionPrefab != null) Instantiate(explosionPrefab, position, Quaternion.identity);
    }

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    public void DestroyAllExisting() 
    {
        foreach (PickupableItem kO in currentPickupableObjects) if (kO != null) Destroy(kO.gameObject);
        foreach (PuzzleBlock lBO in currentLockBlockObjects) if (lBO != null) Destroy(lBO.gameObject);
        currentPickupableObjects.Clear();
        currentLockBlockObjects.Clear();
    }


    public void InitializePuzzleObjects(PuzzleLevelObject levelObject, bool destroyAllExisting = false) 
    {
        levelParent = levelObject.gameObject.transform;

        

        if (destroyAllExisting)
        {
            DestroyAllExisting();
        }


        currentPickupableObjects = levelObject.PickupableObjects;
        currentLockBlockObjects = levelObject.LockBlockObjects;
        
        //Clear Associated blocks
        AssociatedBlockObjects.Clear();

        //Clear transforms and data for resetting levels
        pickupableObjectTransforms.Clear();
        lockBlockObjectTransforms.Clear();

        //Store pickupable objects
        foreach (PickupableItem kO in currentPickupableObjects)
        {
            pickupableObjectTransforms.Add(new KeyAndTransformData() { Position = kO.transform.position, Rotation = kO.transform.rotation, KeyType = kO.ObjectType });
        }

        //Store blocks
        foreach (PuzzleBlock lBO in currentLockBlockObjects)
        {
            lockBlockObjectTransforms.Add(new KeyAndTransformData() { Position = lBO.transform.position, Rotation = lBO.transform.rotation, KeyType = lBO.RequiredKeyType });
            AssociatedBlockObjects.Add(lBO.name, lBO);
        }

        Cloners = levelObject.Cloners;
        Goals = levelObject.Goals;
        Toggles = levelObject.Toggles;


        //TutorialManager.current.SetTutorial(ETutorial.Intro);
        TutorialCheck();
    }

    private bool ShouldPlayIntroTutorial 
    {
        get { return GameManager.current.TotalDaysSinceLastSave > 30 || GameManager.current.LatestLevelInSaveData < 1 && GameManager.current.LevelToLoad < 10; }
    }

    private void TutorialCheck() 
    {
        //Check if we should play the intro tutorial
        if(ShouldPlayIntroTutorial)TutorialManager.current.SetTutorial(ETutorial.Intro);
    }

    private void InitializePuzzleOLD() 
    {
        pickupableObjectTransforms.Clear();

        foreach (PickupableItem kO in currentPickupableObjects)
        {
            pickupableObjectTransforms.Add(new KeyAndTransformData() { Position = kO.transform.position, Rotation = kO.transform.rotation, KeyType = kO.ObjectType });
        }

        lockBlockObjectTransforms.Clear();
        AssociatedBlockObjects.Clear();

        foreach (PuzzleBlock lBO in currentLockBlockObjects)
        {
            lockBlockObjectTransforms.Add(new KeyAndTransformData() { Position = lBO.transform.position, Rotation = lBO.transform.rotation, KeyType = lBO.RequiredKeyType });
            AssociatedBlockObjects.Add(lBO.name, lBO);
        }
    }


    public void ResetPuzzleObjects() 
    {
        foreach (PickupableItem kO in currentPickupableObjects) if (kO != null) Destroy(kO.gameObject);
        currentPickupableObjects.Clear();
        foreach (PuzzleBlock lBO in currentLockBlockObjects) if (lBO != null) Destroy(lBO.gameObject);
        currentLockBlockObjects.Clear();
        AssociatedBlockObjects.Clear();

        foreach (ToggleButtonTrigger toggleObject in Toggles) toggleObject.OnPuzzleReset();

        for (int i = 0; i < lockBlockObjectTransforms.Count; i++) 
        {
            KeyAndTransformData currentTransformData = lockBlockObjectTransforms[i];
            PuzzleBlock newBlock = Instantiate(puzzleBlockObjectsData.Dictionary[currentTransformData.KeyType], currentTransformData.Position, currentTransformData.Rotation, levelParent);
            newBlock.name = newBlock.name + "_" + i;
            currentLockBlockObjects.Add(newBlock);
            AssociatedBlockObjects.Add(newBlock.name, newBlock);
        }


        for (int i = 0; i < pickupableObjectTransforms.Count; i++)
        {
            KeyAndTransformData currentTransformData = pickupableObjectTransforms[i];
            PickupableItem newBlock = Instantiate(keyObjectsData.Dictionary[currentTransformData.KeyType], currentTransformData.Position, currentTransformData.Rotation, levelParent);
            newBlock.name = newBlock.name + "_" + i;
            currentPickupableObjects.Add(newBlock);
        }


        foreach (CubeCloner cc in Cloners) cc.ResetCloner();

        foreach (GoalTrigger gT in Goals) gT.GoalCovered = false;
    }

}

public class KeyAndTransformData : TransformData 
{
    public PuzzleKeyType KeyType;
}