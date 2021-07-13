using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControllerColor : MonoBehaviour
{
    public enum Controller { Left, Right };

    public GameObject ObjectTooltip;
    public Material white;
    public Material yellow;
    public Material blue;
    public Controller controller;

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
            if (Xbutton != null)
                Xbutton.GetComponent<MeshRenderer>().material = white;
            if (Abutton != null)
                Abutton.GetComponent<MeshRenderer>().material = white;

            if (handgrip != null)
            {
                if (handgrip.GetChild(0).childCount == 0)
                {
                    GameObject tooltip = Instantiate(ObjectTooltip, new Vector3(0, 0, 0), Quaternion.identity, handgrip.GetChild(0));
                    
                    if (controller == Controller.Left)
                    {
                        tooltip.transform.localEulerAngles = new Vector3(200, 90, 0);
                        tooltip.transform.localPosition = new Vector3(0.0183f, 0.0151f, -0.0597f);
                    }
                    else {
                        tooltip.transform.localEulerAngles = new Vector3(150, 90, 180);
                        tooltip.transform.localPosition = new Vector3(0.0232f, -0.0037f, -0.0597f);
                    }
                    tooltip.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "Move";
                    tooltip.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = "Move";
                }
                handgrip.GetComponent<MeshRenderer>().material = yellow;
            }

            if (trigger != null)
            {
                if (trigger.GetChild(0).childCount == 0)
                {
                    GameObject tooltip = Instantiate(ObjectTooltip, new Vector3(0, 0, 0), Quaternion.identity, trigger.GetChild(0));

                    tooltip.transform.eulerAngles = transform.eulerAngles;
                    tooltip.transform.localEulerAngles = new Vector3(tooltip.transform.localEulerAngles.x - 90, tooltip.transform.localEulerAngles.y, tooltip.transform.localEulerAngles.z + 180);

                    if (controller == Controller.Left)
                    {
                        tooltip.transform.localPosition = new Vector3(-0.0722f, 0.0121f, -0.007f);
                    }
                    else
                    {
                        tooltip.transform.localEulerAngles = new Vector3(-80,0,180);
                        tooltip.transform.localPosition = new Vector3(-0.0722f, -0.0208f, -0.0235f);
                    }

                    tooltip.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "Select";
                    tooltip.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = "Select";
                }
                trigger.GetComponent<MeshRenderer>().material = blue;
            }

            
        }
    }
}