using UnityEngine;
using System.Collections;
using EmeraldAI.Utility;

public class RotateOnTrigger : MonoBehaviour
{
    [SerializeField]
    private Vector3 originalRotationEulerAngles; // Set this in the Inspector

    private Quaternion originalRotation;
    private EmeraldAIDetection emeraldAIDetection;

    private void Start()
    {
        // Convert the Euler angles to a Quaternion and save it
        originalRotation = Quaternion.Euler(originalRotationEulerAngles);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Guard" tag
        if (other.CompareTag("Guard"))
        {
            Debug.Log("Rotating");
            emeraldAIDetection = other.GetComponent<EmeraldAIDetection>();
            // Start the coroutine to smoothly rotate the guard to the original rotation
            StartCoroutine(SmoothRotate(other.transform, originalRotation, 1.2f));
        }
    }

    IEnumerator SmoothRotate(Transform target, Quaternion toRotation, float duration)
    {
        yield return new WaitForSeconds(0.5f); // Wait for half a second

        Quaternion startRotation = target.rotation; // Store the starting rotation
        float time = 0;

        while (time < duration && emeraldAIDetection.EmeraldComponent.CurrentTarget == null)
        {
            target.rotation = Quaternion.Slerp(startRotation, toRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        target.rotation = toRotation; // Ensure the rotation is exactly the target rotation after the duration
    }
}
