using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class PickupableItem : CubeTrigger
{
    public CubeCharacter currentCube;
    public float movementSpeed = 5;
    public Vector3 Offset = new Vector3(0, 1, 0);
    private Vector3 trueOffset;
    public abstract PuzzleKeyType ObjectType { get; }

    [SerializeField]
    private Collider bc;

    private void Reset()
    {
        bc = GetComponent<Collider>();
    }

    protected override bool OnCubeEnter(CubeCharacter cube)
    {
        if (currentCube == null)
        {
            currentCube = cube;
            SetTrueOffset(currentCube.AddKeyObject(this));
            OnValidPickup(cube);
            return true;
        }

        return false;
    }

    protected abstract void OnValidPickup(CubeCharacter pickerUpper);

    protected override bool OnCubeExit(CubeCharacter cube) { return false; }

    private void Update()
    {
        if (currentCube != null) 
        {
            float speedStep = movementSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, currentCube.transform.position + trueOffset, speedStep);
            if (bc.enabled) bc.enabled = false;
        }
    }


    public void SetTrueOffset(float offsetAmount) 
    {
        trueOffset = Offset * offsetAmount;
    }
}

public enum PuzzleKeyType 
{
    Key,
    Bomb,
    None
}