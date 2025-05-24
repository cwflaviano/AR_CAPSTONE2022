using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TryDemo : MonoBehaviour
{
    [SerializeField] private GameObject slider;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private KinectSystem kinectSystem;

    public void AdjustAngle()
    {
        int angle = 0;
        Slider sld = slider.GetComponent<Slider>();

        if (sld.value > 27 || sld.value < -27) angle = 0;
        angle = (int)sld.value;
        kinectSystem.SetKinectSensorElevationAngle(angle);
        text.text = angle.ToString() + " Deg";
    }


    public void LoadARScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
