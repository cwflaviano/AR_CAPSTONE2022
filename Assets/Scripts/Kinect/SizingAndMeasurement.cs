
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SizingAndMeasurement : MonoBehaviour
{
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectTracking KinectTracking;

    [System.Serializable]
    public class FilipinoSizeProfile
    {
        public string sizeName; // XS, S, M, L, XL
        public float minShoulderWidth; // in meters
        public float maxShoulderWidth;
        public float minShoulderToHip;
        public float maxShoulderToHip;
        public Vector3 scaleMultiplier; // Model scale adjustment
    }

    public FilipinoSizeProfile[] sizeProfiles = new FilipinoSizeProfile[]
    {
        // Based on average Filipino anthropometric data
        new FilipinoSizeProfile { sizeName = "XS", minShoulderWidth = 0.35f, maxShoulderWidth = 0.38f, minShoulderToHip = 0.45f, maxShoulderToHip = 0.50f, scaleMultiplier = new Vector3(0.9f, 0.9f, 0.9f) },
        new FilipinoSizeProfile { sizeName = "S",  minShoulderWidth = 0.38f, maxShoulderWidth = 0.41f,  minShoulderToHip = 0.50f, maxShoulderToHip = 0.55f, scaleMultiplier = new Vector3(0.95f, 0.95f, 0.95f) },
        new FilipinoSizeProfile { sizeName = "M",  minShoulderWidth = 0.41f, maxShoulderWidth = 0.44f, minShoulderToHip = 0.55f, maxShoulderToHip = 0.60f, scaleMultiplier = Vector3.one },
        new FilipinoSizeProfile { sizeName = "L",  minShoulderWidth = 0.44f, maxShoulderWidth = 0.47f,  minShoulderToHip = 0.60f, maxShoulderToHip = 0.65f, scaleMultiplier = new Vector3(1.05f, 1.05f, 1.05f) },
        new FilipinoSizeProfile { sizeName = "XL", minShoulderWidth = 0.47f, maxShoulderWidth = 0.50f, minShoulderToHip = 0.65f, maxShoulderToHip = 0.70f, scaleMultiplier = new Vector3(1.1f, 1.1f, 1.1f) }
    };


    public float GetDepthCompensatedDistance(Vector3 joint1, Vector3 joint2, float referenceDistance)
    {
        // Get raw distance
        float rawDistance = Vector3.Distance(joint1, joint2);

        // Get average depth (distance from Kinect)
        float depth = (joint1.z + joint2.z) * 0.5f;

        // Compensate for perspective (objects further away appear smaller)
        // This formula assumes Kinect is at z=0
        float depthCompensation = Mathf.Clamp(depth / referenceDistance, 0.8f, 1.2f);

        return rawDistance * depthCompensation;
    }


    public string DetectSize()
    {
        if (!KinectConfig.userCalibrated) return "M"; // Default size

        Vector3 shoulderLeft = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
        Vector3 shoulderRight = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
        Vector3 shoulderCenter = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);
        Vector3 hipCenter = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

        // Get depth-compensated measurements
        float shoulderWidth = GetDepthCompensatedDistance(shoulderLeft, shoulderRight, shoulderCenter.z);
        float torsoHeight = GetDepthCompensatedDistance(shoulderCenter, hipCenter, hipCenter.z);

        // Find matching size profile
        foreach (var profile in sizeProfiles)
        {
            if (shoulderWidth >= profile.minShoulderWidth &&  shoulderWidth <= profile.maxShoulderWidth && torsoHeight >= profile.minShoulderToHip && torsoHeight <= profile.maxShoulderToHip)
                return profile.sizeName;
        }

        // If no exact match, find closest
        return FindClosestSize(shoulderWidth, torsoHeight);
    }

    public string FindClosestSize(float shoulderWidth, float torsoHeight)
    {
        float minDistance = float.MaxValue;
        string closestSize = "M";

        foreach (var profile in sizeProfiles)
        {
            float shoulderCenter = (profile.minShoulderWidth + profile.maxShoulderWidth) * 0.5f;
            float torsoCenter = (profile.minShoulderToHip + profile.maxShoulderToHip) * 0.5f;

            float distance = Mathf.Abs(shoulderWidth - shoulderCenter) + Mathf.Abs(torsoHeight - torsoCenter);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestSize = profile.sizeName;
            }
        }

        return closestSize;
    }
}
