using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NoneBlock : PuzzleBlock
{
    [SerializeField]
    private Animator animator;

    public override PuzzleKeyType RequiredKeyType => PuzzleKeyType.None;

    protected override void OnValidKeyGiven(PickupableItem key, CubeCharacter cube)
    {
    }
    public override void OnFailedInteraction()
    {
        if (animator != null) animator.Play("Blocked");
    }

    protected new void Reset()
    {
        base.Reset();
        animator = GetComponent<Animator>();
    }
}
