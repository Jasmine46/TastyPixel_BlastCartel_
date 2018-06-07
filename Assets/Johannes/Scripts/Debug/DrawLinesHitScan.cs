using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLinesHitScan : MonoBehaviour
{
    public ControlPC pc;
    public PUN_ControlPC PUNpc;
    public Material lineMat;

    void OnPostRender()
    {
        if (PUNpc.firedPrimary /*pc.firedPrimary*/)
        {
            Vector3 originPointPos = PUNpc.transform.position + Vector3.up * .75f + PUNpc.transform.TransformDirection(Vector3.right) * .5f; /*pc.transform.position + Vector3.up * .75f + pc.transform.TransformDirection(Vector3.right) * .5f;*/
            Vector3 targetPos = transform.position + (transform.TransformDirection(Vector3.forward) * 200);

            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            GL.Color(new Color(lineMat.color.r, lineMat.color.g, lineMat.color.b, lineMat.color.a));
            GL.Vertex3(originPointPos.x, originPointPos.y, originPointPos.z);
            GL.Vertex3(targetPos.x, targetPos.y, targetPos.z);
            GL.End();
        }
    }
}
