using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HackableATM : NetworkBehaviour
{
    [SerializeField] private GameObject _cashPrefab;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private float _maxHackingDuration = 5;
    [SerializeField] private float shortKeyPressTime = 2;

    [SerializeField] private int _moneyAmount;
    [SerializeField] private int _cashCount;
    [SerializeField] private Transform _cashEndPoint;
    [SerializeField] private List<Key> _keySequence;
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private AudioSource _hackingAudio;
    [SerializeField] private AudioSource _cashWidrawAudio;
    [SerializeField] private AudioSource _cashAddAudio;

    private int _totalSegments;
    private int _currentSegmant;
    private float _currenDuration;
    private float timePerSegmant;
    private float keyPressTimer;

    private bool _isHacking;
    private bool _isHacked;
    private bool _isholding;

    private bool _isSegmantCompleted;
    private Key _currentKey;
    private ThirdPersonController _thirdPersonController;
    private InputPromptUIManager _inputPromptUIManager;

    private void Start()
    {
        _totalSegments = _keySequence.Count;
        timePerSegmant = _maxHackingDuration / _totalSegments;
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

        if (_isHacking)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame && _isholding == false)
            {
                Key[] keys = (Key[])System.Enum.GetValues(typeof(Key));
                int startIndex = (int)Key.A;
                int lastIndex = (int)Key.Z;

                for (int i = startIndex; i <= lastIndex; i++)
                {
                    var key = keys[i];
                    if (Keyboard.current[key].wasPressedThisFrame)
                    {
                        if (key >= Key.A && key <= Key.Z)
                        {
                            hasPressedCorrectKey = key == _currentKey;
                            if (hasPressedCorrectKey)
                            {
                                _isholding = true;
                                _hackingAudio.Play();
                            }
                            _isSegmantCompleted = false;
                            keyPressTimer = 0;
                        }
                    }
                }
            }

            if (hasPressedCorrectKey && Keyboard.current[_currentKey].isPressed && _isSegmantCompleted == false)
            {
                _currenDuration += Time.deltaTime;
                if (_currenDuration >= ((_currentSegmant + 1) * timePerSegmant))
                {
                    _currentSegmant++;
                    _isSegmantCompleted = true;
                    _isholding = false;

                    if (_currentSegmant < _totalSegments)
                    {
                        _currentKey = _keySequence[_currentSegmant];
                        Sprite sprite = _sprites[_currentSegmant];
                        _inputPromptUIManager.SetInteractionImage(sprite);
                    }

                    if (_currentSegmant >= _totalSegments)
                    {
                        _hackingAudio.Stop();
                        _inputPromptUIManager.HideInteractionImage();
                        _slider.gameObject.SetActive(false);
                        HackATM();
                    }
                }
            }
            else if (hasPressedCorrectKey == false && _currenDuration > (_currentSegmant * timePerSegmant))
            {
                _currenDuration -= Time.deltaTime;

                if (_currenDuration <= (_currentSegmant * timePerSegmant))
                {
                    _currentSegmant = Mathf.Clamp(_currentSegmant - 1, 0, _totalSegments);
                }

            }
            else if (_currenDuration > 0)
            {
                if (keyPressTimer >= shortKeyPressTime && _currenDuration > (_currentSegmant * timePerSegmant))
                {
                    _currenDuration -= Time.deltaTime;
                    if (_currenDuration <= (_currentSegmant * timePerSegmant))
                    {
                        keyPressTimer = 0;
                        _currentSegmant = Mathf.Clamp(_currentSegmant - 1, 0, _totalSegments);
                    }
                }
                else
                {
                    keyPressTimer += Time.deltaTime;
                }
            }

            if (Keyboard.current[_currentKey].wasReleasedThisFrame)
            {
                _isholding = false;
                _hackingAudio.Stop();
            }

            EvaluateProgress();
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
                _thirdPersonController = null;
                _isHacking = false;
                _slider.gameObject.SetActive(false);
            }
        }
    }

    bool hasPressedCorrectKey = false;

    private void EvaluateProgress()
    {
        _currenDuration = Mathf.Clamp(_currenDuration, 0, _maxHackingDuration);
        _slider.value = Mathf.Clamp01(_currenDuration / _maxHackingDuration);
        _fillImage.color = _gradient.Evaluate(_slider.value);
    }

    private void HackATM()
    {
        if (_isHacked)
        {
            return;
        }

        _isHacked = true;
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
