using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothManager : MonoBehaviour
{ 
    [SerializeField] public TopGarments TopGarments;
    [SerializeField] public GameObject DemoModel;

    [HideInInspector] public bool isConfigured = false;

    public void ManageModels()
    {   
        if(!DemoModel.GetComponent<TopGarments>())
            DemoModel.AddComponent<TopGarments>();

        TopGarments.FindAndGetTransforms();
        isConfigured = true;
    }


    public void EnableModel()
    {
        isConfigured = false;
        DemoModel.SetActive(true);
    }

    public void DisableModel()
    {
        isConfigured = false;
        DemoModel.SetActive(false);
    }
}
