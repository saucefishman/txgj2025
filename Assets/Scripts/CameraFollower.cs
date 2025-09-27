using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // the object the camera will follow
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        // Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = desiredPosition;

        transform.LookAt(target);
    }
}