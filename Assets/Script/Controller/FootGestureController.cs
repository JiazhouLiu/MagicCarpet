using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Gesture { 
    SlideToRight,
    SlideToLeft,
    ToeRaised,
    None
}

public class FootGestureController : MonoBehaviour
{
    public Transform directionIndicator;
    public Transform mainFoot;
    public Transform mainFootToe;
    public Transform mainFootHeel;
    public Transform mainFootToeComponent;
    public FootCollision FC;
    public Transform interactiveOBJ;

    public int windowFrames = 5;    // buff frames before detecting sliding
    public float scaleFactor = 0.01f;     // scale object multiplier
    public int GlobalStaticPosCounter = 100;    // smooth people stop moving during sliding
    public float GlobalAngleToCancelGes = 20; // foot with related angle will cancel sliding
    public float GlobalRaiseFootToCancelGes = 0.18f; // foot over this height will cancel sliding

    // sliding
    private List<Vector3> mainFootLocPositions;

    private Gesture currentGesture;
    private bool passedWindow = false;
    private int staticPosCounter;

    // toe touch
    private bool toeTap = false;
    private bool selected = false;

    // Start is called before the first frame update
    void Start()
    {
        mainFootLocPositions = new List<Vector3>();
        staticPosCounter = GlobalStaticPosCounter;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Right Foot Toe Height: " + mainFootToe.position.y);
        //Debug.Log("Right Foot Heel Height: " + mainFootHeel.position.y);

        mainFootLocPositions.Add(mainFoot.localPosition);
        if (mainFootLocPositions.Count > windowFrames) {
            mainFootLocPositions.RemoveAt(0);
        }

        if (mainFootLocPositions.Count == windowFrames) {
            if (!passedWindow)
            {
                currentGesture = GestureWindowDetector();

                if (currentGesture != Gesture.None)
                {
                    passedWindow = true;
                    if(currentGesture == Gesture.SlideToLeft || currentGesture == Gesture.SlideToRight)
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

                if (currentGesture == Gesture.ToeRaised) {
                    mainFootToeComponent.GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.green);
                    if (ToeTapGestureCheck()) {
                        //Debug.Log("ToeTap");
                        RunToeTap();
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
        mainFootToeComponent.GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", Color.black);
    }

    private void RunToeTap() {
        if (!selected)
        {
            selected = true;
            if (FC.TouchedObj != null) {
                FC.TouchedObj.SetParent(mainFoot);
                FC.transform.localPosition = Vector3.zero;
            }
            
        }
        else {
            selected = false;
            if (FC.TouchedObj != null)
                FC.TouchedObj.SetParent(null);
        }
    }

    private Gesture GestureWindowDetector() {

        // sliding
        List<float> angles = new List<float>();
        List<float> distance = new List<float>();

        // toe touch
        List<float> footToeHeight = new List<float>();
        List<float> footHeelHeight = new List<float>();


        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++) {
            angles.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.up));
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));

            footHeight.Add(mainFoot.position.y);

            footToeHeight.Add(mainFootToe.position.y);
            footHeelHeight.Add(mainFootHeel.position.y);
        }

        bool slideLeft = true;
        bool slideRight = true;

        bool toeRaised = true;

        // sliding
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

        // toe touch
        foreach (float toeHeight in footToeHeight) {
            if (!(footHeight[footToeHeight.IndexOf(toeHeight)] < 0.15f && mainFootHeel.position.y < 0.02f && toeHeight > 0.1f))
                toeRaised = false;
        }

        if (toeRaised)
            return Gesture.ToeRaised;


        return Gesture.None;
    }

    private bool ToeTapGestureCheck() {
        // toe touch
        List<float> footToeHeight = new List<float>();
        List<float> footHeelHeight = new List<float>();

        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++) {

            footHeight.Add(mainFoot.position.y);

            footToeHeight.Add(mainFootToe.position.y);
            footHeelHeight.Add(mainFootHeel.position.y);
        }

        // toe touch
        foreach (float toeHeight in footToeHeight)
        {
            if (footHeight[footToeHeight.IndexOf(toeHeight)] < 0.15f && mainFootHeel.position.y < 0.03f && toeHeight <= 0.1f) {
                toeTap = true;
                return true;
            }
        }
        return false;
    }

    private bool SlideGestureCheck(Gesture gesture)
    {
        List<float> angles = new List<float>();
        List<float> distance = new List<float>();
        List<float> footHeight = new List<float>();

        // angles between right direction of right foot and different frames
        for (int i = 0; i < windowFrames - 1; i++)
        {
            angles.Add(Vector3.Angle(mainFootLocPositions[i + 1] - mainFootLocPositions[i], mainFoot.up));
            distance.Add(Vector3.Distance(mainFootLocPositions[i + 1], mainFootLocPositions[i]));
            footHeight.Add(mainFoot.position.y);
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
