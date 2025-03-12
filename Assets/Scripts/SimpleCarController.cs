using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    [Header("Car Movement Settings")]
    public float maxAccelerationForce = 1500f; // The peak force the car can apply for moving forward and backward
    public float steeringForce = 100f; // Steering force for turning left and right
    public float maxSpeed = 20f; // Maximum speed of the car

    [Header("Physics Settings")]
    public float drag = 3f; // Higher values mean more resistance when moving.
    public float angularDrag = 5f; // Higher values mean more resistance when turning.

    [Header("Steering Settings")]
    public float maxSteeringAngle = 30f; // Maximum angle the car can steer

    private Rigidbody _rigidbody;
    private InputSystem _input;
    private float currentAccelerationForce; // For smoothly increasing/decreasing acceleration

    public LevelTrigger levelTrigger;

    public bool canMove = true;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _input = GetComponent<InputSystem>();

        _rigidbody.drag = drag;
        _rigidbody.angularDrag = angularDrag;

        Application.targetFrameRate = 240; // Limiting the frame rate for consistency
    }

    private void Update()
    {
        InteractLevel();
    }
    private void FixedUpdate()
    {
        if (canMove)
        {
            MoveCar();
        }
    }

    void MoveCar()
    {
        Vector2 inputVector = _input.move;
        SmoothMoveCar(inputVector.y);
        SmoothSteerCar(inputVector.x);
        LimitCarSpeed();
    }
    private void SmoothMoveCar(float forwardInput)
    {
        // Invert the forwardInput to correct the direction if necessary
        forwardInput = -forwardInput; // Invert the sign of the input value

        // Smoothly interpolate the current acceleration force towards the target force based on input
        float targetAccelerationForce = maxAccelerationForce * forwardInput;
        currentAccelerationForce = Mathf.Lerp(currentAccelerationForce, targetAccelerationForce, Time.fixedDeltaTime * 3f); // Adjust the lerp speed as needed

        Vector3 acceleration = transform.forward * currentAccelerationForce;
        _rigidbody.AddForce(acceleration, ForceMode.Acceleration);
    }


    private void SmoothSteerCar(float steeringInput)
    {
        // Calculate the target steering angle and smoothly interpolate the current rotation towards it
        float targetSteeringAngle = maxSteeringAngle * steeringInput;
        Quaternion targetRotation = Quaternion.AngleAxis(targetSteeringAngle, Vector3.up);
        _rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, _rigidbody.rotation * targetRotation, Time.fixedDeltaTime * steeringForce));
    }

    private void LimitCarSpeed()
    {
        // Clamp the car's velocity to not exceed the maximum speed
        if (_rigidbody.velocity.magnitude > maxSpeed)
        {
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
        }
    }
    void InteractLevel()
    {
        if(levelTrigger != null)
        {
            if (_input.interact)
            {
                levelTrigger.InteractBuilding();
                _input.interact = false;
            }
        }
    }
}
