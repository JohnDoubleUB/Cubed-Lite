using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager current;

    [EnumFlags]
    public EUIContext EnabledContextsFromStartup;


    //public GroupUIContext t;
    public List<SerializedPair<EUIContext, UIContextObject>> UIContexts = new List<SerializedPair<EUIContext, UIContextObject>>();

    [SerializeField]
    public Dictionary<EUIContext, List<UIContextObject>> _UIContexts = new Dictionary<EUIContext, List<UIContextObject>>();

    public void SetActiveContexts(bool active, params EUIContext[] contexts)
    {
        foreach (EUIContext context in contexts)
        {
            if (_UIContexts.ContainsKey(context))
            {
                foreach (UIContextObject contextObject in _UIContexts[context]) contextObject.SetDisplayAndActive(active);
            }
        }
    }

    public void SetActiveContexts(bool active, bool immediate, params EUIContext[] contexts)
    {
        foreach (EUIContext context in contexts)
        {
            if (_UIContexts.ContainsKey(context))
            {
                foreach (UIContextObject contextObject in _UIContexts[context]) contextObject.SetDisplayAndActive(active, immediate);
            }
        }
    }

    public void SetToggleActiveContexts(params EUIContext[] contexts)
    {
        foreach (EUIContext context in contexts)
        {
            if (_UIContexts.ContainsKey(context))
            {
                foreach (UIContextObject contextObject in _UIContexts[context]) contextObject.SetDisplayAndActive(!contextObject.Display);
            }
        }
    }

    public void InitializeContexts() 
    {
        List<EUIContext> enabledContexts = ReturnEnabledContextsFromStartup();
        
        foreach (KeyValuePair<EUIContext, List<UIContextObject>> kVP in _UIContexts) 
        {
            bool contextEnabled = enabledContexts.Contains(kVP.Key);
            foreach (UIContextObject contextObject in kVP.Value) contextObject.SetDisplayAndActive(contextEnabled, true); 
        }
    }


    public void Test_ToggleMainMenu()
    {
        SetToggleActiveContexts(EUIContext.MainMenu);
    }

    public void Test_ToggleLevelMenu()
    {
        SetToggleActiveContexts(EUIContext.LevelMenu);
    }

    private void Start()
    {
        InitializeContexts();
        SetActiveContexts(false, EUIContext.InitialFade);
    }

    private List<EUIContext> ReturnEnabledContextsFromStartup()
    {

        List<EUIContext> selectedElements = new List<EUIContext>();
        for (int i = 0; i < System.Enum.GetValues(typeof(EUIContext)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)EnabledContextsFromStartup & layer) != 0)
            {
                selectedElements.Add((EUIContext)i);
            }
        }

        return selectedElements;
    }

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    private void OnValidate()
    {
        _UpdateUIContexts();
    }

    private void OnEnable()
    {
        _UpdateUIContexts();
    }

    private void Reset()
    {
        _ClearUIContexts();
    }

    private void _ClearUIContexts() 
    {
        UIContexts.Clear();
        _UIContexts.Clear();
    }

    private void _UpdateUIContexts()
    {
        _UIContexts.Clear(); //Clear the existing context incase there is a drastic change to things

        foreach (SerializedPair<EUIContext, UIContextObject> sKVP in UIContexts) //Go through the new context and load this into a nice dictionary
        {
            if (sKVP.Value == null) continue;


            if (_UIContexts.ContainsKey(sKVP.Key)) //If key already exists then add the value into the existing dictionary list
            {
                _UIContexts[sKVP.Key].Add(sKVP.Value);
            }
            else //If not then create a whole new object in the dictionary and then assign the current canvas group into that context
            {
                _UIContexts.Add(sKVP.Key, new List<UIContextObject> { sKVP.Value });
            }
        }
    }
}


//[System.Flags]
public enum EUIContext 
{
    MainMenu = 0,
    LevelMenu = 1,
    LevelCompleteMenu = 2,
    InitialFade = 3,
    CreditsMenu = 4,
    TutorialMenu = 5,
    HintMenu = 6,
    AdPermissionMenu = 7,
    Notification = 8,
    ExplicitHint = 9
}