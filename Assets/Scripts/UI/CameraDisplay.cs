using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraDisplay : MonoBehaviour
{
    public ScreenConfig ScreenConfig;

    public RawImage screen;
    [SerializeField] private float scale = 1.69f;
    [SerializeField] public int rawImageWidth = 640;
    [SerializeField] public int rawImageHeight = 480;

    [HideInInspector] public Vector2 ScreenResolution;
    [HideInInspector] public bool isScreenResolutionUpdated = false;

    private float prevScale = 0f;

    // set and configure screen rawimage
    public void ConfigureScreen()
    {
        ScreenConfig.SetScreenVisibility(screen);
        ScreenConfig.FlipScreenVertically(screen);
        SetResolution();
    }

    // set screen rawimage resolution
    public void SetResolution()
    {
        if (scale != prevScale)
        {
            prevScale = scale;
            isScreenResolutionUpdated = true;
            ScreenResolution = ScreenConfig.SetResolution(screen, scale, rawImageWidth, rawImageHeight);
            return;
        }
        else
            isScreenResolutionUpdated = false;
    }

    // set and display camera feed
    public void DisplayCameraFeed(KinectStreams kinectStreams)
    {
        Texture texture = kinectStreams.GetUsersClrTex();
        screen.texture = texture;
    }

    // set to default if encountered error from kinect device
    public void KinectError()
    {
        Vector2 defaultRes = new Vector2(1080f, 1920f);
        screen.texture = null;
        screen.rectTransform.sizeDelta = defaultRes;
        screen.transform.localScale = new Vector3(1f, 1f, 1f);
        screen.color = Color.black;
    }
}
