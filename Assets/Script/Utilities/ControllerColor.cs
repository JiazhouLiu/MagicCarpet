using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControllerColor : MonoBehaviour
{
    public GameObject ObjectTooltip;
    public Material white;
    public Material green;
    public Material yellow;
    public Material blue;

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount > 0)
        {
            Transform body = transform.Find("body");
            Transform thumbstick = transform.Find("thumbstick");
            Transform menu_button = transform.Find("menu_button");
            Transform yButton = transform.Find("Y");
            Transform bButton = transform.Find("B");

            Transform handgrip = transform.Find("handgrip");
            Transform trigger = transform.Find("trigger");

            Transform Xbutton = transform.Find("X");
            Transform Abutton = transform.Find("A");

            if (body != null)
                body.GetComponent<MeshRenderer>().material = white;
            if (thumbstick != null)
                thumbstick.GetComponent<MeshRenderer>().material = white;
            if (menu_button != null)
                menu_button.GetComponent<MeshRenderer>().material = white;
            if (yButton != null)
                yButton.GetComponent<MeshRenderer>().material = white;
            if (bButton != null)
                bButton.GetComponent<MeshRenderer>().material = white;

            if (handgrip != null)
            {
                if (handgrip.GetChild(0).childCount == 0)
                {
                    GameObject tooltip = Instantiate(ObjectTooltip, new Vector3(0, 0, 0), Quaternion.identity, handgrip.GetChild(0));
                    tooltip.transform.localPosition = Vector3.zero + transform.right * 0.1f;
                    tooltip.transform.eulerAngles = transform.eulerAngles;
                    tooltip.transform.localEulerAngles += Vector3.left * 90;
                }
                handgrip.GetComponent<MeshRenderer>().material = yellow;
            }

            if (trigger != null)
            {
                if (trigger.GetChild(0).childCount == 0)
                {
                    GameObject tooltip = Instantiate(ObjectTooltip, new Vector3(0, 0, 0), Quaternion.identity, trigger.GetChild(0));
                    tooltip.transform.localPosition = Vector3.zero + transform.right * 0.1f;
                    tooltip.transform.eulerAngles = transform.eulerAngles;
                    tooltip.transform.localEulerAngles += Vector3.left * 90;

                }
                trigger.GetComponent<MeshRenderer>().material = blue;
            }

            if (Xbutton != null)
            {
                if (Xbutton.GetChild(0).childCount == 0)
                {
                    GameObject tooltip = Instantiate(ObjectTooltip, new Vector3(0, 0, 0), Quaternion.identity, Xbutton.GetChild(0));
                    tooltip.transform.localPosition = Vector3.zero + transform.right * 0.1f;
                    tooltip.transform.eulerAngles = transform.eulerAngles;
                    tooltip.transform.localEulerAngles += Vector3.left * 90;

                }
                Xbutton.GetComponent<MeshRenderer>().material = green;
            }

            if (Abutton != null)
            {
                if (Abutton.GetChild(0).childCount == 0)
                {
                    GameObject tooltip = Instantiate(ObjectTooltip, new Vector3(0, 0, 0), Quaternion.identity, Abutton.GetChild(0));
                    tooltip.transform.localPosition = Vector3.zero + transform.right * 0.1f;
                    tooltip.transform.eulerAngles = transform.eulerAngles;
                    tooltip.transform.localEulerAngles += Vector3.left * 90;

                }
                Abutton.GetComponent<MeshRenderer>().material = green;
            }
        }
    }
}