using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LockBlock : PuzzleBlock
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform keyParent;

    public override PuzzleKeyType RequiredKeyType => PuzzleKeyType.Key;

    protected override void OnValidKeyGiven(PickupableItem key, CubeCharacter cube)
    {
        key.currentCube = null;
        bc.enabled = false;
        key.transform.SetParent(keyParent);
        key.transform.localPosition = Vector3.zero;
        key.transform.localRotation = Quaternion.identity;
        animator.Play("Unlock", 0);
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
