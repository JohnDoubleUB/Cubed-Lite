using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            print("test screen shot capturing");
            ScreenCapture.CaptureScreenshot("testScreenShot", 1);
        }
    }
}
