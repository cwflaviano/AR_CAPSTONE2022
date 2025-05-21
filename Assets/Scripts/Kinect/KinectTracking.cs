using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class KinectTracking : MonoBehaviour
{
    [SerializeField] private KinectConfig KinectConfig;
    [SerializeField] private KinectSystem KinectSystem;
    [SerializeField] private KinectStreams KinectStreams;
    [SerializeField] private KinectClothAugmenter KinectClothAugmenter;
    [SerializeField] private Debugging Debugging;

    public void PollSkeleton()
    {
        if (KinectWrapper.PollSkeleton(ref KinectConfig.smoothParameters, ref KinectConfig.skeletonFrame))
            ProcessSkeleton();
    }

    /// <summary>
    /// PROCESSING SKELETON LOGICS
    /// </summary>
    public void ProcessSkeleton()
    {
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - KinectConfig.lastNuiTime;

        // Resets the array of user ids and list of skeleton data
        KinectConfig.trackUserIDs = new uint[KinectWrapper.Constants.NuiSkeletonCount];
        KinectConfig.userSkeletonData.Clear();

        // Track all skeletons (max 6 per frame)
        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            KinectWrapper.NuiSkeletonData skeletonData = KinectConfig.skeletonFrame.SkeletonData[i];
            if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                KinectConfig.trackUserIDs[i] = skeletonData.dwTrackingID;
                KinectConfig.userSkeletonData[skeletonData.dwTrackingID] = skeletonData;

                Debugging.text1.color = Color.green;
                Debugging.text1.text = "USER(s) DETECTED!";
            }
        }

        // Check if current calibrated user is still valid
        bool isCurrentUserValid = false;
        if (KinectConfig.userCalibrated)
        {
            // Verify current user still exists and meets requirements
            if (KinectConfig.userSkeletonData.TryGetValue(KinectConfig.userID, out var currentUserData))
            {
                Vector3 currentPos = KinectConfig.kinectToWorld.MultiplyPoint3x4(currentUserData.Position);
                float currentDistance = Mathf.Abs(currentPos.z);

                isCurrentUserValid = currentUserData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked &&
                                   currentDistance >= KinectConfig.MinUserDistance &&
                                   currentDistance <= KinectConfig.MaxUserDistance;
            }

            if (!isCurrentUserValid)
            {
                // Current user is no longer valid - reset calibration
                KinectConfig.userCalibrated = false;
                KinectConfig.userID = 0;
                Debugging.text2.color = Color.yellow;
                Debugging.text2.text = "User lost - recalibrating...";
                KinectClothAugmenter.isAugmented = false;
                Debugging.text5.text = "...";
            }
        }

        // Process skeleton data if we have tracked users
        if (KinectConfig.userSkeletonData != null && KinectConfig.userSkeletonData.Count > 0)
        {
            // Only look for new user if current one isn't valid
            if (!isCurrentUserValid)
            {
                uint posibleUserToBeTrack = 0;
                float closestUser = float.MaxValue;

                // Find closest valid user
                foreach (uint userId in KinectConfig.trackUserIDs)
                {
                    if (userId == 0) continue; // Skip empty slots

                    KinectWrapper.NuiSkeletonData userSkeleton = KinectConfig.userSkeletonData[userId];
                    KinectConfig.skeletonPos = KinectConfig.kinectToWorld.MultiplyPoint3x4(userSkeleton.Position);
                    float distance = Mathf.Abs(KinectConfig.skeletonPos.z);

                    if (distance < closestUser &&
                        distance >= KinectConfig.MinUserDistance &&
                        distance <= KinectConfig.MaxUserDistance)
                    {
                        posibleUserToBeTrack = userId;
                        closestUser = distance;
                    }
                }

                // Calibrate new user if found
                if (posibleUserToBeTrack != 0)
                {
                    KinectWrapper.NuiSkeletonData userSkeletonData = KinectConfig.userSkeletonData[posibleUserToBeTrack];

                    KinectConfig.userCalibrated = true;
                    KinectConfig.userID = posibleUserToBeTrack;
                    KinectConfig.userIndexes = userSkeletonData.dwUserIndex;
                    KinectConfig.userDistance = closestUser;

                    Debugging.text2.color = Color.white;
                    Debugging.text2.text = $"USER {posibleUserToBeTrack}, calibrated!";

                    // Reset filters
                    if (KinectConfig.trackingStateFilter[0] != null)
                        KinectConfig.trackingStateFilter[0].Reset();
                    if (KinectConfig.boneOrientationFilter[0] != null)
                        KinectConfig.boneOrientationFilter[0].Reset();
                }
            }

            // Process calibrated user
            if (KinectConfig.userCalibrated)
            {

                // Use the current user ID (might be existing or newly calibrated)
                uint currentUserId = KinectConfig.userID;
                var skeletonData = KinectConfig.userSkeletonData[currentUserId];

                // Update distance display
                Vector3 currentPos = KinectConfig.kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
                float currentDistance = Mathf.Abs(currentPos.z);
                Debugging.text3.text = $"DISTANCE: {currentDistance:F2} meters";

                // Process skeleton data
                KinectConfig.userPos = currentPos;
                KinectConfig.trackingStateFilter[0].UpdateFilter(ref skeletonData);

                if (KinectConfig.UseClippedLegsFilter && KinectConfig.clippedLegsFilter[0] != null)
                    KinectConfig.clippedLegsFilter[0].FilterSkeleton(ref skeletonData, deltaNuiTime);

                if (KinectConfig.UseSelfIntersectionConstraint && KinectConfig.selfIntersectionConstraint != null)
                    KinectConfig.selfIntersectionConstraint.Constrain(ref skeletonData);

                // Process joints
                for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
                {
                    bool jointTracked = KinectConfig.IgnoreInferredJoints
                        ? (int)skeletonData.eSkeletonPositionTrackingState[j] == KinectConfig.stateTracked
                        : (int)skeletonData.eSkeletonPositionTrackingState[j] != KinectConfig.stateNotTracked;

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
            }
        }
        else
        {
            // No users detected
            KinectConfig.userCalibrated = false;
            KinectConfig.userID = 0;

            Debugging.text1.color = Color.red;
            Debugging.text1.text = "NO USERS DETECTED!";
            Debugging.text2.color = Color.white;
            Debugging.text2.text = "...";
            Debugging.text3.color = Color.white;
            Debugging.text3.text = "...";
            Debugging.text5.text = "...";
            KinectClothAugmenter.isAugmented = false;
        }
    }


    // second process skeleton logic - not used
    public void ProcessSkeleton1()
    {
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - KinectConfig.lastNuiTime;

        // resets the array of user ids and list of skeleton data
        KinectConfig.trackUserIDs = new uint[KinectWrapper.Constants.NuiSkeletonCount];
        KinectConfig.userSkeletonData.Clear();

        // loops through all skeleton, max of 6 skeleton per frame, check if skeleton is tracked then store its tracking id and data to an array.
        for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
        {
            KinectWrapper.NuiSkeletonData skeletonData = KinectConfig.skeletonFrame.SkeletonData[i];
            if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
            {
                KinectConfig.trackUserIDs[i] = skeletonData.dwTrackingID;
                KinectConfig.userSkeletonData[skeletonData.dwTrackingID] = skeletonData;

                Debugging.text1.color = Color.green;
                Debugging.text1.text = "USER(s) DETECTED!";
            }
        }

        // calibrate and process all active user skeleton data
        if(KinectConfig.userSkeletonData != null && KinectConfig.userSkeletonData.Count > 0)
        {
            uint posibleUserToBeTrack = 0;
            float closestUser = float.MaxValue;

            // getting each tracked skeleton and calibrating it whos closest and whos not
            foreach (uint userId in KinectConfig.trackUserIDs)
            {
                KinectWrapper.NuiSkeletonData userSkeleton = KinectConfig.userSkeletonData[userId];
                KinectConfig.skeletonPos = KinectConfig.kinectToWorld.MultiplyPoint3x4(userSkeleton.Position);
                float distance = Mathf.Abs(KinectConfig.skeletonPos.z);

                if (distance < closestUser && distance >= KinectConfig.MinUserDistance && distance <= KinectConfig.MaxUserDistance)
                {
                    posibleUserToBeTrack = userId;
                    closestUser = distance;
                }
            }

            // calibrate user
            if (posibleUserToBeTrack != 0 && !KinectConfig.userCalibrated)
            {
                KinectWrapper.NuiSkeletonData userSkeletonData = KinectConfig.userSkeletonData[posibleUserToBeTrack];

                KinectConfig.userCalibrated = true;
                KinectConfig.userID = posibleUserToBeTrack;
                KinectConfig.userIndexes = userSkeletonData.dwUserIndex;
                KinectConfig.userDistance = closestUser;

                Debugging.text2.color = Color.white;
                Debugging.text2.text = $"USER {posibleUserToBeTrack.ToString()}, is Calibrated!";

                if (KinectConfig.trackingStateFilter[0] != null)
                    KinectConfig.trackingStateFilter[0].Reset();
                if (KinectConfig.boneOrientationFilter[0] != null)
                    KinectConfig.boneOrientationFilter[0].Reset();
            }

            if (KinectConfig.userCalibrated)
            {
                Debugging.text3.text = $"DISTANCE: {closestUser.ToString()} Meter(s)";

                var skeletonData = KinectConfig.userSkeletonData[posibleUserToBeTrack];

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
            }
        }
        else
        {
            Debugging.text1.color = Color.red;
            Debugging.text1.text = "NO USERS TO DETECT!";
            Debugging.text2.color = Color.white;
            Debugging.text2.text = "...";
            Debugging.text3.color = Color.white;
            Debugging.text3.text = "...";
        }
    }

    // thrid process skeleton logic - not used
    public void ProcessSkeleton2()
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
                    Debugging.text1.text = "User Tracked";

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
                    Debugging.text1.text = "No Closest User within 1 to 3 meters";
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
                Debugging.text2.text = $"User: {KinectConfig.userID.ToString()}, DISTANCE: {distance.ToString()}";

                if (distance < KinectConfig.MinUserDistance || distance > KinectConfig.MaxUserDistance)
                {
                    Debug.LogWarning($"[TRACKING] User: {KinectConfig.userID.ToString()}, Is not within 1 to 3 meters");
                    Debugging.text2.text = $"User: {KinectConfig.userID.ToString()}, Is not within 1 to 3 meters";
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

                Debugging.text3.text = "...";
                break;
            }
            
            if(KinectConfig.userCalibrated && skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.NotTracked)
            {
                Debugging.text1.text = "No User to be tracked";
                Debugging.text2.text = "...";
                Debugging.text3.text = "User is out of view!";
                KinectConfig.userCalibrated = false;
                KinectConfig.userID = 0;
                KinectConfig.userIndex = -1;
                KinectConfig.userDistance = -1;
            }
        }
    }



    /// <summary>
    /// OTHER FUNCTIONS
    /// </summary>
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
