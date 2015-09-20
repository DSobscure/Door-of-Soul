using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 10.0f;

    public float maxDistance = 15;
    public float minDistance = 5;

    public float zoomSpeed = 500f;
    public float rotateSpeed = 400f;

    private float rotateHorizontal;
    private float roateVertical;

    void Update()
    {
        if(target)
        {
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
            if(Input.GetMouseButton(1))
            {
                rotateHorizontal = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                roateVertical = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            }
            else
            {
                rotateHorizontal = 0;
                roateVertical = 0;
            }
            transform.position = target.position - transform.rotation * Vector3.forward * distance;
            transform.RotateAround(target.position, Vector3.up, rotateHorizontal);
            transform.RotateAround(target.position, Vector3.right, roateVertical);
            transform.LookAt(target);
        }
    }
}
