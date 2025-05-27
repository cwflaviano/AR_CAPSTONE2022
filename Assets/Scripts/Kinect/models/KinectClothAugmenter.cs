using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static KinectWrapper;

// not yet used!
public enum Sizing 
{
    Too_Small,
    XS,
    S,
    M,
    L,
    XL,
    Too_Large
}
public struct SizeChart
{
    public string Size;
    public float[] ShoulderCm;
    public float[] HeightCm;
    public float[] ChestCm;
}
public class KinectClothAugmenter : MonoBehaviour
{

    private readonly float[] sizing = {
        
    };


    [Header("Scripts")]
    [SerializeField] private KinectConfig kinectConfig;
    [SerializeField] private KinectTracking KinectTracking;
    [SerializeField] private Debugging Debugging;

    [Header("Main Conponenets")]
    [SerializeField] private GameObject modelContainer;

    [Header("Rigg Model")]
    [SerializeField] private GameObject model;
    [SerializeField] private Transform hipCenter;
    [SerializeField] private Transform spine;
    [SerializeField] private Transform shoulderCenter;
    [SerializeField] private Transform leftShoulder;
    [SerializeField] private Transform leftElbow;
    [SerializeField] private Transform rightShoulder;
    [SerializeField] private Transform rightElbow;
    [SerializeField] private Transform leftHip;
    [SerializeField] private Transform rightHip;


    [HideInInspector] public bool isAugmented = false;
    private float timer = 1.1f;
    private float time = 0f;

    // Position Model Container method
    public void DefaultPositionModelContainer()
    {
        if (modelContainer != null)
        {
            modelContainer.transform.localPosition = new Vector3(0, 0, 0);
            modelContainer.transform.localRotation = Quaternion.Euler(0, 180, 0);
            modelContainer.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // default sition Model Method 
    public void DefaultPositionModel()
    {
        if (model != null)
        {
            model.transform.localPosition = new Vector3(0, 0, 0);
            model.transform.localRotation = Quaternion.Euler(0, 0, 0);
            model.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }


    // model positioning...
    public void PositionModel()
    {
        if (!isAugmented)
        {
            Debugging.text4.text = $"Stay within 1 - 3 meters....Trying to Augment model in {(int)time}";

            // Increment time by deltaTime (frame-independent)
            time += Time.deltaTime;

            if (time >= timer)
            {   
                time = 0f;
                isAugmented = true;
                Debugging.text4.text = "Augmented.";
            }
        }

        if (model != null && shoulderCenter != null)
        {
            uint userId = kinectConfig.userID;
            model.transform.localScale = new Vector3(0.22f, 0.3f, 0.2f); // hard coded model scaling

            // joint position
            JointPosition(userId, hipCenter, KinectWrapper.NuiSkeletonPositionIndex.HipCenter);
            JointPosition(userId, leftHip, KinectWrapper.NuiSkeletonPositionIndex.HipCenter);
            JointPosition(userId, rightHip, KinectWrapper.NuiSkeletonPositionIndex.HipCenter);
            JointPosition(userId, spine, KinectWrapper.NuiSkeletonPositionIndex.Spine);
            JointPosition(userId, shoulderCenter, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);
        }
    }


    //public float CalculateHeight(uint userId, KinectWrapper.NuiSkeletonPositionIndex head, KinectWrapper.NuiSkeletonPositionIndex foot1, KinectWrapper.NuiSkeletonPositionIndex foot2)
    //{
    //    Vector3 h = KinectTracking.GetJointPosition(userId, (int)head);
    //    Vector3 f1 = KinectTracking.GetJointPosition(userId, (int)foot1);
    //    Vector3 f2 = KinectTracking.GetJointPosition(userId, (int)foot2);

    //    // Use the higher foot (closer to sensor) to avoid errors from leg positioning
    //    float footY = Math.Max(f1.y, f2.y);
    //    float height = Math.Abs(h.y - footY) * 100; // Convert meters to cm
    //    return height;
    //}

    //// get normalzied joint distance from joint a to joint b returns float value
    //public float JointDistance(Vector3 joint1, Vector3 joint2)
    //{
    //    return Vector3.Distance(joint1, joint2);
    //}

    // get normalzied joint position
    public void JointPosition(uint userId, Transform jointTransform, KinectWrapper.NuiSkeletonPositionIndex joint)
    {
        Vector3 jointPos = KinectTracking.GetJointPosition(userId, (int)joint);
        jointTransform.position = new Vector3(jointPos.x, jointPos.y, jointPos.z);
    }

    //public void JointRotation(uint userId, Transform jointTransform, KinectWrapper.NuiSkeletonPositionIndex joint)
    //{
    //    bool flip = true; // Adjust based on your model's coordinate system (true for mirroring)
    //    Quaternion jointRot = KinectTracking.GetJointOrientation(userId, (int)joint, flip);
    //    jointTransform.rotation = jointRot;
    //}





    //public float CalculateShoulderWidth(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint1, KinectWrapper.NuiSkeletonPositionIndex joint2)
    //{
    //    Vector3 j1 = KinectTracking.GetJointPosition(userId, (int)joint1);
    //    Vector3 j2 = KinectTracking.GetJointPosition(userId, (int)joint2);
    //    return Vector3.Distance(
    //        new Vector3(j1.x, j1.y, j1.z),
    //        new Vector3(j2.x, j2.y, j2.z)
    //    ) * 100;
    //}
    //public float EstimateChest(float shoulderWidth)
    //{
    //    return shoulderWidth * 2.5f; // Heuristic based on average body proportions
    //}
    //List<float> shoulderMeasurements = new List<float>();
    //public float SmoothMeasurement(float newMeasurement)
    //{
    //    shoulderMeasurements.Add(newMeasurement);
    //    if (shoulderMeasurements.Count > 10) shoulderMeasurements.RemoveAt(0);
    //    return shoulderMeasurements.Average();
    //}


    //public List<SizeChart> GetSizeCharts()
    //{
    //    return new List<SizeChart>
    //    {
    //        new SizeChart
    //        {
    //            Size = "Small",
    //            ShoulderCm = new float[] { 40, 44 },
    //            HeightCm = new float[] { 160, 170 },
    //            ChestCm = new float[] { 85, 90 }
    //        },
    //        new SizeChart
    //        {
    //            Size = "Medium",
    //            HeightCm = new float[] { 165, 175 },
    //            ShoulderCm = new float[] { 45, 49 },
    //            ChestCm = new float[] { 91, 96 }
    //        },
    //        new SizeChart
    //        {
    //            Size = "Large",
    //            ShoulderCm = new float[] { 50, 54 },
    //            HeightCm = new float[] { 170, 180 },
    //            ChestCm = new float[] { 97, 102 }
    //        }
    //    };
    //}

    //public string RecommendSize(float shoulder, float height, float chest)
    //{
    //    var charts = GetSizeCharts();
    //    string bestSize = "Unknown";
    //    float bestScore = float.MaxValue;

    //    foreach (var chart in charts)
    //    {
    //        float shoulderMid = (chart.ShoulderCm[0] + chart.ShoulderCm[1]) / 2;
    //        float heightMid = (chart.HeightCm[0] + chart.HeightCm[1]) / 2;
    //        float chestMid = (chart.ChestCm[0] + chart.ChestCm[1]) / 2;
    //        float score = math.abs(shoulder - shoulderMid) + math.abs(height - heightMid) + math.abs(chest - chestMid);

    //        if (score < bestScore)
    //        {
    //            bestScore = score;
    //            bestSize = chart.Size;
    //        }
    //    }
    //    return bestSize;
    //}
}
