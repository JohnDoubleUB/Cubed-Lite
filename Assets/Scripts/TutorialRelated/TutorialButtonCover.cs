using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonCover : MonoBehaviour
{
    [SerializeField]
    private Image InteractableImage;

    [SerializeField]
    private GameObject Indicator;

    public void SetInteractableAndIndicator(bool IsInteractable = false, bool IsHighlighted = false) 
    {
        InteractableImage.enabled = !IsInteractable;
        Indicator.SetActive(IsHighlighted);
    }
}
