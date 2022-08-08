using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CustomCreditsDebugButton : UIBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    private float timeToTrigger = 10f;

    [SerializeField]
    [ReadOnly]
    private float currentDownTime = 0f;


    [SerializeField]
    private Text creditsText;
    
    [SerializeField]
    private Color normalColor;
    [SerializeField]
    private Color downColor;
    [SerializeField]
    private Color triggeredColor;

    private bool isCurrentlyDown;

    private bool toggle = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        isCurrentlyDown = true;
        currentDownTime = 0f;
        creditsText.color = downColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isCurrentlyDown = false;
        currentDownTime = 0f;
        creditsText.color = normalColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isCurrentlyDown = false;
        currentDownTime = 0f;
        creditsText.color = normalColor;
    }

    private void Update()
    {
        if (isCurrentlyDown) 
        {
            currentDownTime += Time.deltaTime;
            if (currentDownTime > timeToTrigger) 
            {
                GameManager.current.EnableDebugFeatures(toggle);
                GameManager.current.EnableTestingFeatures(toggle);
                if (creditsText != null) creditsText.color = triggeredColor;
                toggle = !toggle;
                currentDownTime = 0f;
                isCurrentlyDown = false;
            }
        }
    }

    private new void Reset()
    {
        creditsText = GetComponent<Text>();
        normalColor = creditsText.color;
    }
}
