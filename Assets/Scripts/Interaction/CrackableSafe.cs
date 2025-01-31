using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class CrackableSafe : NetworkBehaviour
{

    [SerializeField] private Transform _dial;
    [SerializeField] private int _partsCounts = 100;
    [SerializeField] private float _tolerance = 5f; // Allowable error margin
    [SerializeField] public int[] _combination = { 30, 90, 180 }; // Safe combination in step values


    private ThirdPersonController _thirdPersonController;
    private bool _isHolding;
    private float _stepAngle;
    private int currentIndex = 0;
    private float currentRotation = 0f;
    private bool isUnlocked = false;

    private InputAction _inputAction;

    private void Start()
    {
        _stepAngle = 360f / _partsCounts; // Calculate step size
    }

    private void Update()
    {
        if (_isHolding == false || isUnlocked)
        {
            return;
        }

        var value = _inputAction.ReadValue<float>();
        float stepMovement = Mathf.Round(value) * _stepAngle;
        float newRotation = currentRotation + stepMovement;

        // Ensure the dial stays within 0 - 360 degrees
        if (newRotation >= 0f && newRotation <= 360f)
        {
            currentRotation = newRotation;
            _dial.rotation = Quaternion.Euler(0, 0, currentRotation);
        }

        CheckCombination();
    }

    void CheckCombination()
    {
        float currentAngle = Mathf.Round(_dial.eulerAngles.z / _stepAngle) * _stepAngle;

        if (Mathf.Abs(currentAngle - _combination[currentIndex] * _stepAngle / _partsCounts) < _tolerance)
        {
            Debug.Log("Correct Position: " + _combination[currentIndex]);
            currentIndex++;

            if (currentIndex >= _combination.Length)
            {
                UnlockSafe();
            }
        }
    }

    void UnlockSafe()
    {
        isUnlocked = true;
        Debug.Log("Safe Unlocked!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            _thirdPersonController = other.GetComponent<ThirdPersonController>();
            var input = other.GetComponent<PlayerInput>();
            _inputAction = input.actions["SafeInteraction"];
            _inputAction.performed += CrackableSafe_performed;
            _inputAction.canceled += CrackableSafe_canceled;
            _inputAction.Enable();
        }
    }

    private void CrackableSafe_performed(InputAction.CallbackContext context)
    {
        _isHolding = true;
    }
    private void CrackableSafe_canceled(InputAction.CallbackContext obj)
    {
        _isHolding = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            _thirdPersonController = other.GetComponent<ThirdPersonController>();
            var input = other.GetComponent<PlayerInput>();
            _inputAction = input.actions["SafeInteraction"];
            _inputAction.performed -= CrackableSafe_performed;
            _inputAction.canceled -= CrackableSafe_canceled;
            _inputAction.Disable();
        }
    }
}
