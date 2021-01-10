using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public int savePointIndex = -1;

    public int rowNumber { get; set; }
    public int colNumber { get; set; }
    public List<Transform> savedSMs { get; set; }
    public List<Vector3> savedSMPositions { get; set; }
    public Dictionary<string, List<Transform>> savedDataPoints { get; set; }
    public Dictionary<string, List<Vector3>> savedDataPointPositions { get; set; }

    private void Awake()
    {
        rowNumber = 0;
        colNumber = 0;
        savedSMs = new List<Transform>();
        savedSMPositions = new List<Vector3>();
        savedDataPoints = new Dictionary<string, List<Transform>>();
        savedDataPointPositions = new Dictionary<string, List<Vector3>>();
    }

    public Marker(int row, int col, List<Transform> sm, List<Vector3> smPositions, Dictionary<string, 
        List<Transform>> dataPoints, Dictionary<string, List<Vector3>> dataPositions) {
        rowNumber = row;
        colNumber = col;
        savedSMs = sm;
        savedSMPositions = smPositions;
        savedDataPoints = dataPoints;
        savedDataPointPositions = dataPositions;
    }

}
