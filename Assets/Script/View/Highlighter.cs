using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Highlighter : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GetComponent<HDAdditionalLightData>().SetAreaLightSize(new Vector2(transform.parent.localScale.x, transform.parent.localScale.y));
    }
}
