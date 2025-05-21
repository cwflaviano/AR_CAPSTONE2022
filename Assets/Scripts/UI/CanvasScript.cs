using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    public GameObject MainMenuCanvas;
    public GameObject StudentCanvas;
    public GameObject FacultyCanvas;
    //public GameObject ARCanvas;
    //public GameObject ARUICanvas;


    public void SetCanvasActiveStatus(GameObject canvas, bool setActive)
    {
            canvas.SetActive(setActive);
    }

    private void Awake()
    {
        SetCanvasActiveStatus(MainMenuCanvas, true);
        SetCanvasActiveStatus(StudentCanvas, false);
        SetCanvasActiveStatus(FacultyCanvas, false);
        //SetCanvasActiveStatus(ARCanvas, false);
        //SetCanvasActiveStatus(ARUICanvas, false);
    }
}
