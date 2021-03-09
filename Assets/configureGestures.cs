using System.Collections;
using System.Collections.Generic;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

public class configureGestures : MonoBehaviour
{
    // Start is called before the first frame update

    private Gestures GestureTracker;
    private Interacter Interact;

    void Start()
    {
        GestureTracker = GetComponent<Gestures>();
        Interact = GetComponent<Interacter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1)) {
            CreateAction("Grab", Interact.Grip);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            CreateAction("Release", Interact.Release);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            CreateAction("Select", Interact.SelectVis);
        }
    }

    void CreateAction(string name, UnityAction action)
    {
        Gesture g = GestureTracker.Save();
        UnityEvent e = new UnityEvent();
        UnityEventTools.AddPersistentListener(e, action);
        g.onRecognise = e;
        g.name = name;
        GestureTracker.gestures.Add(g);
    }
}
