using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : UIBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public int Level 
    {
        get { return level; }
        set 
        {
            if (value != level) 
            {
                level = value;
                ButtonText.text = level.ToString();
            }
        }
    }

    [SerializeField]
    private int level;
    
    
    
    public bool IsUnlocked 
    {
        get 
        {
            return isUnlocked;
        }

        set 
        {
            isUnlocked = value;
            foreach (MaskableGraphic mG in graphicsEffectedOnClick) mG.color = isUnlocked ? Color.white : DisabledColor;
        }
    }
    [SerializeField]
    private bool isUnlocked;

    [SerializeField]
    private MaskableGraphic[] graphicsToFade;

    [SerializeField]
    private MaskableGraphic[] graphicsEffectedOnClick;

    [SerializeField]
    private Text ButtonText;

    [SerializeField]
    private Color OnClickColor;

    [SerializeField]
    private Color DisabledColor;

    public float textFadeSpeed = 4f;
    private float currentTextTransparencyValue = 0f;
    private bool isHovered;

    protected override void OnEnable()
    {
        isHovered = false;
        currentTextTransparencyValue = 0f;
    }

    protected override void Awake()
    {
        foreach (MaskableGraphic mG in graphicsEffectedOnClick) mG.color = isUnlocked ? Color.white : DisabledColor;
        ButtonText.text = level.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isUnlocked) SetButtonClicked(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isUnlocked)
        {
            SetButtonClicked(false);
            if (isHovered) OnConfirmedClick();
        }
    }

    private void SetButtonClicked(bool clicked) 
    {
        foreach (MaskableGraphic mG in graphicsEffectedOnClick) 
        {
            mG.color = clicked ? OnClickColor : Color.white;
        }
    }

    private void OnConfirmedClick() 
    {
        GameManager.current.Button_LoadLevel(level);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isUnlocked) isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(isUnlocked) isHovered = false;
    }

    private void Update()
    {
        bool valueHasChanged = false;
        if (isHovered && currentTextTransparencyValue != 1f)
        {
            currentTextTransparencyValue = Mathf.Min(currentTextTransparencyValue + (Time.unscaledDeltaTime * textFadeSpeed), 1f);
            valueHasChanged = true;
        }
        else if (!isHovered && currentTextTransparencyValue != 0f) 
        {
            currentTextTransparencyValue = Mathf.Max(currentTextTransparencyValue - (Time.unscaledDeltaTime * textFadeSpeed), 0f);
            valueHasChanged = true;
        }

        if (valueHasChanged) 
        {
            foreach (MaskableGraphic mG in graphicsToFade) mG.color = new Color(mG.color.r, mG.color.g, mG.color.b, currentTextTransparencyValue);
        }

    }

    protected override void OnDisable()
    {
        currentTextTransparencyValue = 0f;
        foreach (MaskableGraphic mG in graphicsToFade) mG.color = new Color(mG.color.r, mG.color.g, mG.color.b, currentTextTransparencyValue);
    }
}
