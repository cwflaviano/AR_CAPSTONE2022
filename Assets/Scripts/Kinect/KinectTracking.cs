using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KinectTracking : MonoBehaviour
{
    public KinectConfig KinectConfig;
    public KinectSystem KinectSystem;
    public KinectStreams KinectStreams;

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;

    public void PollSkeleton()
    {
        if (KinectWrapper.PollSkeleton(ref KinectConfig.smoothParameters, ref KinectConfig.skeletonFrame))
            ProcessSkeleton();
    }

    public void ProcessSkeleton()
    {
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - KinectConfig.lastNuiTime;

        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            KinectWrapper.NuiSkeletonData skeletonData = KinectConfig.skeletonFrame.SkeletonData[i];
            KinectConfig.skeletonPos = KinectConfig.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
            float distance = Mathf.Abs(KinectConfig.skeletonPos.z);


            if (KinectConfig.userID == 0 && !KinectConfig.userCalibrated && skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                if (!(distance < KinectConfig.MinUserDistance) && !(distance > KinectConfig.MaxUserDistance))
                {
                    text1.text = "User Tracked";

                    KinectConfig.userCalibrated = true;
                    KinectConfig.userID = skeletonData.dwTrackingID;
                    KinectConfig.userTrackedIndex = skeletonData.dwUserIndex;
                    KinectConfig.userIndex = i;
                    KinectConfig.userDistance = distance;

                    if (KinectConfig.trackingStateFilter[0] != null)
                        KinectConfig.trackingStateFilter[0].Reset();
                    if (KinectConfig.boneOrientationFilter[0] != null)
                        KinectConfig.boneOrientationFilter[0].Reset();
                }
                else
                {
                    Debug.LogWarning("[TRACKING] No Closest User within 1 to 3 meters");
                    text1.text = "No Closest User within 1 to 3 meters";
                    continue;
                }
            }

            if(KinectConfig.userID != 0 && IsMoreThanOneUser() && KinectConfig.userID != skeletonData.dwTrackingID && skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                if((distance < KinectConfig.userDistance) && !(distance < KinectConfig.MinUserDistance) && !(distance > KinectConfig.MaxUserDistance))
                {
                    KinectConfig.userCalibrated = true;
                    KinectConfig.userID = skeletonData.dwTrackingID;
                    KinectConfig.userTrackedIndex = skeletonData.dwUserIndex;
                    KinectConfig.userIndex = i;
                    KinectConfig.userDistance = distance;
                }
            }

            if (KinectConfig.userCalibrated &&
                //KinectConfig.userID == KinectConfig.skeletonFrame.SkeletonData[i].dwTrackingID &&
                skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                KinectConfig.userDistance = distance;
                text2.text = $"User: {KinectConfig.userID.ToString()}, DISTANCE: {distance.ToString()}";

                if (distance < KinectConfig.MinUserDistance || distance > KinectConfig.MaxUserDistance)
                {
                    Debug.LogWarning($"[TRACKING] User: {KinectConfig.userID.ToString()}, Is not within 1 to 3 meters");
                    text2.text = $"User: {KinectConfig.userID.ToString()}, Is not within 1 to 3 meters";
                    break;
                }

                KinectConfig.userPos = KinectConfig.skeletonPos;
                KinectConfig.trackingStateFilter[0].UpdateFilter(ref skeletonData);

                if (KinectConfig.UseClippedLegsFilter && KinectConfig.clippedLegsFilter[0] != null)
                    KinectConfig.clippedLegsFilter[0].FilterSkeleton(ref skeletonData, deltaNuiTime);

                if (KinectConfig.UseSelfIntersectionConstraint && KinectConfig.selfIntersectionConstraint != null)
                    KinectConfig.selfIntersectionConstraint.Constrain(ref skeletonData);

                for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
                {
                    bool jointTracked = KinectConfig.IgnoreInferredJoints ? (int)skeletonData.eSkeletonPositionTrackingState[j] == KinectConfig.stateTracked : (int)skeletonData.eSkeletonPositionTrackingState[j] != KinectConfig.stateNotTracked;
                    KinectConfig.userJointsTracked[j] = jointTracked;
                    KinectConfig.userPrevTracked[j] = jointTracked;

                    if (jointTracked)
                        KinectConfig.userJointsPos[j] = KinectConfig.kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
                }

                // Calculate joint orientations
                KinectWrapper.GetSkeletonJointOrientation(ref KinectConfig.userJointsPos, ref KinectConfig.userJointsTracked, ref KinectConfig.userJointsOri);

                // Apply orientation constraints
                if (KinectConfig.UseBoneOrientationsConstraint && KinectConfig.boneConstraintsFilter != null)
                    KinectConfig.boneConstraintsFilter.Constrain(ref KinectConfig.userJointsOri, ref KinectConfig.userJointsTracked);

                if (KinectConfig.UseBoneOrientationsFilter && KinectConfig.boneOrientationFilter[0] != null)
                    KinectConfig.boneOrientationFilter[0].UpdateFilter(ref skeletonData, ref KinectConfig.userJointsOri);

                KinectConfig.userOri = KinectConfig.userJointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];

                text3.text = "...";
                break;
            }
            
            if(KinectConfig.userCalibrated && skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.NotTracked)
            {
                text1.text = "No User to be tracked";
                text2.text = "...";
                text3.text = "User is out of view!";
                KinectConfig.userCalibrated = false;
                KinectConfig.userID = 0;
                KinectConfig.userIndex = -1;
                KinectConfig.userDistance = -1;
            }
        }
    }

    //public void ProcessSkeleton()
    //{
    //    float currentNuiTime = Time.realtimeSinceStartup;
    //    float deltaNuiTime = currentNuiTime - KinectConfig.lastNuiTime;

    //    for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
    //    {
    //        KinectWrapper.NuiSkeletonData skeletonData = KinectConfig.skeletonFrame.SkeletonData[i];
    //        if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
    //        {
    //            KinectConfig.skeletonPos = KinectConfig.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
    //            float distance = Mathf.Abs(KinectConfig.skeletonPos.z);
    //            KinectConfig.userDistance = distance;

    //            // check if currently track user is within the minUserDistance (1m) to maxUserDistance (3m)
    //            if (distance < KinectConfig.MinUserDistance || distance > KinectConfig.MaxUserDistance)
    //            {
    //                Debug.LogWarning("[WARNING] Tracked User is out or range");
    //                continue;
    //            }

    //            if (!KinectConfig.userCalibrated)
    //            {
    //                KinectConfig.userCalibrated = true;
    //                KinectConfig.userID = skeletonData.dwUserIndex;
    //                KinectConfig.userIndex = i;
    //                //KinectConfig.userDistance = distance;
    //                //Debug.Log($"USER: {KinectConfig.userID}, DISTANCE: {distance}");

    //                if (KinectConfig.trackingStateFilter[0] != null)
    //                    KinectConfig.trackingStateFilter[0].Reset();
    //                if (KinectConfig.boneOrientationFilter[0] != null)
    //                    KinectConfig.boneOrientationFilter[0].Reset();
    //            }

    //            if (KinectConfig.userCalibrated)
    //            {
    //                KinectConfig.userPos = KinectConfig.skeletonPos;
    //                KinectConfig.trackingStateFilter[0].UpdateFilter(ref skeletonData);

    //                if (KinectConfig.UseClippedLegsFilter && KinectConfig.clippedLegsFilter[0] != null)
    //                    KinectConfig.clippedLegsFilter[0].FilterSkeleton(ref skeletonData, deltaNuiTime);

    //                if (KinectConfig.UseSelfIntersectionConstraint && KinectConfig.selfIntersectionConstraint != null)
    //                    KinectConfig.selfIntersectionConstraint.Constrain(ref skeletonData);

    //                for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
    //                {
    //                    bool jointTracked = KinectConfig.IgnoreInferredJoints ? (int)skeletonData.eSkeletonPositionTrackingState[j] == KinectConfig.stateTracked : (int)skeletonData.eSkeletonPositionTrackingState[j] != KinectConfig.stateNotTracked;
    //                    KinectConfig.userJointsTracked[j] = jointTracked;
    //                    KinectConfig.userPrevTracked[j] = jointTracked;

    //                    if (jointTracked)
    //                        KinectConfig.userJointsPos[j] = KinectConfig.kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
    //                }

    //                // Calculate joint orientations
    //                KinectWrapper.GetSkeletonJointOrientation(ref KinectConfig.userJointsPos, ref KinectConfig.userJointsTracked, ref KinectConfig.userJointsOri);

    //                // Apply orientation constraints
    //                if (KinectConfig.UseBoneOrientationsConstraint && KinectConfig.boneConstraintsFilter != null)
    //                    KinectConfig.boneConstraintsFilter.Constrain(ref KinectConfig.userJointsOri, ref KinectConfig.userJointsTracked);

    //                if (KinectConfig.UseBoneOrientationsFilter && KinectConfig.boneOrientationFilter[0] != null)
    //                    KinectConfig.boneOrientationFilter[0].UpdateFilter(ref skeletonData, ref KinectConfig.userJointsOri);

    //                KinectConfig.userOri = KinectConfig.userJointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];

    //                break;
    //            }
    //        }
    //    }
    //}


    public bool CheckIfTrackedUserIsOutOfView()
    {
        bool isUserOutOfView = false;

        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            if (KinectConfig.skeletonFrame.SkeletonData[i].dwTrackingID == KinectConfig.userID)
            {
                KinectConfig.userCalibrated = false;
                KinectConfig.userID = 0;
                KinectConfig.userIndex = -1;
                isUserOutOfView = true;
                break;
            }
        }

        return isUserOutOfView;
    }


    public bool IsMoreThanOneUser()
    {
        if (!KinectSystem.IsInitialized())
            throw new Exception("[CRITICAL] Kinect is not Initialized or Device is not connected");

        int numOfActiveSkeleton = 0;

        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            if (KinectConfig.skeletonFrame.SkeletonData[i].eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
                numOfActiveSkeleton += 1;
        }

        if (numOfActiveSkeleton > 1)
        {
            numOfActiveSkeleton = 0;
            return true;
        }
        return false;
    }
    public void ResetFilters()
    {
        if (!KinectSystem.IsInitialized())
            return;

        // clear kinect vars
        KinectConfig.userPos = Vector3.zero;
        KinectConfig.userOri = Matrix4x4.identity;

        int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        for (int i = 0; i < skeletonJointsCount; i++)
        {
            KinectConfig.userJointsTracked[i] = false;
            KinectConfig.userPrevTracked[i] = false;
            KinectConfig.userJointsPos[i] = Vector3.zero;
            KinectConfig.userJointsOri[i] = Matrix4x4.identity;
        }

        if (KinectConfig.trackingStateFilter != null)
        {
            for (int i = 0; i < KinectConfig.trackingStateFilter.Length; i++)
                if (KinectConfig.trackingStateFilter[i] != null)
                    KinectConfig.trackingStateFilter[i].Reset();
        }

        if (KinectConfig.boneOrientationFilter != null)
        {
            for (int i = 0; i < KinectConfig.boneOrientationFilter.Length; i++)
                if (KinectConfig.boneOrientationFilter[i] != null)
                    KinectConfig.boneOrientationFilter[i].Reset();
        }

        if (KinectConfig.clippedLegsFilter != null)
        {
            for (int i = 0; i < KinectConfig.clippedLegsFilter.Length; i++)
                if (KinectConfig.clippedLegsFilter[i] != null)
                    KinectConfig.clippedLegsFilter[i].Reset();
        }
    }

    // returns true if at least one user is currently detected by the sensor
    public bool IsUserDetected()
    {
        if (!KinectSystem.IsInitialized())
            throw new Exception("[CRITICAL] Kinect is not Initialized or Device is not connected");

        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            if (KinectConfig.skeletonFrame.SkeletonData[i].eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
                return true;
        }
        return false;
    }

    // returns the UserID of Player1, or 0 if no Player1 is detected
    public uint GetUserID()
    {
        return KinectConfig.userID;
    }

    // returns the index of Player1, or 0 if no Player2 is detected
    public int GetUserIndex()
    {
        return KinectConfig.userIndex;
    }

    // returns true if the User is calibrated and ready to use
    public bool IsUserCalibrated()
    {
        return KinectConfig.userCalibrated;
    }

    // returns the raw unmodified joint position, as returned by the Kinect sensor
    public Vector3 GetRawSkeletonJointPos(uint UserId, int joint)
    {
        if (UserId == KinectConfig.userID)
            return joint >= 0 && joint < KinectConfig.userJointsPos.Length ? (Vector3)KinectConfig.skeletonFrame.SkeletonData[KinectConfig.userIndex].SkeletonPositions[joint] : Vector3.zero;
        return Vector3.zero;
    }

    // returns the User position, relative to the Kinect-sensor, in meters
    public Vector3 GetUserPosition(uint UserId)
    {
        if (UserId == KinectConfig.userID)
            return KinectConfig.userPos;

        return Vector3.zero;
    }

    // returns the User rotation, relative to the Kinect-sensor
    public Quaternion GetUserOrientation(uint UserId, bool flip)
    {
        if (UserId == KinectConfig.userID && KinectConfig.userJointsTracked[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
            return ConvertMatrixToQuat(KinectConfig.userOri, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);

        return Quaternion.identity;
    }

    // returns true if the given joint of the specified user is being tracked
    public bool IsJointTracked(uint UserId, int joint)
    {
        if (UserId == KinectConfig.userID)
            return joint >= 0 && joint < KinectConfig.userJointsTracked.Length ? KinectConfig.userJointsTracked[joint] : false;

        return false;
    }

    // convert the matrix to quaternion, taking care of the mirroring
    private Quaternion ConvertMatrixToQuat(Matrix4x4 mOrient, int joint, bool flip)
    {
        Vector4 vZ = mOrient.GetColumn(2);
        Vector4 vY = mOrient.GetColumn(1);

        if (!flip)
        {
            vZ.y = -vZ.y;
            vY.x = -vY.x;
            vY.z = -vY.z;
        }
        else
        {
            vZ.x = -vZ.x;
            vZ.y = -vZ.y;
            vY.z = -vY.z;
        }

        if (vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f)
            return Quaternion.LookRotation(vZ, vY);
        else
            return Quaternion.identity;
    }

    // returns the joint position of the specified user, relative to the Kinect-sensor, in meters
    public Vector3 GetJointPosition(uint UserId, int joint)
    {
        if (UserId == KinectConfig.userID)
            return joint >= 0 && joint < KinectConfig.userJointsPos.Length ? KinectConfig.userJointsPos[joint] : Vector3.zero;

        return Vector3.zero;
    }

    // returns the local joint position of the specified user, relative to the parent joint, in meters
    public Vector3 GetJointLocalPosition(uint UserId, int joint)
    {
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

        if (UserId == KinectConfig.userID)
            return joint >= 0 && joint < KinectConfig.userJointsPos.Length ?
                (KinectConfig.userJointsPos[joint] - KinectConfig.userJointsPos[parent]) : Vector3.zero;

        return Vector3.zero;
    }

    // returns the joint rotation of the specified user, relative to the Kinect-sensor
    public Quaternion GetJointOrientation(uint UserId, int joint, bool flip)
    {
        if (UserId == KinectConfig.userID)
        {
            if (joint >= 0 && joint < KinectConfig.userJointsOri.Length && KinectConfig.userJointsTracked[joint])
                return ConvertMatrixToQuat(KinectConfig.userJointsOri[joint], joint, flip);
        }

        return Quaternion.identity;
    }

    // returns the joint rotation of the specified user, relative to the parent joint
    public Quaternion GetJointLocalOrientation(uint UserId, int joint, bool flip)
    {
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

        if (UserId == KinectConfig.userID)
        {
            if (joint >= 0 && joint < KinectConfig.userJointsOri.Length && KinectConfig.userJointsTracked[joint])
            {
                Matrix4x4 localMat = (KinectConfig.userJointsOri[parent].inverse * KinectConfig.userJointsOri[joint]);
                return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
            }
        }

        return Quaternion.identity;
    }

    // returns the direction between baseJoint and nextJoint, for the specified user
    public Vector3 GetDirectionBetweenJoints(uint UserId, int baseJoint, int nextJoint, bool flipX, bool flipZ)
    {
        Vector3 jointDir = Vector3.zero;

        if (UserId == KinectConfig.userID)
        {
            if (baseJoint >= 0 && baseJoint < KinectConfig.userJointsPos.Length && KinectConfig.userJointsTracked[baseJoint] &&
                nextJoint >= 0 && nextJoint < KinectConfig.userJointsPos.Length && KinectConfig.userJointsTracked[nextJoint])
            {
                jointDir = KinectConfig.userJointsPos[nextJoint] - KinectConfig.userJointsPos[baseJoint];
            }
        }

        if (jointDir != Vector3.zero)
        {
            if (flipX)
                jointDir.x = -jointDir.x;

            if (flipZ)
                jointDir.z = -jointDir.z;
        }

        return jointDir;
    }

    public void DisplayColorAndUserMapSkeletonLines(KinectWrapper.NuiSkeletonData skeletonData)
    {
        // Draw skeleton (if enabled)
        if (KinectConfig.DisplaySkeletonLines)
        {
            if (KinectConfig.ComputeUserMap) // if user map is ebabled
            {
                KinectStreams.DrawSkeleton(KinectConfig.usersLblTex, ref skeletonData, ref KinectConfig.userJointsTracked);
                KinectConfig.usersLblTex.Apply();
            }
            if (KinectConfig.ComputeColorMap) // if color map is enabled
            {
                KinectStreams.DrawSkeleton(KinectConfig.usersClrTex, ref skeletonData, ref KinectConfig.userJointsTracked);
                KinectConfig.usersClrTex.Apply();
            }
        }
    }
}
