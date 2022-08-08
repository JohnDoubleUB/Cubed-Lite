using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ToggleAudioButton : UIBehaviour, IEventSystemHandler, IPointerDownHandler
{
    [SerializeField]
    private Sprite AudioOnImage;
    [SerializeField]
    private Sprite AudioOffImage;

    [SerializeField]
    private Image SpeakerImageObject;

    [SerializeField]
    private Image CrossImage;

    protected override void Start()
    {
        UpdateButtonStatus(AudioManager.current.MusicEnabled);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateButtonStatus(AudioManager.current.ToggleMusic());
    }

    private void UpdateButtonStatus(bool enable) 
    {
        if (enable) 
        {
            SpeakerImageObject.sprite = AudioOnImage;
            CrossImage.enabled = false;
        }
        else 
        {
            SpeakerImageObject.sprite = AudioOffImage;
            CrossImage.enabled = true;
        }
    }


    protected new void Reset()
    {
        SpeakerImageObject = GetComponent<Image>();
    }
}
