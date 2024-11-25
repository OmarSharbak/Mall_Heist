using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Needed to get the current scene name or index

public class DialogueManager : MonoBehaviour
{
	public static DialogueManager Instance { get; private set; }

	ThirdPersonController thirdPersonController=null;
	PlayerDamageHandler playerDamageHandler;
	[SerializeField] TMP_Text contentText;
	[SerializeField] GameObject dialogueUI; // Container for all dialogue-related UI elements
	[SerializeField] Button continueButton; // Button to close the dialogue
	[SerializeField] Animator animator; // Animator for the dialogue box transitions

	private Queue<string> sentences; // Queue to store sentences
	private Dialogue currentDialogue;
	private int selectedOptionIndex = -1;

	private AudioSource audioSource;

	public bool inDialogue = false;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		sentences = new Queue<string>();
		audioSource = GetComponent<AudioSource>();
	}

	private void HandleLocalPlayerStarted(ThirdPersonController localPlayer)
	{
		thirdPersonController= localPlayer;
		playerDamageHandler= thirdPersonController.GetComponent<PlayerDamageHandler>();
	}

	void Update()
	{
		if (thirdPersonController!=null &&  dialogueUI.activeInHierarchy &&
			EventSystem.current.currentSelectedGameObject != continueButton.gameObject &&
			EscalatorManager.Instance.currentState != EscalatorManager.GameState.Pause)
		{
			EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
		}
	}

	public void StartDialogue(Dialogue dialogue)
	{
		// Check if the current level is finished before starting the dialogue
		if (IsCurrentLevelFinished()) return;

		thirdPersonController.StopPlayer();
		playerDamageHandler.SetInvincible();
		EscalatorManager.Instance.SetExposed(false);
		EscalatorManager.Instance.ClearTargetAll();

		dialogueUI.SetActive(true); // Activate dialogue UI

		currentDialogue = dialogue;

		animator.SetBool("IsOpen", true);
		contentText.text = dialogue.content;

		EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
		EventSystem.current.SetSelectedGameObject(continueButton.gameObject); // Set new selection

		inDialogue = true;
	}

	public void EndDialogue()
	{
		thirdPersonController.canMove = true;
		playerDamageHandler.ResetInvincible();

		dialogueUI.SetActive(false); // Deactivate dialogue UI

		animator.SetBool("IsOpen", false);

		selectedOptionIndex = -1;
		inDialogue = false;
	}

	private bool IsCurrentLevelFinished()
	{
		// Replace this with the actual method to check if the current level is finished
		// This is a placeholder function
		string currentLevelName = SceneManager.GetActiveScene().name;
		LevelData currentLevelData = SaveLoadManager.Instance.GetCurrentLevelData(currentLevelName);
		return currentLevelData != null && currentLevelData.finished;
	}

	private void OnEnable()
	{
		// Subscribe to the event
		ThirdPersonController.OnLocalPlayerStarted += HandleLocalPlayerStarted;
	}

	private void OnDisable()
	{
		// Unsubscribe from the event to avoid memory leaks
		ThirdPersonController.OnLocalPlayerStarted -= HandleLocalPlayerStarted;
	}
}
