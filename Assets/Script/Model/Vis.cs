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
    public bool showOnHead;
    public bool showOnWaist;
    public bool showOnGround;
    public bool showOnShelves;
    public bool showHighlighted;
    public bool showSelected;
    public bool testingOnGround;

    public string VisName { get; set; }
    public Vector3 GroundPosition { get; set; }
    public Vector3 HeadDashboardPosition { get; set; }
    public Vector3 GroundScale { get; set; }
    public Vector3 HeadDashboardScale { get; set; }
    public bool OnHead { get; set; }
    public bool OnWaist { get; set; }
    public bool OnGround { get; set; }
    public bool OnShelves { get; set; }
    public bool Highlighted { get; set; }
    public bool Selected { get; set; }

    private void Awake()
    {
        if (testingOnGround)
            OnGround = true;
    }

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
        OnHead = v.OnHead;
        OnWaist = v.OnWaist;
        OnGround = v.OnGround;
        OnShelves = v.OnShelves;
        Highlighted = v.Highlighted;
        Selected = v.Selected;

        showName = VisName;
        showGroundPosition = GroundPosition;
        showHeadDashboardPosition = HeadDashboardPosition;
        showGroundScale = GroundScale;
        showHeadDashboardScale = HeadDashboardScale;
        showOnHead = OnHead;
        showOnWaist = OnWaist;
        showOnGround = OnGround;
        showOnShelves = OnShelves;
        showHighlighted = Highlighted;
        showSelected = Selected;
    }

    public void Update()
    {
        showName = VisName;
        showGroundPosition = GroundPosition;
        showHeadDashboardPosition = HeadDashboardPosition;
        showGroundScale = GroundScale;
        showHeadDashboardScale = HeadDashboardScale;
        showOnHead = OnHead;
        showOnWaist = OnWaist;
        showOnGround = OnGround;
        showOnShelves = OnShelves;
        showHighlighted = Highlighted;
        showSelected = Selected;
    }
}
