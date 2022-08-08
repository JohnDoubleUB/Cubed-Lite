using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class CustomLogoButton : UIBehaviour, IEventSystemHandler, IPointerDownHandler
{
    [SerializeField]
    private int TouchCountToReachCredits = 10;
    [SerializeField]
    private int currentTouchCount;

    [SerializeField]
    private float touchTimerReset = 2f;
    private float timeSinceLastTouch = 0f;

    [SerializeField]
    private Animator animator;

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.Play("Touch", 1, 0);
        currentTouchCount++;
        timeSinceLastTouch = 0;
        if (currentTouchCount == TouchCountToReachCredits) 
        {
            UIManager.current.SetActiveContexts(true, EUIContext.CreditsMenu);
        }
    }

    private void Update()
    {
        if (currentTouchCount > 0) 
        {
            timeSinceLastTouch += Time.deltaTime;
            if (timeSinceLastTouch > touchTimerReset) 
            {
                currentTouchCount = 0;
            }
        }
    }

    protected new void Reset()
    {
        animator = GetComponent<Animator>();
    }
}
