using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TransparencyTest : MonoBehaviour
{
    public bool display;
    public float fadeSpeed = 3f;

    private float currentTransparency = 0f;


    [SerializeField]
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        currentTransparency = display ? 1f : 0f;
        canvasGroup.alpha = currentTransparency;
    }

    // Update is called once per frame
    void Update()
    {
        bool transparencyHasChanged = false;
        if (!display && currentTransparency != 0f)
        {
            currentTransparency = Mathf.Max(currentTransparency - (Time.deltaTime * fadeSpeed), 0f);
            transparencyHasChanged = true;
        }
        else if (display && currentTransparency != 1f) 
        {
            currentTransparency = Mathf.Min(currentTransparency + (Time.deltaTime * fadeSpeed), 1f);
            transparencyHasChanged = true;
        }


        if (transparencyHasChanged) canvasGroup.alpha = currentTransparency;
    }

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
}
