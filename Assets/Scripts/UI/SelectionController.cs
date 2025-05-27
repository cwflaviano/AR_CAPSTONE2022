using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private SceneController SceneController;

    [Header("UI Button")]
    [SerializeField] private Button uniform_btn;
    [SerializeField] private Button department_btn;
    [SerializeField] private Button campus_org_btn;
    [SerializeField] private Button back_btn1;
    [SerializeField] private Button back_btn2;
    [SerializeField] private Button back_btn3;

    [Header("UI Button")]
    public Button Polo;
    public Button BLouse;
    public Button DIT;
    public Button BSBA_MM;
    public Button ATLAS;
    public Button Anglicist_Guild_B;
    public Button Anglicist_Guild_W;
    public Button TIC_B;
    public Button TIC_W;
    public Button TTOPS;
    public Button Thessalonians;
    public Button ABEES;


    private void Start()
    {
        AddButtonEventListener();
        AddEventListenerCLoth();
    }

    // add event listener to buttons
    public void AddButtonEventListener()
    {
        uniform_btn.onClick.AddListener(() => { SceneController.OpenUniformSelectionPanel(); });
        department_btn.onClick.AddListener(() => { SceneController.OpenDepartmentSelectionPanel(); });
        campus_org_btn.onClick.AddListener(() => { SceneController.OpenCampusOrgSelectionPanel(); });

        back_btn1.onClick.AddListener(() => { SceneController.BackToSelectionTypePanel();});
        back_btn2.onClick.AddListener(() => { SceneController.BackToSelectionTypePanel();});
        back_btn3.onClick.AddListener(() => { SceneController.BackToSelectionTypePanel();});
    }

    public void AddEventListenerCLoth()
    {
        Polo.onClick.AddListener(() => { SceneController.OpenARScene(); });
        BLouse.onClick.AddListener(() => { SceneController.OpenARScene(); });
        BSBA_MM.onClick.AddListener(() => { SceneController.OpenARScene(); });
        ATLAS.onClick.AddListener(() => { SceneController.OpenARScene(); });
        Anglicist_Guild_B.onClick.AddListener(() => { SceneController.OpenARScene(); });
        Anglicist_Guild_W.onClick.AddListener(() => { SceneController.OpenARScene(); });
        TIC_B.onClick.AddListener(() => { SceneController.OpenARScene(); });
        TIC_W.onClick.AddListener(() => { SceneController.OpenARScene(); });
        TTOPS.onClick.AddListener(() => { SceneController.OpenARScene(); });
        Thessalonians.onClick.AddListener(() => { SceneController.OpenARScene(); });
        ABEES.onClick.AddListener(() => { SceneController.OpenARScene(); });
    }


    // quit application - change logic to qiut application and return back to EBA
    public void QuitApplication()
    {
        Debug.Log("App Quitting");
        Application.Quit();
    }
}


