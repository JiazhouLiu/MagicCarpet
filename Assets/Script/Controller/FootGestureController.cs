﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Gesture { 
    SlideToRight,
    SlideToLeft,
    RightToeTap,
    LeftToeTap,
    None
}

public class FootGestureController : MonoBehaviour
{
    public Transform directionIndicator;
    public Foot leftFoot;
    public Foot rightFoot;
    public Transform interactiveOBJ;

    public int windowFrames = 5;
    public float scaleFactor = 0.01f;
    public int GlobalStaticPosCounter = 100;
    public float GlobalAngleToCancelGes = 20;
    public float GlobalRaiseFootToCancelGes = 0.18f;

    private List<Vector3> rightFootLocPositions;

    private Gesture currentGesture;
    private bool passedWindow = false;
    private int staticPosCounter;

    // Start is called before the first frame update
    void Start()
    {
        rightFootLocPositions = new List<Vector3>();
        staticPosCounter = GlobalStaticPosCounter;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Left Foot Toe Height: " + leftFoot.Toe.position.y);
        Debug.Log("Right Foot Toe Height: " + rightFoot.Toe.position.y);

        rightFootLocPositions.Add(rightFoot.CurrentTransform.localPosition);
        if (rightFootLocPositions.Count > windowFrames) {
            rightFootLocPositions.RemoveAt(0);
        }

        if (rightFootLocPositions.Count == windowFrames) {
            if (!passedWindow)
            {
                currentGesture = GestureWindowDetector();

                if (currentGesture != Gesture.None)
                {
                    passedWindow = true;
                    directionIndicator.gameObject.SetActive(true);
                }
                else
                {
                    ResetIndicator();
                }
            }
            else {
                if (currentGesture == Gesture.SlideToLeft)
                {
                    directionIndicator.Find("ArrowLeft").GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.green);
                    if (!SlideGestureCheck(currentGesture))
                    {
                        passedWindow = false;
                        ResetIndicator();
                    }
                }

                if (currentGesture == Gesture.SlideToRight)
                {
                    directionIndicator.Find("ArrowRight").GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.green);
                    if (!SlideGestureCheck(currentGesture))
                    {
                        passedWindow = false;
                        ResetIndicator();
                    }
                }
            }
        }
    }

    private void ResetIndicator() {
        foreach (Transform t in directionIndicator)
            t.GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.black);
        directionIndicator.gameObject.SetActive(false);
    }

    private Gesture GestureWindowDetector() {

        List<float> angles = new List<float>();
        List<float> distance = new List<float>();
        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++) {
            angles.Add(Vector3.Angle(rightFootLocPositions[i + 1] - rightFootLocPositions[i], rightFoot.CurrentTransform.up));
            distance.Add(Vector3.Distance(rightFootLocPositions[i + 1], rightFootLocPositions[i]));
            footHeight.Add(rightFoot.CurrentTransform.position.y);
        }

        bool slideLeft = true;
        bool slideRight = true;

        foreach (float angle in angles.ToArray())
        {
            if (angle == 0 && distance[angles.IndexOf(angle)] == 0) // if stationary
                angles.Remove(angle); // remove 0 for validation
            else {
                if (footHeight[angles.IndexOf(angle)] > GlobalRaiseFootToCancelGes) {
                    slideRight = false;
                    slideLeft = false;
                }
                if (angle > GlobalAngleToCancelGes) // if not to right
                    slideRight = false;
                if (angle < (180 - GlobalAngleToCancelGes)) // if not to left
                    slideLeft = false;
            }
        }

        if (angles.Count > windowFrames / 2) { // if valid angles are more than half
            if(slideLeft && slideRight)
                Debug.Log("ERROR");

            if (slideLeft)
                return Gesture.SlideToLeft;

            if (slideRight)
                return Gesture.SlideToRight;
        } 

        return Gesture.None;
    }

    private bool SlideGestureCheck(Gesture gesture)
    {
        List<float> angles = new List<float>();
        List<float> distance = new List<float>();
        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++)
        {
            angles.Add(Vector3.Angle(rightFootLocPositions[i + 1] - rightFootLocPositions[i], rightFoot.CurrentTransform.up));
            distance.Add(Vector3.Distance(rightFootLocPositions[i + 1], rightFootLocPositions[i]));
            footHeight.Add(rightFoot.CurrentTransform.position.y);
        }

        foreach (float angle in angles.ToArray())
        {
            if (angle == 0 && distance[angles.IndexOf(angle)] == 0) // if stationary
                angles.Remove(angle); // remove 0 for validation
            else
            {
                if (gesture == Gesture.SlideToRight)
                {
                    if (angle > GlobalAngleToCancelGes || footHeight[angles.IndexOf(angle)] > GlobalRaiseFootToCancelGes)
                    {
                        return false;
                    } // if not to right
                    else
                    {
                        Vector3 resultScale = interactiveOBJ.localScale + distance[angles.IndexOf(angle)] * Vector3.one * scaleFactor;
                        if (resultScale.x < 2)
                            interactiveOBJ.localScale = resultScale;
                    }

                }


                if (gesture == Gesture.SlideToLeft)
                {
                    if (angle < (180 - GlobalAngleToCancelGes) || footHeight[angles.IndexOf(angle)] > GlobalRaiseFootToCancelGes) {
                        return false;
                    } // if not to left
                    else
                    {
                        Vector3 resultScale = interactiveOBJ.localScale - distance[angles.IndexOf(angle)] * Vector3.one * scaleFactor;
                        if (resultScale.x > 0)
                            interactiveOBJ.localScale = resultScale;
                    }
                }
            }
        }

        if (angles.Count == 0) {
            if (staticPosCounter-- == 0) {
                staticPosCounter = GlobalStaticPosCounter;
                return false;
            }
        }
            

        return true;
    }
}
