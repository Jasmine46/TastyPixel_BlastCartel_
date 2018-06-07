using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// See the route of the object in play mode.
[ExecuteInEditMode]
public class OK_PayloadRail : MonoBehaviour
{

    public Transform[] Points;


    private void Start()
    {
        Points = GetComponentsInChildren<Transform>();
    }

    // Sets the objects path and the positions it will lerp between.
    public Vector3 LinearPosition(int Segment, float Ratio)
    {
        Vector3 Position1 = Points[Segment].position;
        Vector3 Position2 = Points[Segment + 1].position;

        return Vector3.Lerp(Position1, Position2, Ratio);

    }

    // Sets the objects orientation while on the line.
    public Quaternion Orientation(int Segment, float Ratio)
    {
        Quaternion q1 = Points[Segment].rotation;
        Quaternion q2 = Points[Segment + 1].rotation;

        return Quaternion.Lerp(q1, q2, Ratio);
    }

#if UNITY_EDITOR
    // Draw a line to each point.
    private void OnDrawGizmos()
    {
        for (int i = 0; i < Points.Length - 1; i++)
        {
            Handles.DrawDottedLine(Points[i].position, Points[i + 1].position, 3.0f);
        }
    }
#endif


}
