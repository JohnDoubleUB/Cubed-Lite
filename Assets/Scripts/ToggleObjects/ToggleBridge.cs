using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Animator))]
public class ToggleBridge : ToggleObject
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Collider bridgeCollider;

    public override bool ToggleCurrent => bridgeCollider.enabled;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        bridgeCollider = GetComponent<Collider>();
    }

    public override void Toggle(bool toggle, bool immediate = false) 
    {
        bridgeCollider.enabled = toggle;

        if (!immediate) animator.Play(toggle ? "Up" : "Down", 0);
        else animator.Play(toggle ? "Idle_Up" : "Idle_Down", 0);
    }
}
