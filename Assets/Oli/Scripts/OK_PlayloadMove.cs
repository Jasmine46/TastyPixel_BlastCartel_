using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OK_PlayloadMove : MonoBehaviour
{
    public OK_PayloadRail Rail;

    private int in_currentSegment;
    private float fl_transition;
    private bool bl_isComplete;

    public float Speed;
    public bool isReversed;
    public bool isLooping;

    private void Update()
    {
        if (!Rail)
            return;

        if (!bl_isComplete)
            Play(!isReversed);

    }

    private void Play(bool forward = true)
    {
        float Magnitude = (Rail.Points[in_currentSegment + 1].position - Rail.Points[in_currentSegment].position).magnitude;
        float s = (Time.deltaTime * 1 / Magnitude) * Speed;


        fl_transition += (forward)? s : -s ;

        if (fl_transition > 1)
        {
            fl_transition = 0;
            in_currentSegment++;
            if (in_currentSegment == Rail.Points.Length - 1)
            {
                if (isLooping)
                {
                    in_currentSegment = 0;
                }
                else
                {
                    bl_isComplete = true;
                    return;
                }
            }
        }
        else if (fl_transition < 0)
        {
            fl_transition = 1;
            in_currentSegment--;

        }

        transform.position = Rail.LinearPosition(in_currentSegment, fl_transition);
        transform.rotation = Rail.Orientation(in_currentSegment, fl_transition);


    }

}
