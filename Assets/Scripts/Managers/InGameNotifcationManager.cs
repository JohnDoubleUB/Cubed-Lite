using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameNotifcationManager : MonoBehaviour
{
    public static InGameNotifcationManager current;

    [SerializeField]
    private float messageVisibilityTime = 2f;

    private float currentVisibilityTime = 0f;

    [SerializeField]
    private Text notificationText;

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    public void Notify(string notificationString) 
    {
        currentVisibilityTime = 0f;
        notificationText.text = notificationString;
        
        //Show the ui
        UIManager.current.SetActiveContexts(true, EUIContext.Notification);
    }

    private void Update()
    {
        if (currentVisibilityTime < messageVisibilityTime)
        {
            currentVisibilityTime += Time.deltaTime;

            if (!(currentVisibilityTime < messageVisibilityTime))
            {
                UIManager.current.SetActiveContexts(false, EUIContext.Notification);
            }
        }


    }

}
