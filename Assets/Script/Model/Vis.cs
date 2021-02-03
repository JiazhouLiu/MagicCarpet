using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vis : MonoBehaviour
{
    public string showName;
    public Vector3 showGroundPosition;
    public Vector3 showInAirPosition;
    public Vector3 showGroundScale;
    public Vector3 showInAirScale;
    public bool showOnDashBoard;
    public bool showPinOnDashBoard;

    public string VisName { get; set; }
    public Vector3 GroundPosition { get; set; }
    public Vector3 InAirPosition { get; set; }
    public Vector3 GroundScale { get; set; }
    public Vector3 InAirScale { get; set; }
    public bool OnDashBoard { get; set; }
    public bool PinOnDashBoard { get; set; }

    public Vis() { }

    public Vis(string name, Vector3 position, Vector3 scale) {
        VisName = name;
        GroundPosition = position;
        GroundScale = scale;
    }

    public Vis(string name, Vector3 GPosition, Vector3 APosition, Vector3 GScale, Vector3 AScale)
    {
        VisName = name;
        GroundPosition = GPosition;
        InAirPosition = APosition;
        GroundScale = GScale;
        InAirScale = AScale;
    }

    public void CopyEntity(Vis v)
    {
        VisName = v.VisName;
        GroundPosition = v.GroundPosition;
        InAirPosition = v.InAirPosition;
        GroundScale = v.GroundScale;
        InAirScale = v.InAirScale;
        OnDashBoard = v.OnDashBoard;
        PinOnDashBoard = v.PinOnDashBoard;

        showName = VisName;
        showGroundPosition = GroundPosition;
        showInAirPosition = InAirPosition;
        showGroundScale = GroundScale;
        showInAirScale = InAirScale;
        showOnDashBoard = OnDashBoard;
        showPinOnDashBoard = PinOnDashBoard;
    }

    public void Update()
    {
        showName = VisName;
        showGroundPosition = GroundPosition;
        showInAirPosition = InAirPosition;
        showGroundScale = GroundScale;
        showInAirScale = InAirScale;
        showOnDashBoard = OnDashBoard;
        showPinOnDashBoard = PinOnDashBoard;
    }
}
