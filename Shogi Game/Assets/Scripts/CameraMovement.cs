using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private static readonly float speed = 0.01f;

    Vector3 targetPosition;

    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            targetPosition = new Vector3(
                transform.position.x,
                transform.position.y,
                Mathf.Clamp(hit.point.z - 5.5f, -6.5f, -3.5f)
            );
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed);
    }
}
