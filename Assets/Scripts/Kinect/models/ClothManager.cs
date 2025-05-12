using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClothManager : MonoBehaviour
{

    public KinectConfig KinectConfig;
    public KinectTracking KinectTracking;
    public TopGarments TopGarments;
    public CameraDisplay CameraDisplay;

    private Vector3 canvasOffset = new Vector3(0f, 1f, 89.9f);

    public void SetModelPos()
    {
        TopGarments.model.transform.position = canvasOffset;
    }
    public void SetModelActive()
    {
        TopGarments.model.SetActive(true);
    }
    public void SetModelNotActive()
    {
        TopGarments.model.SetActive(false);
    }



    public void ClothAugmention()
    {
        //Vector3 shoulderCenter = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);
        //Vector3 leftShoulder = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
        //Vector3 rightShoulder = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
        //Vector3 RightShoulder = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
        //Vector3 RightElbow = KinectTracking.GetJointPosition(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight);
        //float RightUpperArm = Vector3.Distance(RightShoulder, RightElbow);

        //Quaternion RightRotShoulder = KinectTracking.GetJointOrientation(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, true);
        //Quaternion LeftRotShoulder = KinectTracking.GetJointOrientation(KinectConfig.userID, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, true);
        //TopGarments.RightShoulder.transform.rotation = LeftRotShoulder;
        //TopGarments.LeftShoulder.transform.rotation =  RightRotShoulder;

    }

    public void UpdateModelPositionAndScale()
    {
        // Get user position from Kinect (in meters)
        Vector3 userPos = KinectTracking.GetUserPosition(KinectConfig.userID);

        // Convert Kinect position to your scene coordinates
        // Adjust these values based on your scene setup
        float scaleFactor = 100f; // Adjust this to match your scene scale
        Vector3 modelPosition = new Vector3(
            -userPos.x * scaleFactor + canvasOffset.x,
            userPos.y * scaleFactor + canvasOffset.y,
            canvasOffset.z
        );

        TopGarments.model.transform.position = modelPosition;

        // Scale based on distance (simplified approach)
        // Get distance between shoulders to estimate scale
        Vector3 leftShoulder = KinectTracking.GetJointPosition(KinectConfig.userID,
            (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
        Vector3 rightShoulder = KinectTracking.GetJointPosition(KinectConfig.userID,
            (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);

        float shoulderWidth = Vector3.Distance(leftShoulder, rightShoulder);
        float referenceShoulderWidth = 0.4f; 

        float scale = shoulderWidth / referenceShoulderWidth;
        TopGarments.model.transform.localScale = Vector3.one * scale;
    }


    public void UpdateModelRotation()
    {
        // Get user orientation from Kinect
        Quaternion userOrientation = KinectTracking.GetUserOrientation(KinectConfig.userID, true);

        // Adjust for your model's initial rotation (if it's facing 180 degrees on Y)
        Quaternion modelRotation = Quaternion.Euler(0f, 180f, 0f) * userOrientation;

        TopGarments.model.transform.rotation = modelRotation;
    }



    public void UpdateJoints()
    {
        // Update spine joints
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.HipCenter, TopGarments.HipCenter);
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.HipLeft, TopGarments.LeftHip);
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.HipRight, TopGarments.RightHip);

        // Update spine hierarchy
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.Spine, TopGarments.Spine);
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter, TopGarments.ShoulderCenter);

        // Update shoulders and arms
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, TopGarments.LeftShoulder);
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft, TopGarments.LeftElbow);

        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, TopGarments.RightShoulder);
        UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex.ElbowRight, TopGarments.RightElbow);
    }

    private void UpdateJoint(KinectWrapper.NuiSkeletonPositionIndex jointIndex, Transform jointTransform)
    {
        if (jointTransform == null) return;

        // Get joint position and rotation from Kinect
        Vector3 jointPos = KinectTracking.GetJointPosition(KinectConfig.userID, (int)jointIndex);
        Quaternion jointRot = KinectTracking.GetJointOrientation(KinectConfig.userID, (int)jointIndex, true);

        // Convert to model space
        float scaleFactor = 100f; // Same as in position update
        Vector3 modelJointPos = new Vector3(
            -jointPos.x * scaleFactor,
            jointPos.y * scaleFactor,
            jointPos.z * scaleFactor
        );

        // Apply to model
        jointTransform.localPosition = modelJointPos;
        jointTransform.localRotation = jointRot;
    }














    //[Header("Cloth Models")]
    //public GameObject topModel;
    //public GameObject bottomModel;

    //[Header("Reference Points")]
    //public Transform shoulderCenter;
    //public Transform spine3;

    //[Header("Scaling")]
    //public float scaleFactor = 1.0f;

    //[SerializeField] private KinectTracking kinectTracking;
    //[SerializeField] private CameraDisplay cameraDisplay;

    //public void UpdateClothPosition()
    //{
    //    if (!topModel.activeSelf || !kinectTracking.IsUserCalibrated())
    //        return;

    //    // Get the user's shoulder center position in world space
    //    Vector3 shoulderPos = kinectTracking.GetJointPosition(kinectTracking.GetUserID(),
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);

    //    // Get the spine position for orientation reference
    //    Vector3 spinePos = kinectTracking.GetJointPosition(kinectTracking.GetUserID(),
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.Spine);

    //    // Position the cloth model
    //    topModel.transform.position = shoulderPos;

    //    // Scale based on user size (distance from shoulders to hips)
    //    float userScale = CalculateUserScale();
    //    topModel.transform.localScale = Vector3.one * userScale * scaleFactor;

    //    // Adjust for camera perspective
    //    AdjustForCameraPerspective();
    //}

    //private float CalculateUserScale()
    //{
    //    uint userId = kinectTracking.GetUserID();

    //    Vector3 shoulderPos = kinectTracking.GetJointPosition(userId,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);

    //    Vector3 hipPos = kinectTracking.GetJointPosition(userId,
    //        (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

    //    // Default scale factor based on distance between shoulders and hips
    //    return Vector3.Distance(shoulderPos, hipPos) / 0.5f; // 0.5f is a reference distance
    //}

    //private void AdjustForCameraPerspective()
    //{
    //    if (cameraDisplay == null || cameraDisplay.mainCamera == null)
    //        return;

    //    // Get the main camera
    //    Camera cam = cameraDisplay.mainCamera;

    //    // Calculate screen position of the shoulder center
    //    Vector3 screenPos = cam.WorldToScreenPoint(topModel.transform.position);

    //    // Adjust z-position based on camera distance to make it appear in front of the user
    //    float zOffset = cam.nearClipPlane + 0.1f; // Small offset to ensure it renders in front
    //    screenPos.z = zOffset;

    //    // Convert back to world space
    //    Vector3 newWorldPos = cam.ScreenToWorldPoint(screenPos);

    //    // Apply the new position
    //    topModel.transform.position = newWorldPos;
    //}

    //public void EnableModel(GameObject model)
    //{
    //    if (model != null)
    //    {
    //        model.SetActive(true);
    //        UpdateClothPosition();
    //    }
    //}

    //public void DisableModel(GameObject model)
    //{
    //    if (model != null)
    //    {
    //        model.SetActive(false);
    //    }
    //}
}
