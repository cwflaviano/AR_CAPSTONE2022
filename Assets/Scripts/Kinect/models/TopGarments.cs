using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TopGarments : MonoBehaviour
{
    [HideInInspector] public Transform HipCenter;
    [HideInInspector] public Transform LeftHip;
    [HideInInspector] public Transform RightHip;

    [HideInInspector] public Transform Spine3;
    [HideInInspector] public Transform Spine2;
    [HideInInspector] public Transform Spine1;
    [HideInInspector] public Transform Spine;

    [HideInInspector] public Transform ShoulderCenter;

    [HideInInspector] public Transform LeftClavicle;
    [HideInInspector] public Transform LeftShoulder;
    [HideInInspector] public Transform LeftUpperArm;
    [HideInInspector] public Transform LeftElbow;

    [HideInInspector] public Transform RightClavicle;
    [HideInInspector] public Transform RightShoulder;
    [HideInInspector] public Transform RightUpperArm;
    [HideInInspector] public Transform RightElbow;

    public void FindAndGetTransforms()
    {
        HipCenter = GameObject.Find("HipCenter").transform;
        LeftHip = GameObject.Find("LeftHip").transform;
        RightHip = GameObject.Find("RightHip").transform;

        Spine3 = GameObject.Find("Spine3").transform;
        Spine2 = GameObject.Find("Spine2").transform;
        Spine1 = GameObject.Find("Spine1").transform;
        Spine = GameObject.Find("Spine").transform;

        ShoulderCenter = GameObject.Find("ShoulderCenter").transform;

        LeftClavicle = GameObject.Find("LeftClavicle").transform;
        LeftShoulder = GameObject.Find("LeftShoulder").transform;
        LeftUpperArm = GameObject.Find("LeftUpperArm").transform;
        LeftElbow = GameObject.Find("LeftElbow").transform;

        RightClavicle = GameObject.Find("RightClavicle").transform;
        RightShoulder = GameObject.Find("RightShoulder").transform;
        RightUpperArm = GameObject.Find("RightUpperArm").transform;
        RightElbow = GameObject.Find("RightElbow").transform;
    }
}
