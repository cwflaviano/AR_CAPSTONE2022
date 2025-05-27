using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private LoadingUI LoadingUI;

    [Header("data to persist across scenes")]
    [SerializeField] private GameObject[] persitentGameobjects;

    [Header("UI")]
    [SerializeField] private GameObject selectionTypePanel;
    [SerializeField] private GameObject uniformPanel;
    [SerializeField] private GameObject deparmentsPanel;
    [SerializeField] private GameObject campusOrgPanel;

    private string menu = "Main";
    private string AR = "AR";


    // open uniform panel
    public void OpenUniformSelectionPanel()
    {
        if(SceneManager.GetActiveScene().name == menu)
        {
            selectionTypePanel.SetActive(false);
            uniformPanel.SetActive(true);
            return;
        }
    }

    // open deparment panel
    public void OpenDepartmentSelectionPanel()
    {
        if (SceneManager.GetActiveScene().name == menu)
        {
            selectionTypePanel.SetActive(false);
            deparmentsPanel.SetActive(true);
            return;
        }
    }

    // open campus / org panel
    public void OpenCampusOrgSelectionPanel()
    {
        if (SceneManager.GetActiveScene().name == menu)
        {
            selectionTypePanel.SetActive(false);
            campusOrgPanel.SetActive(true);
            return;
        }
    }

    public void BackToSelectionTypePanel()
    {
        if(uniformPanel.activeSelf)
            uniformPanel.SetActive(false);
        if (deparmentsPanel.activeSelf)
            deparmentsPanel.SetActive(false);
        if (campusOrgPanel.activeSelf)
            campusOrgPanel.SetActive(false);

        selectionTypePanel.SetActive(true);
    }

    // load AR scene
    public void OpenARScene()
    {
        StartCoroutine(SmoothLoadScene(AR));
    }

    // load menu scene
    public void OpenMainMenuScene()
    {
        StartCoroutine(SmoothLoadScene(menu));
    }

    // smooth scene loader / show loading UI
    private IEnumerator SmoothLoadScene(string sceneName)
    {
        LoadingUI.StartAnimation();
        yield return new WaitForSeconds(2f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    // reload current active scene
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // disable trpe selection ui
    public void DisableTypeSelectionMenu()
    {
        if (selectionTypePanel.activeSelf)
            selectionTypePanel.SetActive(false);
        else
            selectionTypePanel.SetActive(true);
    }
}