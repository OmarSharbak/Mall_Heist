using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class CrackableSafe : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private int _partsCounts = 100;
    [SerializeField] private float _tolerance = 5f; // Allowable error margin
    [SerializeField] public int[] _combination = { 30, 90, 180 }; // Safe combination in step values
    [SerializeField] private float unlockDelay = 1f; // Time to stay in range before unlocking
    [SerializeField] private AudioSource _dialAudio;
    [SerializeField] private AudioSource _doorOpenAudio;

    private ThirdPersonController _thirdPersonController;
    private CrackableSafePanel _crackableSafePanel;
    private bool _isHolding;
    private float _stepAngle;
    private int currentIndex = 0;
    private float currentRotation = 0f;
    private float stayInRangeTime = 0f;
    private bool isUnlocked;
    private bool isInteracting;
    private bool _isCracking;

    private InputAction _inputAction;
    private InputAction _interactionInput;
    private InputPromptUIManager _inputPromptUIManager;

    private void Start()
    {
        _stepAngle = 360f / _partsCounts; // Calculate step size

        var intP = FindObjectOfType<InputPromptUIManager>();

        if (intP != null)
        {
            _inputPromptUIManager = intP.GetComponent<InputPromptUIManager>();
        }
    }

    private void Update()
    {
        if (isUnlocked)
        {
            return;
        }

        if (_interactionInput != null && _interactionInput.WasPressedThisFrame() && isInteracting == false)
        {
            _crackableSafePanel.SetGameObjectActive(true);
            _inputPromptUIManager.HideSouthButtonUI();
            isInteracting = true;
        }

        CheckCombination();
        if (_isHolding == false)
        {
            return;
        }

        var value = _inputAction.ReadValue<float>();
        float stepMovement = Mathf.Round(value) * _stepAngle;
        float newRotation = currentRotation + stepMovement;

        currentRotation = newRotation % 360f;
        if (currentRotation < 0f) currentRotation += 360f;
        {
            //currentRotation = newRotation;
            _crackableSafePanel.Dial.rotation = Quaternion.Euler(0, 0, currentRotation);
        }

    }

    void CheckCombination()
    {
        if (_crackableSafePanel == null)
            return;

        int currentStep = Mathf.RoundToInt(currentRotation / _stepAngle) + 1;
        float diff = Mathf.Abs(currentStep - _combination[currentIndex]);

        Debug.Log("step " + currentStep);
        Debug.Log("diff " + diff);

        if (diff < _tolerance)
        {
            if (stayInRangeTime == 0)
            {
                _crackableSafePanel.StartShakeTween(currentIndex, unlockDelay);
            }

            stayInRangeTime += Time.deltaTime;
            if (stayInRangeTime < unlockDelay)
                return;

            Debug.Log("Correct Position: " + _combination[currentIndex]);
            _crackableSafePanel.StopShakeTween(currentIndex);
            _crackableSafePanel.Unlock(currentIndex);
            currentIndex++;
            stayInRangeTime = 0f;

            if (currentIndex >= _combination.Length)
            {
                UnlockSafe();
            }
        }
        else
        {
            _crackableSafePanel.StopShakeTween(currentIndex);
            stayInRangeTime = 0f;
        }
    }

    void UnlockSafe()
    {
        isUnlocked = true;
        _crackableSafePanel.SetGameObjectActive(false);
        _animator.SetTrigger("Open");
        _doorOpenAudio.Play();
        Debug.Log("Safe Unlocked!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked || _isCracking)
        {
            return;
        }

        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            _isCracking = true;
            _thirdPersonController = other.GetComponent<ThirdPersonController>();
            _crackableSafePanel = FindObjectOfType<CrackableSafePanel>(true);
            _crackableSafePanel.CreateLocks(_combination.Length);
            _inputPromptUIManager.ShowSouthButtonUI();
            var input = other.GetComponent<PlayerInput>();
            _inputAction = input.actions["SafeInteraction"];
            _interactionInput = input.actions["Interact"];
            _inputAction.performed += CrackableSafe_performed;
            _inputAction.canceled += CrackableSafe_canceled;
            _inputAction.Enable();
        }
    }

    private void CrackableSafe_performed(InputAction.CallbackContext context)
    {
        _isHolding = true;
        _dialAudio.Play();
    }
    private void CrackableSafe_canceled(InputAction.CallbackContext obj)
    {
        _isHolding = false;
        _dialAudio.Stop();
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (_thirdPersonController == controller)
            {
                _isCracking = false;
                _inputPromptUIManager.HideSouthButtonUI();
                _crackableSafePanel.SetGameObjectActive(false);
                _crackableSafePanel.Dial.eulerAngles = Vector3.zero;
                isInteracting = false;
                currentIndex = 0;
                currentRotation = 0;
                var input = other.GetComponent<PlayerInput>();
                _interactionInput = null;
                _inputAction = input.actions["SafeInteraction"];
                _inputAction.performed -= CrackableSafe_performed;
                _inputAction.canceled -= CrackableSafe_canceled;
                _inputAction.Disable();
            }
        }
    }
}
