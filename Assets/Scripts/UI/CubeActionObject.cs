using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeActionObject : MonoBehaviour
{
    public ECubeAction Action;
    public int ActionIndex;

    public Color failColor;
    public Color successColor;
    public Color activeColor;
    private Color defaultColor;

    [SerializeField]
    private Button button;

    [SerializeField]
    private Image icon;

    private void Start()
    {
        SetDefaultColor();
    }

    public void SetDefaultColor() 
    {
        defaultColor = icon.color;
    }

    public void ButtonAction()
    {
        PuzzleManager.current.RemoveActionAtIndex(ActionIndex);
        Destroy(gameObject);
    }

    public void SetIcon(Sprite setSprite)
    {
        icon.sprite = setSprite;
    }

    public void SetInteractable(bool enabled) 
    {
        button.interactable = enabled;
    }

    public void MarkAsFail() 
    {
        icon.color = failColor;
    }

    public void MarkAsSuccess() 
    {
        icon.color = successColor;
    }

    public void ResetUI()
    {
        icon.color = defaultColor;
    }

    public void MarkAsActive() 
    {
        icon.color = activeColor;
    }
}
