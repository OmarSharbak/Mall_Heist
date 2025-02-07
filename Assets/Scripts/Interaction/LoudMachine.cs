using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EmeraldAI.SoundDetection;
using UnityEngine.InputSystem;
using EmeraldAI;

public class LoudMachine : NetworkBehaviour
{
    //public bool DRAW_Gizmos = true;
    [SerializeField] private AttractModifier _attractModifier;
    [SerializeField] private GameObject _noiseIndicatorPrefab;
    [SerializeField] private EPOOutline.Outlinable _outlinable;

    private MeshRenderer _noiseIndicatorRenderer;
    private GameObject _noiseIndicator; // Instance of the noise indicator
    private ThirdPersonController _thirdPersonController;
    private InputPromptUIManager _inputPromptUIManager;
    private bool _isMakingNoise;
    private bool _isDone;

    private Dictionary<GameObject, SoundDetector> _guards = new();

    private void Start()
    {
        var intP = FindObjectOfType<InputPromptUIManager>();
        if (intP != null)
        {
            _inputPromptUIManager = intP.GetComponent<InputPromptUIManager>();
        }
    }

    private void Update()
    {
        if (_isDone)
        {
            return;
        }

        //if (Keyboard.current.mKey.wasPressedThisFrame)
        //{
        //    MakeNoise();
        //}

        if (_isMakingNoise)
        {
            GetTargets();

            // Collider[] m_DetectedTargets = Physics.OverlapSphere(transform.position, _attractModifier.Radius, _attractModifier.EmeraldAILayer);

            //if (m_DetectedTargets.Length == 0)
            //    return;

            //for (int i = 0; i < m_DetectedTargets.Length; i++)
            //{
            //    Collider collider = m_DetectedTargets[i];

            //    if (_guards.ContainsKey(collider.gameObject))
            //    {
            //        continue;
            //    }

            //    var soundDetector = collider.GetComponent<SoundDetector>();
            //    soundDetector.EmeraldComponent.OnDetectTargetEvent.RemoveListener(OnDetectTarget);
            //    soundDetector.EmeraldComponent.OnDetectTargetEvent.AddListener(OnDetectTarget);
            //    soundDetector.EmeraldComponent.ReachedDestinationEvent.RemoveListener(OnGuardReached);
            //    soundDetector.EmeraldComponent.ReachedDestinationEvent.AddListener(OnGuardReached);
            //    if (soundDetector.DetectedAttractModifier == gameObject)
            //    {
            //        _guards.Add(collider.gameObject, soundDetector);
            //    }
            //}
        }
    }

    void GetTargets(bool HasTriggerLayer = true)
    {
        _attractModifier.PlayTriggerSound();

        Collider[] m_DetectedTargets = Physics.OverlapSphere(transform.position, _attractModifier.Radius, _attractModifier.EmeraldAILayer);

        if (m_DetectedTargets.Length == 0)
            return;

        for (int i = 0; i < m_DetectedTargets.Length; i++)
        {
            if (m_DetectedTargets[i].GetComponent<SoundDetector>() != null)
            {
                Collider collider = m_DetectedTargets[i];

                if (_guards.ContainsKey(collider.gameObject))
                {
                    continue;
                }

                SoundDetector SoundDetectionComponent = m_DetectedTargets[i].GetComponent<SoundDetector>(); //Cache each EmeraldAISoundDetection

                if (_attractModifier.EnemyRelationsOnly && m_DetectedTargets[i].GetComponent<EmeraldAIEventsManager>().GetPlayerRelation() != EmeraldAISystem.RelationType.Enemy) continue;

                SoundDetectionComponent.EmeraldComponent.OnDetectTargetEvent.RemoveListener(OnDetectTarget);
                SoundDetectionComponent.EmeraldComponent.OnDetectTargetEvent.AddListener(OnDetectTarget);
                SoundDetectionComponent.EmeraldComponent.ReachedDestinationEvent.RemoveListener(OnGuardReached);
                SoundDetectionComponent.EmeraldComponent.ReachedDestinationEvent.AddListener(OnGuardReached);

                _guards.Add(collider.gameObject, SoundDetectionComponent);

                if (_attractModifier.AttractReaction != null)
                {
                    SoundDetectionComponent.DetectedAttractModifier = gameObject; //Assign the detected Emerald AI agent as the DetectedAttractModifier
                    SoundDetectionComponent.InvokeReactionList(_attractModifier.AttractReaction, true); //Invoke the ReactionList.
                }
                else
                {
                    Debug.Log("There's no Reaction Object on the " + gameObject.name + "'s AttractReaction slot. Please add one in order for Attract Modifier to work correctly.");
                }
            }
        }
    }

