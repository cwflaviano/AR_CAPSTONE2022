using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenConfig : MonoBehaviour
{
    // set screen resolution
    public Vector2 SetResolution(RawImage textureScreen, float scale, int rawImageWidth, int rawImageHeight)
    {
        textureScreen.transform.localScale = new Vector3(1f, -1f, 1f);
        if (scale < 0 || scale > 10) scale = 1;

        return textureScreen.rectTransform.sizeDelta = ScaleFactor(scale, rawImageWidth, rawImageHeight);
    }

    private Vector2 ScaleFactor(float scale, int rawImageWidth, int rawImageHeight)
    {
        float newWidth = rawImageWidth * scale;
        float newHeight = rawImageHeight * scale;
        return new Vector2(newWidth, newHeight);
    }

    public void SetScreenVisibility(RawImage screen)
    {
         screen.color = Color.white;
    }

    public void FlipScreenVertically(RawImage screen)
    {
        screen.transform.localScale = new Vector3(1f, -1f, 1f);
    }
}
