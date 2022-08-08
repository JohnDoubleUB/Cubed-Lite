using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : PuzzleBlock
{
    public override PuzzleKeyType RequiredKeyType => PuzzleKeyType.Bomb;

    public Animator animator;

    protected override void OnValidKeyGiven(PickupableItem key, CubeCharacter cube)
    {
        key.currentCube = null;
        bc.enabled = false;
        Destroy(key.gameObject);
        animator.Play("Explode");
        cube.QueueForDestruction();
        PuzzleObjectManager.current.CreateExplosionAtLocation(transform.position);
    }

    public override void OnFailedInteraction()
    {
        if (animator != null) animator.Play("Blocked");
    }
}