    private void OnDetectTarget()
    {
        Debug.Log("OnDetectTarget");
        SetStatusDone();
    }

    public void OnGuardReached()
    {
        Debug.Log("OnGuardReached");
        SetStatusDone();
    }

    private void SetStatusDone()
    {
        _isDone = true;
        _isMakingNoise = false;

        foreach (var item in _guards)
        {
            item.Value.EmeraldComponent.OnDetectTargetEvent.RemoveListener(OnDetectTarget);
            item.Value.EmeraldComponent.ReachedDestinationEvent.RemoveListener(OnGuardReached);
            _attractModifier.AudioSource.Stop();
        }

        _guards.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDone)
        {
            return;
        }

        if (_isMakingNoise)
        {
            if (_guards.ContainsKey(other.gameObject))
            {
                SetStatusDone();
            }
            return;
        }

        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            _guards.Clear();
            _thirdPersonController = other.GetComponent<ThirdPersonController>();

            var input = other.GetComponent<PlayerInput>();
            var interaction = input.actions["Interact"];
            interaction.performed += Interaction_performed;
            _inputPromptUIManager.ShowInteractionImage();
        }
    }

    private void Interaction_performed(InputAction.CallbackContext context)
    {
        _inputPromptUIManager.HideInteractionImage();
        MakeNoise();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isDone)
        {
            return;
        }

        if ((other.CompareTag("Player") || other.CompareTag("PlayerInvisible")))
        {
            var thirdPersonController = other.GetComponent<ThirdPersonController>();
            if (_thirdPersonController == thirdPersonController)
            {
                var input = other.GetComponent<PlayerInput>();
                var interaction = input.actions["Interact"];
                interaction.performed -= Interaction_performed;
                _inputPromptUIManager.HideInteractionImage();
                _thirdPersonController = null;
            }
        }
    }

    private void MakeNoise()
    {
        if (_noiseIndicatorPrefab != null)
        {
            float scale = 2.162f;

            if (_noiseIndicator != null)
            {
                Destroy(_noiseIndicator);
            }

            Vector3 position = transform.position;
            position.y += .5f;
            _noiseIndicator = Instantiate(_noiseIndicatorPrefab, position, Quaternion.identity);
            _noiseIndicator.transform.localScale = new Vector3(_attractModifier.Radius * scale, _attractModifier.Radius * scale, _attractModifier.Radius * scale);
            _noiseIndicator.transform.rotation = Quaternion.Euler(90, 0, 0);
            _noiseIndicatorRenderer = _noiseIndicator.GetComponent<MeshRenderer>();
            _noiseIndicator.SetActive(true);
        }

        _isMakingNoise = true;
        StartCoroutine(BlinkNoiseIndicator());
    }

    IEnumerator BlinkNoiseIndicator()
    {
        while (_isMakingNoise)
        {
            float noiseLevel = Mathf.PingPong(Time.time, 0.3f);
            Color originalColor = _noiseIndicatorRenderer.material.color;

            _noiseIndicatorRenderer.material.SetColor("_BaseColor", new Color(originalColor.r, originalColor.g, originalColor.b, noiseLevel));
            yield return null;
        }

        Destroy(_noiseIndicator);
    }

    //private void OnDrawGizmos()
    //{
    //    if (_attractModifier != null && DRAW_Gizmos)
    //        Gizmos.DrawWireSphere(transform.position, _attractModifier.Radius);
    //}
}
