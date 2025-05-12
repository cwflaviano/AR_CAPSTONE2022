using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TopGarments : MonoBehaviour
{
    public GameObject model;

    public Transform HipCenter;
    public Transform LeftHip;
    public Transform RightHip;

    public Transform Spine3;
    public Transform Spine2;
    public Transform Spine1;
    public Transform Spine;

    public Transform ShoulderCenter;

    public Transform LeftClavicle;
    public Transform LeftShoulder;
    public Transform LeftUpperArm;
    public Transform LeftElbow;

    public Transform RightClavicle;
    public Transform RightShoulder;
    public Transform RightUpperArm;
    public Transform RightElbow;

    // find and assigne bones, if found assigned its transform, else throw and exception
    public void FindAndAssignTransforms()
    {
        if(HipCenter != null)
            HipCenter = (HipCenter == null) ? GameObject.Find("HipCenter").transform : throw new System.Exception("HipCenter is null, cant find this bone");

        if (LeftHip != null)
            LeftHip = (LeftHip == null) ? GameObject.Find("LeftHip").transform : throw new System.Exception("LeftHip is null, cant find this bone");

        if (RightHip != null)
            RightHip = (RightHip == null) ? GameObject.Find("RightHip").transform : throw new System.Exception("RightHip is null, cant find this bone");

        if (Spine3 != null)
            Spine3 = (Spine3 == null) ? GameObject.Find("Spine3").transform : throw new System.Exception("Spine3 is null, cant find this bone");

        if (Spine2 != null)
            Spine2 = (Spine2 == null) ? GameObject.Find("Spine2").transform : throw new System.Exception("Spine2 is null, cant find this bone");

        if (Spine1 != null)
            Spine1 = (Spine1 == null) ? GameObject.Find("Spine1").transform : throw new System.Exception("Spine1 is null, cant find this bone");

        if (Spine != null)
            Spine = (Spine == null) ? GameObject.Find("Spine").transform : throw new System.Exception("Spine is null, cant find this bone");

        if (ShoulderCenter != null)
            ShoulderCenter = (ShoulderCenter == null) ? GameObject.Find("ShoulderCenter").transform : throw new System.Exception("ShoulderCenter is null, cant find this bone");

        if (LeftClavicle != null)
            LeftClavicle = (LeftClavicle == null) ? GameObject.Find("LeftClavicle").transform : throw new System.Exception("LeftClavicle is null, cant find this bone");

        if (LeftShoulder != null)
            LeftShoulder = (LeftShoulder == null) ? GameObject.Find("LeftShoulder").transform : throw new System.Exception("LeftShoulder is null, cant find this bone");

        if (LeftUpperArm != null)
            LeftUpperArm = (LeftUpperArm == null) ? GameObject.Find("LeftUpperArm").transform : throw new System.Exception("LeftUpperArm is null, cant find this bone");

        if (LeftElbow != null)
            LeftElbow = (LeftElbow == null) ? GameObject.Find("LeftElbow").transform : throw new System.Exception("LeftElbow is null, cant find this bone");

        if (RightClavicle != null)
            RightClavicle = (RightClavicle == null) ? GameObject.Find("RightClavicle").transform : throw new System.Exception("RightClavicle is null, cant find this bone");

        if (RightShoulder != null)
            RightShoulder = (RightShoulder == null) ? GameObject.Find("RightShoulder").transform : throw new System.Exception("RightShoulder is null, cant find this bone");

        if (RightUpperArm != null)
            RightUpperArm = (RightUpperArm == null) ? GameObject.Find("RightUpperArm").transform : throw new System.Exception("RightUpperArm is null, cant find this bone");

        if (RightElbow != null)
            RightElbow = (RightElbow == null) ? GameObject.Find("RightElbow").transform : throw new System.Exception("RightElbow is null, cant find this bone");
    }
}
