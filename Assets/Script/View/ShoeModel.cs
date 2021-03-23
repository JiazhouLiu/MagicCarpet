using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeModel : MonoBehaviour
{
    public ShoeModelMatCollection matCollection;
    public ShoePart shoePart;

    private void Update()
    {
        switch (shoePart) {
            case ShoePart.shoeInner:
                GetComponent<MeshRenderer>().material = matCollection.shoeInnerMat;
                break;
            case ShoePart.shoeLaces:
                GetComponent<MeshRenderer>().material = matCollection.shoeLacesMat;
                break;
            case ShoePart.shoeSurface:
                GetComponent<MeshRenderer>().material = matCollection.shoeSurfaceMat;
                break;
            default:
                break;
        }
    }
}
