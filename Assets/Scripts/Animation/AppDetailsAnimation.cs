using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AppDetailsAnimation : MonoBehaviour
{
    [SerializeField] private Button dropdownAnimation;
    [SerializeField] private GameObject detailsMenuAnimation;


    private bool isOpen = false;

    private void Start()
    {
        if(dropdownAnimation.GameObject().activeSelf)
        {
            dropdownAnimation.GetComponent<RectTransform>().localPosition = new Vector3(0.4649966f, -178.1821f, 0f);
            if (dropdownAnimation == null && detailsMenuAnimation == null) return;
            dropdownAnimation.GetComponent<Animator>().Play("dropdown_btn_animation");
        }
    }

    public void OnClickOpenAppDetailsMenuApplication()
    {
        StartCoroutine(OpenDetailsMenuAnim());
        ChangeButtonSpriteRotationOnAnimate();

        if(isOpen) isOpen = false;
        else isOpen = true;
    }

    private IEnumerator OpenDetailsMenuAnim()
    {
        Animator animate = detailsMenuAnimation.GetComponent<Animator>();
        if(isOpen)
            animate.SetBool("show", true);
        else 
            animate.SetBool("show", false);
        yield return new WaitForSeconds(1.3f);
    }

    private void ChangeButtonSpriteRotationOnAnimate()
    {
        if(isOpen)
            dropdownAnimation.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 180);
        else
            dropdownAnimation.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
    }
}
