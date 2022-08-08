using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterCubeHelper : MonoBehaviour
{
    public float TransparencyValue = 1f;
    public AudioClip[] soundEffects;

    [SerializeField]
    private MeshRenderer[] MeshRenderers;

    private Material matReference;

    private void Awake()
    {
        matReference = MeshRenderers[0].material;

        for (int i = 1; i < MeshRenderers.Length; i++) 
        {
            MeshRenderers[i].material = matReference;
        }
    }

    private void Update()
    {
        if (matReference.color.a != TransparencyValue) matReference.color = ChangeTransparencyValue(matReference.color, TransparencyValue);
    }

    private Color ChangeTransparencyValue(Color color, float newAlpha) 
    {
        color.a = newAlpha;
        return color;
    }

    public void PlaySound(int soundIndex) 
    {
        if (soundIndex < soundEffects.Length) transform.PlayClipAtTransform(soundEffects[soundIndex], false, 0.5f);
    }
}
