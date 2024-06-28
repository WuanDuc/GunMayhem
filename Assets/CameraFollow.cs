using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player transform to follow
    public Vector3 offset; // Offset of the camera from the player
    public float smoothSpeed = 0.125f; // Smoothing speed

    public Vector2 minPosition; // Minimum position the camera can go
    public Vector2 maxPosition; // Maximum position the camera can go

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            desiredPosition.z = transform.position.z;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            //// Clamping the camera position
            //smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minPosition.x, maxPosition.x);
            //smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minPosition.y, maxPosition.y);

            transform.position = smoothedPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
