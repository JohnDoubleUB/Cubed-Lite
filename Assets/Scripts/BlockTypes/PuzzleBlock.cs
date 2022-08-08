using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class PuzzleBlock : MonoBehaviour
{
    [SerializeField]
    private AudioClip OnValidKeySoundEffect;

    [SerializeField]
    private float Volume = 0.5f;

    [SerializeField]
    private float Pitch = 1f;

    [SerializeField]
    private bool PitchVariation = true;

    [SerializeField]
    protected Collider bc;

    public abstract PuzzleKeyType RequiredKeyType { get; }

    public bool GiveKey(PickupableItem key, CubeCharacter cube) 
    {
        if (key.ObjectType == RequiredKeyType)
        {
            if (OnValidKeySoundEffect != null) transform.PlayClipAtTransform(OnValidKeySoundEffect, false, Volume, PitchVariation, 0, Pitch);
            OnValidKeyGiven(key, cube);
            return true;
        }
        else 
        {
            return false;
        }
    }

    protected abstract void OnValidKeyGiven(PickupableItem key, CubeCharacter cube);

    public virtual void OnFailedInteraction() { }

    protected void Reset()
    {
        bc = GetComponent<Collider>();
    }
}
