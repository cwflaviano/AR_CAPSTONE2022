using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayDateTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dateTimeText;
    private string dateStringFormat = "dddd, MM dd, yyyy";
    private string timeStringFormat = "hh:mm:ss tt";

    private void Update()
    {
        if(dateTimeText != null)
            dateTimeText.text = DateTime.Now.ToString(dateStringFormat) +" - "+ DateTime.Now.ToString(timeStringFormat);
    }
}
