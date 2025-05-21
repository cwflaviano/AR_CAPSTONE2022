using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

public class KinectClothAugmenter : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private KinectConfig kinectConfig;
    [SerializeField] private KinectTracking KinectTracking;
    [SerializeField] private Debugging Debugging;

    [Header("Main Conponenets")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas arCanvas;
    [SerializeField] private RawImage screen;
    [SerializeField] private GameObject modelContainer;

    [Header("Rigg Model")]
    [SerializeField] private GameObject model;
    [SerializeField] private Transform hipCenter;
    [SerializeField] private Transform spine;
    [SerializeField] private Transform spine1;
    [SerializeField] private Transform spine2;
    [SerializeField] private Transform spine3;
    [SerializeField] private Transform shoulderCenter;
    [SerializeField] private Transform leftClavicle;
    [SerializeField] private Transform leftShoulder;
    [SerializeField] private Transform leftUpperArm;
    [SerializeField] private Transform leftElbow;
    [SerializeField] private Transform rightClavicle;
    [SerializeField] private Transform rightShoulder;
    [SerializeField] private Transform rightUpperArm;
    [SerializeField] private Transform rightElbow;
    [SerializeField] private Transform leftHip;
    [SerializeField] private Transform rightHip;


    [HideInInspector] public bool isAugmented = false;
    private float timer = 2f;
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

    // Position Model Method 
    public void DefaultPositionModel()
    {
        if (model != null)
        {
            model.transform.localPosition = new Vector3(0, 0, 0);
            model.transform.localRotation = Quaternion.Euler(0, 0, 0);
            model.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }


    // model positioning...
    public void PositionModel(uint userId)
    {
        if (!isAugmented)
        {
            Debugging.text4.text = $"Stay within 1 - 3 meters ...Trying to Augment model in {(int)time}";

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
            Vector3 sc = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);
            Vector3 ls = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
            Vector3 rs = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
            Vector3 hc = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

            float yAxis = sc.y - 0.48f;
            float sx = Vector3.Distance(ls, rs);
            float sy = Vector3.Distance(sc, hc);


            model.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            model.transform.position = new Vector3(hc.x, yAxis, hc.z);
        }
    }





    //private Vector3 _modelBasePosition = new Vector3(0, 1.27f, 0);
    //private Vector3 _originalScale = Vector3.one;
    //private float _referenceDistance = 2f; // Standard calibration distance

    //void Start()
    //{
    //    _originalScale = model.transform.localScale;
    //    model.transform.position = _modelBasePosition;
    //}

    //float CalculateDepthAdjustedScale()
    //{
    //    if (!kinectConfig.userCalibrated) return 1f;

    //    // Get hip position (in Kinect space)
    //    Vector3 hipPos = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

    //    // Calculate actual distance from Kinect
    //    float userDistance = Mathf.Abs(hipPos.z);

    //    // Standard scaling at reference distance (2m)
    //    float baseScale = CalculateBodyProportionScale();

    //    // Adjust for depth (objects further away should appear smaller)
    //    float depthScale = _referenceDistance / userDistance;

    //    // Clamp to prevent extreme scaling
    //    return baseScale * Mathf.Clamp(depthScale, 0.7f, 1.5f);
    //}

    //float CalculateBodyProportionScale()
    //{
    //    Vector3 shoulderLeft = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
    //    Vector3 shoulderRight = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
    //    Vector3 head = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.Head);

    //    float shoulderWidth = Vector3.Distance(shoulderLeft, shoulderRight);
    //    float headHeight = Vector3.Distance(head, (shoulderLeft + shoulderRight) * 0.5f);

    //    // Compare to reference measurements (adjust these to match your model)
    //    float widthScale = shoulderWidth / 0.4f; // 0.4m = reference shoulder width
    //    float heightScale = headHeight / 0.3f;   // 0.3m = reference head height

    //    return (widthScale + heightScale) * 0.5f; // Average scale
    //}


    //void UpdateModelPosition()
    //{
    //    if (!kinectConfig.userCalibrated) return;

    //    // Get tracked hip position (in Kinect space)
    //    Vector3 trackedHipPos = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

    //    // Convert to our scene coordinates
    //    Vector3 modelPosition = new Vector3(
    //        trackedHipPos.x,             // Mirror X if needed (-trackedHipPos.x)
    //        _modelBasePosition.y,         // Maintain fixed height
    //        _modelBasePosition.z          // Fixed Z position
    //    );

    //    // Apply depth-aware scaling
    //    float currentScale = CalculateDepthAdjustedScale();

    //    // Smooth transitions
    //    model.transform.position = Vector3.Lerp(
    //        model.transform.position,
    //        modelPosition,
    //        Time.deltaTime * 5f
    //    );

    //    model.transform.localScale = Vector3.Lerp(
    //        model.transform.localScale,
    //        _originalScale * currentScale,
    //        Time.deltaTime * 5f
    //    );

    //    // Update rotation based on shoulders
    //    UpdateModelRotation();
    //}



    //void UpdateModelRotation()
    //{
    //    Vector3 shoulderLeft = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
    //    Vector3 shoulderRight = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);

    //    // Calculate shoulder direction (flip X if using mirrored view)
    //    Vector3 shoulderDir = shoulderRight - shoulderLeft;
    //    shoulderDir.y = 0; // Keep upright

    //    // Only rotate if we have valid shoulder data
    //    if (shoulderDir.magnitude > 0.1f)
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(
    //            -shoulderDir,  // Negative because model is rotated 180
    //            Vector3.up
    //        );

    //        modelContainer.transform.rotation = Quaternion.Slerp(
    //            modelContainer.transform.rotation,
    //            targetRotation,
    //            Time.deltaTime * 5f
    //        );
    //    }
    //}


    //void Update()
    //{
    //    if (kinectConfig.userCalibrated)
    //    {
    //        UpdateModelPosition();

    //        // Debug information
    //        Vector3 hipPos = KinectTracking.GetJointPosition(kinectConfig.userID,
    //            (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);
    //        Debug.Log($"User Distance: {Mathf.Abs(hipPos.z):F2}m | Current Scale: {model.transform.localScale.x:F2}");
    //    }
    //}
    //[System.Serializable]
    //public class FilipinoSizeProfile
    //{
    //    public string sizeName; // XS, S, M, L, XL
    //    public float minShoulderWidth; // in meters
    //    public float maxShoulderWidth;
    //    public float minShoulderToHip;
    //    public float maxShoulderToHip;
    //    public Vector3 scaleMultiplier; // Model scale adjustment
    //}





    //public float GetDepthCompensatedDistance(Vector3 joint1, Vector3 joint2, float referenceDistance)
    //{
    //    // Get raw distance
    //    float rawDistance = Vector3.Distance(joint1, joint2);

    //    // Get average depth (distance from Kinect)
    //    float depth = (joint1.z + joint2.z) * 0.5f;

    //    // Compensate for perspective (objects further away appear smaller)
    //    // This formula assumes Kinect is at z=0
    //    float depthCompensation = Mathf.Clamp(depth / referenceDistance, 0.8f, 1.2f);

    //    return rawDistance * depthCompensation;
    //}

    //string DetectSize()
    //{
    //    if (!kinectConfig.userCalibrated) return "M"; // Default size

    //    Vector3 shoulderLeft = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
    //    Vector3 shoulderRight = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
    //    Vector3 shoulderCenter = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);
    //    Vector3 hipCenter = KinectTracking.GetJointPosition(kinectConfig.userID,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

    //    // Get depth-compensated measurements
    //    float shoulderWidth = GetDepthCompensatedDistance(shoulderLeft, shoulderRight, 1.8f);
    //    float torsoHeight = GetDepthCompensatedDistance(shoulderCenter, hipCenter, 1.8f);

    //    // Find matching size profile
    //    foreach (var profile in sizeProfiles)
    //    {
    //        if (shoulderWidth >= profile.minShoulderWidth &&
    //            shoulderWidth <= profile.maxShoulderWidth &&
    //            torsoHeight >= profile.minShoulderToHip &&
    //            torsoHeight <= profile.maxShoulderToHip)
    //        {
    //            return profile.sizeName;
    //        }
    //    }

    //    // If no exact match, find closest
    //    return FindClosestSize(shoulderWidth, torsoHeight);
    //}

    //private void Update()
    //{
    //    if (kinectConfig.userCalibrated)
    //    {
    //        string detectedSize = DetectSize();
    //        FilipinoSizeProfile sizeProfile = System.Array.Find(sizeProfiles, p => p.sizeName == detectedSize);

    //        // Get depth for distance-based scaling
    //        Vector3 hipPos = KinectTracking.GetJointPosition(kinectConfig.userID,
    //            (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);
    //        float userDistance = Mathf.Abs(hipPos.z);

    //        // Apply size scaling with depth compensation
    //        float depthScale = Mathf.Clamp(userDistance / 2f, 0.8f, 1.2f); // Normalize for 2m distance
    //        Vector3 finalScale = sizeProfile.scaleMultiplier * depthScale;

    //        model.transform.localScale = Vector3.Lerp(
    //            model.transform.localScale,
    //            finalScale,
    //            Time.deltaTime * 5f // Smooth transition
    //        );

    //        Debug.Log($"Detected size: {detectedSize}, Depth: {userDistance:F2}m");
    //    }
    //}
    //string FindClosestSize(float shoulderWidth, float torsoHeight)
    //{
    //    float minDistance = float.MaxValue;
    //    string closestSize = "M";

    //    foreach (var profile in sizeProfiles)
    //    {
    //        float shoulderCenter = (profile.minShoulderWidth + profile.maxShoulderWidth) * 0.5f;
    //        float torsoCenter = (profile.minShoulderToHip + profile.maxShoulderToHip) * 0.5f;

    //        float distance = Mathf.Abs(shoulderWidth - shoulderCenter) +
    //                        Mathf.Abs(torsoHeight - torsoCenter);

    //        if (distance < minDistance)
    //        {
    //            minDistance = distance;
    //            closestSize = profile.sizeName;
    //        }
    //    }

    //    return closestSize;
    //}

}
