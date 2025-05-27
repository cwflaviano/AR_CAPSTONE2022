using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class KinectManager : MonoBehaviour
{
    [Header("CORE KINECT SCRIPTS")]
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectSystem KinectSystem;
    [SerializeField] private KinectTracking KinectTracking;
    [SerializeField] private KinectStreams KinectStreams;


    [Header("MODEL SCRIPTS")]
    [SerializeField] private KinectClothAugmenter KinectClothAugmenter;
    [SerializeField] private SceneController SceneController;

    [Header("UI SCRIPTS")]
    [SerializeField] private Button BackToMainMenu;
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
        if (KinectTracking == null)
        {
            Debug.LogError("[CRITICAL] KinectTracking is not assigned in the inspector");
            return;
        }
        
        // Initialze kinect
        KinectSystem.KinectInitialization();
    }

    private void Start()
    {
        CameraDisplay.ConfigureScreen();
        BackToMainMenu.onClick.AddListener(() => { SceneController.OpenMainMenuScene(); });

        if (KinectSystem.IsInitialized())
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
                    // augmention
                    KinectClothAugmenter.PositionModel(); // update position per frame scaling
                    //float shoulder = KinectClothAugmenter.CalculateShoulderWidth(KinectConfig.userID, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
                    //float height = KinectClothAugmenter.CalculateHeight(KinectConfig.userID, KinectWrapper.NuiSkeletonPositionIndex.Head, KinectWrapper.NuiSkeletonPositionIndex.FootLeft, KinectWrapper.NuiSkeletonPositionIndex.FootRight);
                    //float chest = KinectClothAugmenter.EstimateChest(shoulder); // Or use manual input
                    //string size = KinectClothAugmenter.RecommendSize(shoulder, height, chest);
                    //Debug.Log($"Recommended Size: {size}\nEstimatedChest: {chest}\nEstemated Height: {height}\nDistance shoulder to shoulder: {shoulder}");
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
