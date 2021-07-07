using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViconMixedRealityCalibration : MonoBehaviour
{
    /* This is to automatically match the vicon and HPReverbG2 worlds. You will need the two 3d printed controllerframes to be tracking in vicon, and the left and right 
      controllers turned on and sitting inside them, rotated so that the internal posts are nudged up against the controller. Only one way they can sit.
      You will need to add left and right controllers as children of Cameraoffset in XRrig, and add tracked posedriver to them. 
      
      Place reasonable distance apart, but will probably both need to be in view of the headset.
      There is a 5/10 second delay to allow both controllers to be on and tracked by the headset. Will probably be unpleasant when the world shifts.  
      XR rig will be rotated on the y axis until the vicon controller frames are of equal distance from their matching frames to get rotation offset, then moved for position offset.
      Cannot test for equality with floats because of floating point errors, so the distance test might be dumb. Feel free to clean up the algorithm
      */

    public Transform XRrig;
    public Transform leftController;
    public Transform rightController;
    public Transform viconLeft;
    public Transform viconRight;
    float leftDistance;
    float rightDistance;
    bool calibrationInProgress;
    float ClosestRotationDifference = 100f;
    float rotationDistance;
    Vector3 positionOffset = new Vector3();

    void Start()
    {
        StartCoroutine(Delay());
    }
    void Update()
    {
        if (calibrationInProgress)
        {
            leftDistance = Vector3.Distance(viconLeft.position, leftController.position);
            rightDistance = Vector3.Distance(viconRight.position, rightController.position);
            rotationDistance = leftDistance - rightDistance;
            if (rotationDistance > 0.02f ) 
            {
                XRrig.eulerAngles = new Vector3(XRrig.eulerAngles.x, XRrig.eulerAngles.y + 0.005f, XRrig.eulerAngles.z);
                print("WAS Greater" + rotationDistance);//+ "Closest distance is " + ClosestRotationDifference);
                ClosestRotationDifference = rotationDistance;
                positionOffset = (viconRight.position - rightController.position);
                XRrig.position = XRrig.position + positionOffset;

            }
            else if (rotationDistance < -0.02f)
            {
                XRrig.eulerAngles = new Vector3(XRrig.eulerAngles.x, XRrig.eulerAngles.y - 0.005f, XRrig.eulerAngles.z);
                print("WAS LESS" + rotationDistance);//+ "Closest distance is " + ClosestRotationDifference);
                ClosestRotationDifference = rotationDistance;
                positionOffset = (viconRight.position - rightController.position);
                XRrig.position = XRrig.position + positionOffset;
            }
            else
            {
                calibrationInProgress = false;
                print("Rot Calibrated!");
                positionOffset = (viconRight.position - rightController.position);
                XRrig.position = XRrig.position + positionOffset;
            }
          //  print(rotationDistance);
        }
        //leftDistance = Vector3.Distance(viconLeft.position, leftController.position);
        //rightDistance = Vector3.Distance(viconRight.position, rightController.position);
        //rotationDistance = Mathf.Abs(leftDistance - rightDistance);
        // print(rotationDistance);
        // print("rightDistance is" + rightDistance.ToString("F4"));
        //leftDistance = Vector3.Distance(viconLeft.position, leftController.position);
        //rightDistance = Vector3.Distance(viconRight.position, rightController.position);
        //rotationDistance = leftDistance - rightDistance;
       // print(rotationDistance);
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(10f);
        calibrationInProgress = true;



    }
    
}
