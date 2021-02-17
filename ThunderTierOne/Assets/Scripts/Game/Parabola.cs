using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class Parabola : MonoBehaviour
{
    public LayerMask layerMask = -1;


    public float initialVeloctity = 10.0f;

    float timeResolution = 0.02f;
    public float maxTime = 10.0f;

    private LineRenderer lineRender;
    // Use this for initialization
    void Start()
    {
        lineRender = GetComponent<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 velocityVector = transform.forward * initialVeloctity;


        lineRender.SetVertexCount((int)(maxTime / timeResolution));


        int index = 0;
        Vector3 currentPosition = transform.position;

        for (float t = 0.0f; t < maxTime; t += timeResolution)
        {

            lineRender.SetPosition(index, currentPosition);

            currentPosition += velocityVector * timeResolution;
            velocityVector += Physics.gravity * timeResolution;

            index++;

        }
    }
}
