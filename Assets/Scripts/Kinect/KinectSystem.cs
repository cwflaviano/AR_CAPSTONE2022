using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KinectSystem : MonoBehaviour
{
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectStreams KinectStreams;
    [SerializeField] private CameraDisplay CameraDisplay;

    private int sensorAngleCache;


    // initialize kinect device and necessary functions
    public void KinectInitialization()
    {
        int hr = 0;
        try
        {
            hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesSkeleton | KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex | (KinectConfig.ComputeColorMap ? KinectWrapper.NuiInitializeFlags.UsesColor : 0));
            if (hr != 0)
                throw new Exception("NuiInitialize Failed");

            hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);  // 0 = full body, 12 = ??, 8 = seated body tracking/top
            if (hr != 0)
                throw new Exception("Cannot initialize Skeleton Data");

            KinectConfig.depthStreamHandle = IntPtr.Zero;
            if (KinectConfig.ComputeUserMap)
            {
                hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.DepthAndPlayerIndex, KinectWrapper.Constants.DepthImageResolution, 0, 2, IntPtr.Zero, ref KinectConfig.depthStreamHandle);
                if (hr != 0)
                    throw new Exception("Cannot open depth stream");
            }

            KinectConfig.colorStreamHandle = IntPtr.Zero;
            if (KinectConfig.ComputeColorMap)
            {
                hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color, KinectWrapper.Constants.ColorImageResolution, 0, 2, IntPtr.Zero, ref KinectConfig.colorStreamHandle);
                if (hr != 0)
                    throw new Exception("Cannot open color stream");
            }

            SetKinectSensorElevationAngle(KinectConfig.SensorAngle);

            // init skeleton structures
            KinectConfig.skeletonFrame = new KinectWrapper.NuiSkeletonFrame()
            {
                SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount]
            };

            KinectConfig.smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();

            switch (KinectConfig.smoothing)
            {
                case Smoothing.Default:
                    KinectConfig.smoothParameters.fSmoothing = 0.5f;
                    KinectConfig.smoothParameters.fCorrection = 0.5f;
                    KinectConfig.smoothParameters.fPrediction = 0.5f;
                    KinectConfig.smoothParameters.fJitterRadius = 0.05f;
                    KinectConfig.smoothParameters.fMaxDeviationRadius = 0.04f;
                    break;
                case Smoothing.Medium:
                    KinectConfig.smoothParameters.fSmoothing = 0.5f;
                    KinectConfig.smoothParameters.fCorrection = 0.1f;
                    KinectConfig.smoothParameters.fPrediction = 0.5f;
                    KinectConfig.smoothParameters.fJitterRadius = 0.1f;
                    KinectConfig.smoothParameters.fMaxDeviationRadius = 0.1f;
                    break;
                case Smoothing.Aggressive:
                    KinectConfig.smoothParameters.fSmoothing = 0.7f;
                    KinectConfig.smoothParameters.fCorrection = 0.3f;
                    KinectConfig.smoothParameters.fPrediction = 1.0f;
                    KinectConfig.smoothParameters.fJitterRadius = 1.0f;
                    KinectConfig.smoothParameters.fMaxDeviationRadius = 1.0f;
                    break;
            }

            // init the tracking state filter
            KinectConfig.trackingStateFilter = new TrackingStateFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < KinectConfig.trackingStateFilter.Length; i++)
            {
                KinectConfig.trackingStateFilter[i] = new TrackingStateFilter();
                KinectConfig.trackingStateFilter[i].Init();
            }

            // init the bone orientation filter
            KinectConfig.boneOrientationFilter = new BoneOrientationsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < KinectConfig.boneOrientationFilter.Length; i++)
            {
                KinectConfig.boneOrientationFilter[i] = new BoneOrientationsFilter();
                KinectConfig.boneOrientationFilter[i].Init();
            }

            // init the clipped legs filter
            KinectConfig.clippedLegsFilter = new ClippedLegsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
            for (int i = 0; i < KinectConfig.clippedLegsFilter.Length; i++)
                KinectConfig.clippedLegsFilter[i] = new ClippedLegsFilter();

            // init the bone orientation constraints
            KinectConfig.boneConstraintsFilter = new BoneOrientationsConstraint();
            KinectConfig.boneConstraintsFilter.AddDefaultConstraints();
            // init the self intersection constraints
            KinectConfig.selfIntersectionConstraint = new SelfIntersectionConstraint();

            int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;

            KinectConfig.userJointsTracked = new bool[skeletonJointsCount];
            KinectConfig.userPrevTracked = new bool[skeletonJointsCount];
            KinectConfig.userJointsPos = new Vector3[skeletonJointsCount];
            KinectConfig.userJointsOri = new Matrix4x4[skeletonJointsCount];

            //create the transform matrix that converts from kinect-space to world-space
            Quaternion quatTiltAngle = new Quaternion();
            quatTiltAngle.eulerAngles = new Vector3(-KinectConfig.SensorAngle, 0.0f, 0.0f);

            KinectConfig.kinectToWorld.SetTRS(new Vector3(0.0f, KinectConfig.SensorHeight, 0.0f), quatTiltAngle, Vector3.one);
            KinectConfig.flipMatrix = Matrix4x4.identity;
            KinectConfig.flipMatrix[2, 2] = -1;

        }
        catch (DllNotFoundException e)
        {
            string message = "Please check the Kinect SDK installation.";
            Debug.LogError(message);
            Debug.LogError(e.ToString());
            return;
        }
        catch (Exception e)
        {
            string message = e.Message + " - " + KinectWrapper.GetNuiErrorString(hr);
            Debug.LogError(message);
            Debug.LogError(e.ToString());
            return;
        }

        // Initialize depth & label map related stuff
        if (KinectConfig.ComputeUserMap)
        {
            KinectConfig.usersMapSize = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
            KinectConfig.usersLblTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
            KinectConfig.usersMapColors = new Color32[KinectConfig.usersMapSize];
            KinectConfig.usersPrevState = new ushort[KinectConfig.usersMapSize];
            KinectConfig.usersDepthMap = new ushort[KinectConfig.usersMapSize];
            KinectConfig.usersHistogramMap = new float[8192];
        }
        // Initialize color map related stuff
        if (KinectConfig.ComputeColorMap)
        {
            KinectConfig.usersClrTex = new Texture2D(KinectWrapper.GetColorWidth(), KinectWrapper.GetColorHeight());
            KinectConfig.colorImage = new Color32[KinectWrapper.GetColorWidth() * KinectWrapper.GetColorHeight()];
            KinectConfig.usersColorMap = new byte[KinectConfig.colorImage.Length << 2];
        }

        KinectConfig.KinectInitialized = true; // set KinectInitialized to true if no errors and working
        Debug.Log("[LOG] Kinect initialization successful!");
    }


    // checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
    public bool IsInitialized()
    {
        return KinectConfig.KinectInitialized;
    }

    // set kinect device dcamera elevation angle. max of 27 min of -27
    public void SetKinectSensorElevationAngle(int SensorAngle)
    {
        if(sensorAngleCache != SensorAngle)
        {
            if (SensorAngle < -27 || SensorAngle > 27) SensorAngle = 0;
            KinectConfig.SensorAngle = SensorAngle;
            KinectWrapper.NuiCameraElevationSetAngle(SensorAngle);
        }
    }


    // quit app if kinect is not initialzid
    public void OnApplicationQuit()
    {
        if (KinectConfig.KinectInitialized)
        {
            // Shutdown OpenNI
            KinectWrapper.NuiShutdown();
        }
    }
}
