using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SizingAndMeasurement;

public class KinectManager : MonoBehaviour
{
    [Header("CORE KINECT SCRIPTS")]
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectSystem KinectSystem;
    [SerializeField] private KinectTracking KinectTracking;
    [SerializeField] private KinectStreams KinectStreams;
    [SerializeField] private SizingAndMeasurement SizingAndMeasurement;


    [Header("MODEL SCRIPTS")]
    [SerializeField] private KinectClothAugmenter KinectClothAugmenter;

    [Header("UI SCRIPTS")]
    [SerializeField] private CameraDisplay CameraDisplay;
    [SerializeField] private Debugging Debugging;
    [SerializeField] private TryDemo TryDemo;


    private void Awake()
    {
        if (KinectSystem == null)
        {
            Debug.LogError("[CRITICAL] KinectSystem is not assigned in the inspector");
            return;
        }
        if (KinectStreams == null)
        {
            Debug.LogError("[CRITICAL] KinectStreams is not assigned in the inspector");
            return;
        }
        if (CameraDisplay == null)
        {
            Debug.LogError("[CRITICAL] CameraDisplay is not assigned in the inspector");
            return;
        }
        if (KinectConfig == null)
        {
            Debug.LogError("[CRITICAL] KinectConfig is not assigned in the inspector");
            return;
        }
        if (KinectClothAugmenter == null)
        {
            Debug.LogError("[CRITICAL] KinectClothAugmenter is not assigned in the inspector");
            return;
        }

        KinectSystem.KinectInitialization();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CameraDisplay.ConfigureScreen();

        if(KinectSystem.IsInitialized())
        {
            CameraDisplay.DisplayCameraFeed(KinectStreams); // display camera feed

            // position model container and model
            KinectClothAugmenter.DefaultPositionModelContainer();
            KinectClothAugmenter.DefaultPositionModel();
        }
    }


    private void Update()
    {
        if (KinectSystem.IsInitialized())
        {
            // Adjust kinect
            TryDemo.AdjustAngle();  

            // camera display settings 
            CameraDisplay.SetResolution();
            CameraDisplay.UpdateXPOS();
            // kinect sensor angle setting
            KinectSystem.SetKinectSensorElevationAngle(KinectConfig.SensorAngle);

            // checks if raw image resolution changed
            if (CameraDisplay.isScreenResolutionUpdated)
                Debug.Log($"[LOG] New Resolution: {(int)CameraDisplay.ScreenResolution.x} x {(int)CameraDisplay.ScreenResolution.y}");

            // keeps checking if will use camera feed and depth sensing
            KinectStreams.UseUserMap();
            KinectStreams.UseColorMap();

            
            if(KinectConfig.StartTracking)
            {
                KinectTracking.PollSkeleton(); // body tracking logic

                if (KinectConfig.userCalibrated)
                {
                    KinectClothAugmenter.PositionModel(KinectConfig.userID); // update position per frame scaling
                    // logic here to augment model to use boy please...
                }
                else
                {
                    // reposition model 
                    KinectClothAugmenter.DefaultPositionModelContainer();
                    KinectClothAugmenter.DefaultPositionModel();
                }

            }

        }
    }
}
