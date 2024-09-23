using UnityEngine;

public class MatchXZPosition : MonoBehaviour
{
    public Transform targetTransform;
    public float smoothSpeed = 0.125f; // Adjust this value to make the camera movement smoother

    [Header("Boundary Settings")]
    public Vector2 xBoundaries; // Left and Right boundaries on the X axis
    public Vector2 zBoundaries; // Up and Down boundaries on the Z axis

    void LateUpdate()
    {
        if (targetTransform != null)
        {
            Vector3 currentPosition = transform.position;

            // Clamp target position between the specified X and Z boundaries
            float clampedX = Mathf.Clamp(targetTransform.position.x, xBoundaries.x, xBoundaries.y);
            float clampedZ = Mathf.Clamp(targetTransform.position.z, zBoundaries.x, zBoundaries.y);

            // Set target position with clamped X and Z, and current Y (or modify as needed)
            Vector3 targetPosition = new Vector3(clampedX, currentPosition.y, clampedZ);

            // Use Lerp for smooth transition between positions
            transform.position = Vector3.Lerp(currentPosition, targetPosition, smoothSpeed);
        }
    }
}
