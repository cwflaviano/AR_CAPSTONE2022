using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ENUMS - for static variables
/// </summary>
public enum Smoothing : int { None, Default, Medium, Aggressive }


public class KinectConfig : MonoBehaviour
{
    /// <summary>
    /// KINECT DEVICE SENSOR SETTING 
    ///         - for accurate depth sensing,
    ///         - accurate body tracking
    /// </summary>
    // device sensor setting
    [Header("SENSOR CONFIG")]
    public float SensorHeight = 1f;
    public int SensorAngle = 0;

    // depth sensing / bone tracking
    [Header("TRACKING CONFIG")]
    public float MinUserDistance = 1.0f;
    public float MaxUserDistance = 3.0f;
    public bool DetectClosestUser = true;
    public bool IgnoreInferredJoints = true;

    // tracked bone smoothing
    [Header("SKELETON SMOOTHING CONFIG")]
    public Smoothing smoothing = Smoothing.Default;
    public bool UseBoneOrientationsFilter = true;
    public bool UseClippedLegsFilter = true;
    public bool UseBoneOrientationsConstraint = true;
    public bool UseSelfIntersectionConstraint = false;

    // skeleton / bones / joints
    public KinectWrapper.NuiSkeletonFrame skeletonFrame;
    public KinectWrapper.NuiTransformSmoothParameters smoothParameters;

    // bone filters
    [Header("BONE/JOINT FILTERS CONFIG")]
    public TrackingStateFilter[] trackingStateFilter;
    public BoneOrientationsFilter[] boneOrientationFilter;
    public ClippedLegsFilter[] clippedLegsFilter;
    public BoneOrientationsConstraint boneConstraintsFilter;
    public SelfIntersectionConstraint selfIntersectionConstraint;
    public float lastNuiTime;

    // Skeleton tracking states, positions and joints' orientations
    [HideInInspector] public Vector3 userPos;
    [HideInInspector] public Matrix4x4 userOri;
    [HideInInspector] public bool[] userJointsTracked;
    [HideInInspector] public bool[] userPrevTracked;
    [HideInInspector] public Vector3[] userJointsPos;
    [HideInInspector] public Matrix4x4[] userJointsOri;
    [HideInInspector] public KinectWrapper.NuiSkeletonBoneOrientation[] jointOrientations;

    [HideInInspector] public Matrix4x4 kinectToWorld, flipMatrix;

    [HideInInspector] public bool KinectInitialized = false;
    [HideInInspector] public bool userCalibrated = false;

    [HideInInspector] public uint userID;
    [HideInInspector] public uint userTrackedIndex;
    [HideInInspector] public int userIndex;
    [HideInInspector] public float userDistance;
    [HideInInspector] public Vector3 skeletonPos;

    [HideInInspector] public const int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
    [HideInInspector] public const int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;

    /// <summary>
    /// STREAMS - COLOR STREAMS, DEPTH STREAMS
    ///         - for displaying / visualizing bones
    ///         - or camera feed from kinect device
    /// </summary>
    // public COMPONENTS
    [Header("STREAMS CONFIG")]
    public bool ComputeUserMap = false;
    public bool ComputeColorMap = true;
    public bool DisplayUserMap = false;
    public bool DisplayColorMap = false;
    public bool DisplaySkeletonLines = false;
    public float DisplayMapsWidthPercent = 20f;
    public IntPtr colorStreamHandle;
    public IntPtr depthStreamHandle;

    // [HideInInspector] public COMPONENTS
    [HideInInspector] public Color32[] colorImage;
    [HideInInspector] public byte[] usersColorMap;
    [HideInInspector] public Texture2D usersLblTex;

    [HideInInspector] public Color32[] usersMapColors;
    [HideInInspector] public ushort[] usersPrevState;
    [HideInInspector] public Rect usersMapRect;
    [HideInInspector] public int usersMapSize;
    [HideInInspector] public Texture2D usersClrTex;
    [HideInInspector] public Rect usersClrRect;
    [HideInInspector] public ushort[] usersDepthMap;
    [HideInInspector] public float[] usersHistogramMap;
}
