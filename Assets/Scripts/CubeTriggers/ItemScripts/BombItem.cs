using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombItem : PickupableItem
{
    public override PuzzleKeyType ObjectType => PuzzleKeyType.Bomb;

    protected override void OnValidPickup(CubeCharacter pickerUpper)
    {
    }
}
