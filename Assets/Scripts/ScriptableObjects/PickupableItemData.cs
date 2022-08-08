using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickupableItemData", menuName = "ScriptableObjects/PickupableItemData", order = 1)]
public class PickupableItemData : ScriptableObject
{
    public PickupableItem[] PickupableItems;

    [ReadOnly]
    public Dictionary<PuzzleKeyType, PickupableItem> Dictionary = new Dictionary<PuzzleKeyType, PickupableItem>();

    [ReadOnly]
    public List<PuzzleKeyType> ContainedKeys = new List<PuzzleKeyType>();

    private void OnValidate()
    {
        Dictionary.Clear();
        ContainedKeys.Clear();

        foreach (PickupableItem pI in PickupableItems)
        {
            if (pI != null && !Dictionary.ContainsKey(pI.ObjectType))
            {
                Dictionary.Add(pI.ObjectType, pI);
                ContainedKeys.Add(pI.ObjectType);
            }

        }
    }

    private void OnEnable()
    {
        Dictionary.Clear();
        ContainedKeys.Clear();

        foreach (PickupableItem pI in PickupableItems)
        {
            if (pI != null && !Dictionary.ContainsKey(pI.ObjectType))
            {
                Dictionary.Add(pI.ObjectType, pI);
                ContainedKeys.Add(pI.ObjectType);
            }

        }
    }
}
