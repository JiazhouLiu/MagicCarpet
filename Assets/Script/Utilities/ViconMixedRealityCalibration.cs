using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViconMixedRealityCalibration : MonoBehaviour
{
    /* This is to automatically match the vicon and HPReverbG2 worlds. You will need the two 3d printed controllerframes to be tracking in vicon, and the left and right 
      controllers turned on and sitting inside them, rotated so that the internal posts are nudged up against the controller. Only one way they can sit.
      You will need to add left and right controllers as children of Cameraoffset in XRrig, and add tracked posedriver to them. Or if using another Vr sdk, there should be a camera parent, and two controller positions. 
      
      Place reasonable distance apart, but will probably both need to be in view of the headset.
      There is a 10 second delay to allow both controllers to be on and tracked by the headset.   
   
      */

    public Transform XRrig;
    public Transform leftController; // mixed reality controllers
    public Transform rightController;
    public Transform viconLeft; //tracked vicon frames
    public Transform viconRight;
    

    void Start()
    {
        StartCoroutine(Delay());
    }
   
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(10f);
        ApplyOffset();

    }
    void ApplyOffset()
    {
        //Get the angle of the tracked vicon controller frames
        Vector3 viconVector = viconLeft.position - viconRight.position;
        float viconAngle = Vector3.SignedAngle(viconVector, Vector3.forward, Vector3.up);  

        // Get the angle of the XR controllers
        Vector3 controllerVector = leftController.position - rightController.position;
        float controllerAngle = Vector3.SignedAngle(controllerVector, Vector3.forward, Vector3.up);

        // apply rotation offset to the XR camera and controller parent
        float RotOffset = controllerAngle - viconAngle;
        XRrig.eulerAngles = new Vector3(XRrig.eulerAngles.x, XRrig.eulerAngles.y + RotOffset, XRrig.eulerAngles.z);

        // apply position offset to xr parent. 
        Vector3 PosOffset = viconRight.position - rightController.position;
        XRrig.position += PosOffset;
        print("Calibrated! Rotated " + RotOffset + " degrees, moved " + PosOffset.ToString("F4"));
    }
}
