using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Vis : MonoBehaviour
{
    public string showName;
    public Vector3 showGroundPosition;
    public Vector3 showHeadDashboardPosition;
    public Vector3 showGroundScale;
    public Vector3 showHeadDashboardScale;
    public bool showOnHeadDashBoard;
    public bool showOnWaistDashBoard;
    public bool showPinOnDashBoard;
    public bool showTriggeredByLeft;
    public bool showTriggeredByRight;

    public string VisName { get; set; }
    public Vector3 GroundPosition { get; set; }
    public Vector3 HeadDashboardPosition { get; set; }
    public Vector3 GroundScale { get; set; }
    public Vector3 HeadDashboardScale { get; set; }
    public bool OnHeadDashBoard { get; set; }
    public bool OnWaistDashBoard { get; set; }
    public bool PinOnDashBoard { get; set; }
    public bool TriggeredByLeft { get; set; }
    public bool TriggeredByRight { get; set; }

    public Vis() { }

    public Vis(string name, Vector3 position, Vector3 scale) {
        VisName = name;
        GroundPosition = position;
        GroundScale = scale;
    }

    public Vis(string name)
    {
        VisName = name;
    }

    public Vis(string name, Vector3 GPosition, Vector3 APosition, Vector3 GScale, Vector3 AScale)
    {
        VisName = name;
        GroundPosition = GPosition;
        HeadDashboardPosition = APosition;
        GroundScale = GScale;
        HeadDashboardScale = AScale;
    }

    public void CopyEntity(Vis v)
    {
        VisName = v.VisName;
        GroundPosition = v.GroundPosition;
        HeadDashboardPosition = v.HeadDashboardPosition;
        GroundScale = v.GroundScale;
        HeadDashboardScale = v.HeadDashboardScale;
        OnHeadDashBoard = v.OnHeadDashBoard;
        OnWaistDashBoard = v.OnWaistDashBoard;
        PinOnDashBoard = v.PinOnDashBoard;
        TriggeredByLeft = v.TriggeredByLeft;
        TriggeredByRight = v.TriggeredByRight;

        showName = VisName;
        showGroundPosition = GroundPosition;
        showHeadDashboardPosition = HeadDashboardPosition;
        showGroundScale = GroundScale;
        showHeadDashboardScale = HeadDashboardScale;
        showOnHeadDashBoard = OnHeadDashBoard;
        showOnWaistDashBoard = OnWaistDashBoard;
        showPinOnDashBoard = PinOnDashBoard;
        showTriggeredByLeft = TriggeredByLeft;
        showTriggeredByRight = TriggeredByRight;
    }

    public void Update()
    {
        showName = VisName;
        showGroundPosition = GroundPosition;
        showHeadDashboardPosition = HeadDashboardPosition;
        showGroundScale = GroundScale;
        showHeadDashboardScale = HeadDashboardScale;
        showOnHeadDashBoard = OnHeadDashBoard;
        showOnWaistDashBoard = OnWaistDashBoard;
        showPinOnDashBoard = PinOnDashBoard;
        showTriggeredByLeft = TriggeredByLeft;
        showTriggeredByRight = TriggeredByRight;
    }
}
