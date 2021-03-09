using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> bodyPositions;
    //public List<Vector3> bodyRotations;
    public UnityEvent onRecognise;
    public UnityEvent onStop;
    public bool repeatable;
}
public class Gestures : MonoBehaviour
{
    public SteamVR_TrackedController controller;
    public GameObject Root;
    private List<GameObject> relativeTo = new List<GameObject>();
    public float threshold = 1.5f;
    //public float angleThreshold = 10.0f;
    public List<Gesture> gestures = new List<Gesture>();
    
    private Gesture previousGesture = new Gesture();
    private bool pressed = false;
    

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in Root.transform.parent)
        {
            relativeTo.Add(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gestures.Add(Save());
        }
        Gesture currentGesture = Recognise();
        bool hasRecognised = !currentGesture.Equals(new Gesture());
        if(hasRecognised)
        {
            if(!currentGesture.Equals(previousGesture) || previousGesture.repeatable) {
                print(currentGesture.name);
                previousGesture = currentGesture;
                try
                {
                    currentGesture.onRecognise.Invoke();
                }
                catch { }
            }
        }
        if (!currentGesture.Equals(previousGesture))
        {
            try {
                previousGesture.onStop.Invoke();
            }
            catch
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              }
        }
    }

    Gesture Recognise()
    {
        Gesture currentgesture = new Gesture();
        float currentMin = Mathf.Infinity;
        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for(int i = 0; i < relativeTo.Count; i++)
            {
                Vector3 currentData = Root.transform.InverseTransformPoint(relativeTo[i].transform.position);
                //print($"{relativeTo[i].transform.name}: {currentData}");
                
                float distance = Vector3.Distance(currentData, gesture.bodyPositions[i]);
                
                if (distance> threshold)
                {
                    isDiscarded = true;
                }
                sumDistance += distance;
            }
            if(!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentgesture = gesture;
            }
        }
        return currentgesture;
    }

    public Gesture Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> posData = new List<Vector3>();
        //List<Vector3> rotData = new List<Vector3>();
        foreach (var bone in relativeTo)
        {
            posData.Add(Root.transform.InverseTransformPoint(bone.transform.position));
            //rotData.Add(Root.transform.InverseTransformDirection(bone.transform.forward));
        }
        g.bodyPositions = posData;
        //g.bodyRotations = rotData;
        //gestures.Add(g);
        return g;
    }
}
