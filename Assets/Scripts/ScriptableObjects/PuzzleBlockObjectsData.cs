using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleBlockObjectsData", menuName = "ScriptableObjects/PuzzleBlockObjectsData", order = 1)]
public class PuzzleBlockObjectsData : ScriptableObject
{
    public PuzzleBlock[] PuzzleBlockObjects;

    [ReadOnly]
    public Dictionary<PuzzleKeyType, PuzzleBlock> Dictionary = new Dictionary<PuzzleKeyType, PuzzleBlock>();

    [ReadOnly]
    public List<PuzzleKeyType> ContainedKeys = new List<PuzzleKeyType>();

    private void OnValidate()
    {
        Dictionary.Clear();
        ContainedKeys.Clear();

        foreach (PuzzleBlock pBO in PuzzleBlockObjects)
        {
            if (pBO != null && !Dictionary.ContainsKey(pBO.RequiredKeyType))
            {
                Dictionary.Add(pBO.RequiredKeyType, pBO);
                ContainedKeys.Add(pBO.RequiredKeyType);
            }

        }
    }

    private void OnEnable()
    {
        Dictionary.Clear();
        ContainedKeys.Clear();

        foreach (PuzzleBlock pBO in PuzzleBlockObjects)
        {
            if (pBO != null && !Dictionary.ContainsKey(pBO.RequiredKeyType))
            {
                Dictionary.Add(pBO.RequiredKeyType, pBO);
                ContainedKeys.Add(pBO.RequiredKeyType);
            }

        }
    }
}
