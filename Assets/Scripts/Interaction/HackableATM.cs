using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.LowLevel;

public class HackableATM : NetworkBehaviour
{
    [SerializeField] private GameObject _cashPrefab;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private float shortKeyPressTime = 2;
    //[SerializeField] private float waitBeforeNewKey = 1.5f;
    public float progressSmoothTime = 0.2f;


    [SerializeField] private int _moneyAmount;
    [SerializeField] private int _cashCount;
    [SerializeField] private Transform _cashEndPoint;
    [SerializeField] private List<Key> _keySequence;
    [SerializeField] private List<GamepadButton> _padSequence;
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private AudioSource _hackingAudio;
    [SerializeField] private AudioSource _cashWidrawAudio;
    [SerializeField] private AudioSource _cashAddAudio;


    private int _totalSegments;
    private int _currentSegmant;
    private float _progressPerSegmant;
    private float progressVelocity;
    //private float _currenDuration;
    //private float timePerSegmant;
    private float keyPressTimer;

    private bool _isHacking;
    private bool _isHacked;
    private bool _isholding;
    private bool _isCanTakeInput;

    private bool _isSegmantCompleted;
    private Key _currentKey;
    private ThirdPersonController _thirdPersonController;
    private InputPromptUIManager _inputPromptUIManager;
    PlayerInput input;

    private void Start()
    {
        _isCanTakeInput = true;
        _totalSegments = _keySequence.Count;
        _progressPerSegmant = _slider.maxValue / _totalSegments;
        _currentSegmant = 0;
        _currentKey = _keySequence[_currentSegmant];

        var intP = FindObjectOfType<InputPromptUIManager>();

        if (intP != null)
        {
            _inputPromptUIManager = intP.GetComponent<InputPromptUIManager>();
        }
    }

    private void Update()
    {

        if (_isHacked)
        {
            return;
        }

        EvaluateProgress();

        if (Mathf.Approximately(_slider.value, _slider.maxValue))
        {
            _slider.value = _slider.maxValue;
            EvaluateProgress();
            HackATM();
        }

        if (!_isHacking)
        {
            return;
        }

        if (Keyboard.current.anyKey.wasPressedThisFrame && _isholding == false && _isCanTakeInput)
        {
            Key[] keys = (Key[])Enum.GetValues(typeof(Key));
            int startIndex = (int)Key.A;
            int lastIndex = (int)Key.Z;

            for (int i = startIndex; i <= lastIndex; i++)
            {
                var key = keys[i];

                if (!Keyboard.current[key].wasPressedThisFrame)
                {
                    continue;
                }

                if (key < Key.A || key > Key.Z)
                {
                    continue;
                }

                hasPressedCorrectKey = key == _currentKey;

                if (hasPressedCorrectKey)
                {
                    _isholding = true;
                    _hackingAudio.Play();
                    _currentSegmant++;
                    _isholding = false;

                    if (_currentSegmant >= _totalSegments)
                    {
                        _isCanTakeInput = false;
                        _inputPromptUIManager.HideInteractionImage();
                        return;
                    }

                    StartCoroutine(WaitAndAssignNewKey());
                }
                else
                {
                    _currentSegmant = Mathf.Clamp(_currentSegmant - 1, 0, _totalSegments);
                    StartCoroutine(WaitAndAssignNewKey());

                }
                keyPressTimer = 0;
            }
        }
        else if (_slider.value > 0 && _isCanTakeInput)
        {
            if (keyPressTimer >= shortKeyPressTime)
            {
                keyPressTimer = 0;
                _currentSegmant = Mathf.Clamp(_currentSegmant - 1, 0, _totalSegments);
                StartCoroutine(WaitAndAssignNewKey());
            }
            else
            {
                keyPressTimer += Time.deltaTime;
            }
        }

        if (Keyboard.current[_currentKey].wasReleasedThisFrame)
        {
            _isholding = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHacked || _isHacking)
        {
            return;
        }

        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            _thirdPersonController = other.GetComponent<ThirdPersonController>();

            _isHacking = true;
            _isholding = false;
            keyPressTimer = 0;
            _currentKey = _keySequence[_currentSegmant];
            Sprite sprite = _sprites[_currentSegmant];
            _slider.gameObject.SetActive(true);
            _inputPromptUIManager.SetInteractionImage(sprite);
            _inputPromptUIManager.ShowInteractionImage();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isHacked)
        {
            return;
        }

        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            var thirdPersonController = other.GetComponent<ThirdPersonController>();

            if (_thirdPersonController == thirdPersonController)
            {
                _currentSegmant = 0;
                _slider.value = 0;
                _thirdPersonController = null;
                _isHacking = false;
                _isholding = false;
                _hackingAudio.Stop();
                _slider.gameObject.SetActive(false);
                _inputPromptUIManager.HideInteractionImage();
                _isCanTakeInput = true;
                StopAllCoroutines();
            }
        }
    }

    private IEnumerator WaitAndAssignNewKey()
    {
        _isCanTakeInput = false;
        _inputPromptUIManager.HideInteractionImage();
        yield return new WaitForSeconds(progressSmoothTime + .2f);
        AssignNewKey();
    }

    private void AssignNewKey()
    {
        _currentKey = _keySequence[_currentSegmant];
        Sprite sprite = _sprites[_currentSegmant];
        _inputPromptUIManager.SetInteractionImage(sprite);
        _inputPromptUIManager.ShowInteractionImage();
        _isCanTakeInput = true;
    }

    bool hasPressedCorrectKey = false;

    private void EvaluateProgress()
    {
        _slider.value = Mathf.SmoothDamp(_slider.value, (_progressPerSegmant * _currentSegmant), ref progressVelocity, progressSmoothTime);
        _fillImage.color = _gradient.Evaluate(_slider.value);
    }

    private void HackATM()
    {
        if (_isHacked)
        {
            return;
        }

        _isHacked = true;
        _hackingAudio.Stop();
        _slider.gameObject.SetActive(false);
        StartCoroutine(WidrawCash());
    }

    IEnumerator WidrawCash()
    {
        if (_cashPrefab != null && _cashEndPoint != null)
        {
            _cashWidrawAudio.Play();
            for (int i = 0; i < _cashCount; i++)
            {
                var cash = Instantiate(_cashPrefab, _cashEndPoint, true);
                cash.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                yield return new WaitForSeconds(0.1f);
            }
            _cashWidrawAudio.Stop();
        }

        yield return new WaitForSeconds(0.1f);
        _cashEndPoint.gameObject.SetActive(false);
        if (_cashAddAudio != null)
            _cashAddAudio.Play();
        var damageHandler = _thirdPersonController.GetComponent<PlayerDamageHandler>();
        damageHandler.AddMoney(_moneyAmount);

    }
}
