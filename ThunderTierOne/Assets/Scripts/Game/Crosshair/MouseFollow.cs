using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        transform.position = Input.mousePosition;
    }
}
