using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCloner : CubeTrigger
{
    [SerializeField]
    private ParticleSystem particleEffectForTarget;

    [SerializeField]
    private Transform cloneDestination;
    private bool clonerUsed;

    protected override bool OnCubeEnter(CubeCharacter cube)
    {
        if (!clonerUsed && cloneDestination != null) 
        {
            CubeManager.current.CreateCubeAt(cloneDestination.position, cloneDestination.rotation);
            if (particleEffectForTarget != null) 
            { 
                particleEffectForTarget.transform.position = cloneDestination.position - new Vector3(0, 1f, 0);
                particleEffectForTarget.Play();
            }
            clonerUsed = true;
            return true;
        }
        return false;
    }

    protected override bool OnCubeExit(CubeCharacter cube)
    {
        return false;
    }

    public void ResetCloner() 
    {
        clonerUsed = false;
    }
}
