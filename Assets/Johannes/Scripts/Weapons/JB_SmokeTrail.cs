using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JB_SmokeTrail : MonoBehaviour
{
    LineRenderer lr;
    Color startColour;
    public Color endColour;
    public float fadeLength = 2;
    private float fadeTimer = 0;
    private float fadeProgress;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        startColour = lr.material.color;
        Destroy(this.gameObject, 2);
    }

    // Update is called once per frame
    void Update()
    {
        fadeTimer += Time.deltaTime;
        lr.material.color = Color.Lerp(startColour, endColour, fadeTimer / fadeLength);
    }
}
