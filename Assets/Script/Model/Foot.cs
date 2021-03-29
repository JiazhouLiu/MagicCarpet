using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foot : MonoBehaviour
{
    [SerializeField] private Transform ToeTransform;
    [SerializeField] private Transform HeelTransform;
    public Transform CurrentTransform { get; set; }
    public Transform Toe { get; set; }
    public Transform Heel { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _ = new Foot(transform, ToeTransform, HeelTransform);
    }

    public Foot(Transform t, Transform ToeT, Transform HeelT) {
        CurrentTransform = t;
        Toe = ToeT;
        Heel = HeelT;
    }
}
