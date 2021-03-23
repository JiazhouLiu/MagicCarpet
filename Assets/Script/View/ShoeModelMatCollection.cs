using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShoePart // your custom enumeration
{
    shoeSurface,
    shoeInner,
    shoeLaces
};

public class ShoeModelMatCollection : MonoBehaviour
{
    public Material shoeSurfaceMat;
    public Material shoeInnerMat;
    public Material shoeLacesMat;
}
