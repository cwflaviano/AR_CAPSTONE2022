using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Debugging : MonoBehaviour
{
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectTracking KinectTracking;
    [SerializeField] private TopGarments TopGarments;

    private GameObject currentActiveModel;

    [Header("BODY TRACKING DEBUGGING")]
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;
    public TextMeshProUGUI text5;
}
