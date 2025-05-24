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
            Vector3 sc = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter);
            Vector3 ls = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft);
            Vector3 rs = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight);
            Vector3 hc = KinectTracking.GetJointPosition(userId, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter);

            float yAxis = sc.y - 0.48f;
            float sx = Vector3.Distance(ls, rs);
            float sy = Vector3.Distance(sc, hc);

            model.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            model.transform.position = new Vector3(hc.x, yAxis, hc.z);
            //model.transform.position = new Vector3(hc.x, hc.y, hc.z);
        }
    }
}
