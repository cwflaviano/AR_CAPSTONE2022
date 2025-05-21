using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrackingStatus : MonoBehaviour
{
    public TextMeshProUGUI textUI1;
    public TextMeshProUGUI textUI2;
    public TextMeshProUGUI textUI3;

    public GameObject debuggingText;

    private float delay = 2f;
    private float timer = 0f;
    private bool hasUpdated = false;

    public void OnStartDisableDebuggingUI()
    {
        debuggingText.SetActive(false);
    }

    public void DisableTrackingStatus()
    {
        if(debuggingText != null && debuggingText.activeSelf)
            debuggingText.SetActive(false);
        else
            debuggingText.SetActive(true);
    }

    public void TrackingStatusUI(string text)
    {
        if (textUI1 == null && textUI2 == null && textUI3 == null) return;

        if (!hasUpdated)
        {
            timer += Time.deltaTime;

            if (timer >= delay)
            {
                textUI1.text = text;
                hasUpdated = true;
            }
        }
    }
}
