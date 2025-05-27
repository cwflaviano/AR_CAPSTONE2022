using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingUIContainer;
    [SerializeField] private GameObject loadingUI;
    private Animator animate;

    private void Awake()
    {
        if (loadingUI != null)
        {
            animate = loadingUI.GetComponent<Animator>();
        }
    }

    // start loading animation
    public void StartAnimation()
    {
        if (animate != null)
        {
            loadingUIContainer.SetActive(true);
            animate.Play("LogoAnimation");       
        }
    }

    // stop loading animation
    public void StopAnimation()
    {
        if (animate != null)
        {
            animate.Play("idle");
            loadingUIContainer.SetActive(false);
        }
    }
}
