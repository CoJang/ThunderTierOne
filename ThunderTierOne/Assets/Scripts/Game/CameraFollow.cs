using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    public Vector3 cameraOffset = new Vector3(0, 5, -1.5f);

    private void Update()
    {
        transform.position = new Vector3(target.position.x + cameraOffset.x, 
                                        target.position.y  + cameraOffset.y, 
                                        target.position.z  + cameraOffset.z);
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }
}
