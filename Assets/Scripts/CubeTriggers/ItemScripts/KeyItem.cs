using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : PickupableItem
{
    public override PuzzleKeyType ObjectType => PuzzleKeyType.Key;

    protected override void OnValidPickup(CubeCharacter pickerUpper)
    {
    }
}
