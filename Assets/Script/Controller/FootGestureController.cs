using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Gesture { 
    SlideToRight,
    SlideToLeft,
    None
}

public class FootGestureController : MonoBehaviour
{
    public Transform directionIndicator;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform interactiveOBJ;

    public int windowFrames = 5;
    public float scaleFactor = 0.01f;

    private List<Vector3> rightFootLocPositions;

    private Gesture currentGesture;
    private bool passedWindow = false;

    // Start is called before the first frame update
    void Start()
    {
        rightFootLocPositions = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        rightFootLocPositions.Add(rightFoot.localPosition);
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
                    Debug.Log("Slide Left");
                    directionIndicator.Find("ArrowLeft").GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.green);
                    if (!SlideGestureCheck(currentGesture))
                    {
                        passedWindow = false;
                        ResetIndicator();
                    }
                }

                if (currentGesture == Gesture.SlideToRight)
                {
                    Debug.Log("Slide Right");
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
            angles.Add(Vector3.Angle(rightFootLocPositions[i + 1] - rightFootLocPositions[i], rightFoot.right));
            distance.Add(Vector3.Distance(rightFootLocPositions[i + 1], rightFootLocPositions[i]));
            footHeight.Add(rightFoot.position.y);
        }

        bool slideLeft = true;
        bool slideRight = true;

        foreach (float angle in angles.ToArray())
        {
            if (angle == 0 && distance[angles.IndexOf(angle)] == 0) // if stationary
                angles.Remove(angle); // remove 0 for validation
            else {
                if (footHeight[angles.IndexOf(angle)] > 0.18f) {
                    slideRight = false;
                    slideLeft = false;
                }
                if (angle > 10) // if not to right
                    slideRight = false;
                if (angle < 170) // if not to left
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
            angles.Add(Vector3.Angle(rightFootLocPositions[i + 1] - rightFootLocPositions[i], rightFoot.right));
            distance.Add(Vector3.Distance(rightFootLocPositions[i + 1], rightFootLocPositions[i]));
            footHeight.Add(rightFoot.position.y);
        }

        foreach (float angle in angles.ToArray())
        {
            if (angle == 0 && distance[angles.IndexOf(angle)] == 0) // if stationary
                angles.Remove(angle); // remove 0 for validation
            else
            {
                if (gesture == Gesture.SlideToRight)
                {
                    if (angle > 10 || footHeight[angles.IndexOf(angle)] > 0.18f)
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
                    if (angle < 170 || footHeight[angles.IndexOf(angle)] > 0.18f) // if not to left
                        return false;
                    else
                    {
                        Vector3 resultScale = interactiveOBJ.localScale - distance[angles.IndexOf(angle)] * Vector3.one * scaleFactor;
                        if (resultScale.x > 0)
                            interactiveOBJ.localScale = resultScale;
                    }
                }
            }
        }

        if (angles.Count == 0)
            return false;

        return true;
    }
}
