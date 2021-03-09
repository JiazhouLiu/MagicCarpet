using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interacter : MonoBehaviour
{
    [SerializeField]
    private VisController VC;

    [SerializeField]
    private Transform Hand;

    [SerializeField]
    private float speed;

    public float Distance;
    public Transform PostCards;

    private Transform grabbed;

    private Vector3 postCardSize = Vector3.one * 0.015f;


    private void Start()
    {
        
    }

    private void Update()
    {
        if (grabbed != null && (grabbed.position != Hand.position || grabbed.localScale != postCardSize))
        {
            grabbed.position = Vector3.Lerp(grabbed.position, Hand.position, Time.deltaTime * speed);
            grabbed.localScale = Vector3.Lerp(grabbed.localScale, postCardSize, Time.deltaTime * speed);
        }
        
    }

    public void Grip()
    {
        foreach (Transform t in VC.currentVisOnDashboard.Values)
        {
            GrabVis(t);
        }
        foreach (Transform t in VC.currentPinnedOnDashboard.Values)
        {
            GrabPin(t);
        }
        
    }

    private void GrabVis(Transform t)
    {
        print(Vector3.Distance(t.position, Hand.position));
        if (Vector3.Distance(t.position, Hand.position) <= Distance)
        {
            VC.currentVis.Remove(t);

            t.parent = Hand;
            VC.currentVisOnDashboard.Remove(t.name);
            t.GetComponent<Vis>().OnDashBoard = false;
            Transform groundOriginal = VC.GroundVisParent.Find(t.name);
            if (groundOriginal != null)
            {
                Destroy(groundOriginal.gameObject);
                
            }
            
            t.GetComponent<Vis>().InAirScale = Vector3.one * 0.1f;
        }
    }

    /*
     Vis shrinks into hand when
    + pin vis, grab vis again and repin
    + pin vis, place vis on floor, and repin
     */
    public void SelectVis()
    {
        RaycastHit hit;
        
        if(Physics.Raycast(Hand.position, Hand.forward, out hit))
        {
            Transform obj = hit.collider.gameObject.transform;
            obj.transform.position = Vector3.Lerp(obj.transform.position, Hand.position, Time.deltaTime * speed);
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, postCardSize, Time.deltaTime * speed);
            
            if(obj.parent.name == "DashBoard") {
                VC.currentVis.Remove(obj);
                VC.currentVisOnDashboard.Remove(obj.name);
                obj.GetComponent<Vis>().OnDashBoard = false;

                Transform groundOriginal = VC.GroundVisParent.Find(obj.name);
                if (groundOriginal != null)
                {
                    Destroy(groundOriginal.gameObject);

                }
            }
            else if(obj.parent.name == "PinnedDashBoard")
            {
                VC.currentPinnedOnDashboard.Remove(obj.name);
                obj.GetComponent<Vis>().PinOnDashBoard = false;
            }
            
            obj.parent = Hand;
            grabbed = obj;
        }
    }

    public void GrabPin(Transform t)
    {
        print(Vector3.Distance(t.position, Hand.position));
        if (Vector3.Distance(t.position, Hand.position) <= Distance)
        {
            VC.currentPinnedOnDashboard.Remove(t.name);
            t.GetComponent<Vis>().PinOnDashBoard = false;
            t.parent = Hand;
        }
    }

    public void Release()
    {
        if(grabbed != null) {
            Transform t = grabbed;
            if (Hand.position.y > PostCards.position.y)
            {
                GameObject visOnGround = Instantiate(t.gameObject, VC.GroundVisParent);
                visOnGround.transform.position = new Vector3(Hand.position.x, 0, Hand.position.z);
                visOnGround.GetComponent<Vis>().GroundPosition = visOnGround.transform.position;
                visOnGround.transform.localEulerAngles = new Vector3(90, 0, 0);
                visOnGround.transform.localScale = t.GetComponent<Vis>().GroundScale;
                visOnGround.GetComponent<Vis>().PinOnDashBoard = false;
                visOnGround.name = t.name;

                Destroy(t.gameObject);
            }
            else
            {
                print(Hand.position);
                t.SetParent(VC.WaistDashboard);
                VC.currentPinnedOnDashboard.Add(t.name, t);
                t.GetComponent<Vis>().PinOnDashBoard = true;
            }
            grabbed = null;
        }
    }

}
