using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KinectManager : MonoBehaviour
{
    [Header("CORE KINECT SCRIPTS")]
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectSystem KinectSystem;
    [SerializeField] private KinectTracking KinectTracking;
    [SerializeField] private KinectStreams KinectStreams;
    [SerializeField] private ClothManager ClothManager;

    [Header("UI SCRIPTS")]
    [SerializeField] private CameraDisplay CameraDisplay;

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
        if (ClothManager == null)
        {
            Debug.LogError("[CRITICAL] ClothManager is not assigned in the inspector");
            return;
        }
        
        KinectSystem.KinectInitialization();
    }

    private void Start()
    {
        CameraDisplay.ConfigureScreen();

        if(KinectSystem.IsInitialized())
        {
            CameraDisplay.DisplayCameraFeed(KinectStreams);
        }
    }


    private void Update()
    {
        if (KinectSystem.IsInitialized())
        {
            CameraDisplay.SetResolution();
            KinectSystem.SetKinectSensorElevationAngle(KinectConfig.SensorAngle);

            if (CameraDisplay.isScreenResolutionUpdated)
                Debug.Log($"[LOG] New Resolution: {(int)CameraDisplay.ScreenResolution.x} x {(int)CameraDisplay.ScreenResolution.y}");

            KinectStreams.UseUserMap();
            KinectStreams.UseColorMap();
            KinectTracking.PollSkeleton();

            //if(!ClothManager.isConfigured)
            //{
            //    ClothManager.ManageModels();
            //}
        }
    }
}
