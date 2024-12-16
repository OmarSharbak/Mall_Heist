//DAD
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using Cinemachine;
using Mirror;
using System;








#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class ThirdPersonController : NetworkBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 2.0f;

	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 5.335f;

	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)]
	public float RotationSmoothTime = 0.12f;

	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;

	public AudioClip LandingAudioClip;
	public AudioClip[] FootstepAudioClips;
	[Range(0, 1)] public float FootstepAudioVolume = 0.5f;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;

	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -15.0f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.50f;

	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool Grounded = true;

	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;

	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.28f;

	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;

	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 70.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -30.0f;

	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float CameraAngleOverride = 0.0f;

	[Tooltip("For locking the camera position on all axis")]
	public bool LockCameraPosition = false;

	// cinemachine
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;

	// player
	public float _speed;
	private float _animationBlend;
	private float _targetRotation = 0.0f;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;

	// animation IDs
	private int _animIDSpeed;
	private int _animIDAnimation;
	private int _animIDDeath;
	private int _animIDJump;
	private int _animIDCrouch;
	private int _animIDWeaponType;
	private int _animIDGrounded;
	private int _animIDFullAuto;
	private int _animIDShoot;
	private int _animIDReload;
	private int _animIDStatic;
	private int _animIDHeadHorizontal;
	private int _animIDHeadVertical;
	private int _animIDBlend;
	private int _animIDDeathType;
	private int _animIDBodyVertical;
	private int _animIDBodyHorizontal;

	public bool canMove = true;
	public bool captured = false;
	private bool isThrowing = false; // A flag to ensure the throw action happens once per button press

#if ENABLE_INPUT_SYSTEM
	private PlayerInput _playerInput;
#endif
	public Animator animator;
	private CharacterController _controller;
	private InputSystem _input;
	private GameObject _mainCamera;
	private Inventory _inventory; // Player's inventory script
	PlayerDamageHandler _playerDamageHandler;
	private const float _threshold = 0.01f;
	private AudioManager audioManager;

	private bool _hasAnimator;

	// Declare a static event
	public static event Action<ThirdPersonController> OnLocalPlayerStarted;

	private bool IsCurrentDeviceMouse
	{
		get
		{
#if ENABLE_INPUT_SYSTEM
			return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
		}
	}
	public CinemachineVirtualCamera FollowTopCinemachineCamera;
	public CinemachineVirtualCamera FollowCinemachineCamera;
	public CinemachineVirtualCamera FullMapCinemachineCamera;

	public TMP_Text playerNameText;
	public GameObject floatingInfo;

	private SceneScript sceneScript;

	//private Material playerMaterialClone;

	public string playerName="";

	public Color playerColor = Color.white;

	public Renderer modelRenderer;

	//DAD

	private EscalatorManager escalatorManager;

	private bool isVisible = true;
	private const string VISIBLE_TAG = "Player"; // Assuming "Player" is the default tag when the player is visible.
	private const string INVISIBLE_TAG = "PlayerInvisible"; // Custom tag when the player is invisible. Ensure you've added this tag in Unity.


	public void ToggleVisibility()
	{
		var outlinable1 = GetComponent<Outlinable>();
		if (isVisible)
		{
			gameObject.tag = INVISIBLE_TAG;
			outlinable1.FrontParameters.Enabled = true;
			escalatorManager.ClearTargetAll(this);
			escalatorManager.SetExposed(this, false);
		}
		else
		{
			gameObject.tag = VISIBLE_TAG;
			outlinable1.FrontParameters.Enabled = false;
		}

		isVisible = !isVisible;
	}


	private void Awake()
	{
		// get a reference to our main camera
		if (_mainCamera == null)
		{
			Transform mainCameraTransform = GameObject.Find("MainCamera").GetComponent<Transform>();

			if (mainCameraTransform != null)
			{
				_mainCamera = mainCameraTransform.gameObject;
			}
			else
			{
				Debug.LogError("MainCamera not found among the player's parent's children");
			}
		}

		sceneScript = GameObject.FindObjectOfType<SceneScript>();

	}

	[Command]
	public void CmdSendPlayerMessage()
	{
		if (sceneScript)
			sceneScript.statusText = $"{playerName} says hello {UnityEngine.Random.Range(10, 99)}";
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		sceneScript.thirdPersonController = this;

		// Trigger the event
		OnLocalPlayerStarted?.Invoke(this);

	}

	public void SetupPlayer(string _name, Color _col)
	{
		playerNameText.text = _name;
		playerNameText.color = _col;
		sceneScript.statusText = $"{playerName} joined.";
	}

	public override void OnStartClient()
	{
		base.OnStartClient();


		_input = GetComponent<InputSystem>();

#if ENABLE_INPUT_SYSTEM
		_playerInput = GetComponent<PlayerInput>();
#else
        Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

		if (isLocalPlayer)
		{
			FullMapCinemachineCamera = GameObject.Find("PlayerFollowCamera(FullMap)").GetComponent<CinemachineVirtualCamera>();
			FollowTopCinemachineCamera = GameObject.Find("PlayerFollowCamera(Top)").GetComponent<CinemachineVirtualCamera>();
			FollowCinemachineCamera = GameObject.Find("PlayerFollowCamera(Regular)").GetComponent<CinemachineVirtualCamera>();
			pauseMenuGameObject = GameObject.Find("PauseUI");

			FullMapCinemachineCamera.Follow = CinemachineCameraTarget.transform;
			FollowTopCinemachineCamera.Follow = CinemachineCameraTarget.transform;
			FollowCinemachineCamera.Follow = CinemachineCameraTarget.transform;
			pauseMenuGameObject.SetActive(false);

		}
		else
		{
			_input.enabled = false;
			_playerInput.enabled = false;
		}
	}
	private void Start()
	{



		resumeButtonGameObject = GameObject.Find("ResumeButton");
		eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
		outliner = GameObject.Find("MainCamera").GetComponent<Outliner>();
		audioManager = GameObject.Find("Sounds").GetComponent<AudioManager>();
		escalatorManager = GameObject.Find("Escalator").GetComponent<EscalatorManager>();

		_cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

		_hasAnimator = TryGetComponent(out animator);
		_controller = GetComponent<CharacterController>();
		_inventory = GetComponent<Inventory>();
		_playerDamageHandler = GetComponent<PlayerDamageHandler>();
		promptUIManager = GameObject.Find("InteractionPrompts").GetComponent<InputPromptUIManager>();


		AssignAnimationIDs();

		// reset our timeouts on start
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;

	}

	public Vector3 inputDirection;
	private void Update()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		Pause();

		// Check if there is no currently selected GameObject
		if (EventSystem.current.currentSelectedGameObject == null && EscalatorManager.Instance.GetCurrentState(this) == EscalatorManager.GameState.Pause)
		{
			// If nothing is selected, select the resume button GameObject
			EventSystem.current.SetSelectedGameObject(resumeButtonGameObject);
		}

		if (captured)
		{
			canMove = false;
			_speed = 0.0f;
		}

		inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

		if (canMove)
		{
			Move();
			Throw();
			Melee();
			Place();
			Interact();
			InteractRegister();
			InteractEscalator();
			Switch();
			Seal();

		}

		if ((EscalatorManager.Instance.GetCurrentState(this) == EscalatorManager.GameState.Stealth || EscalatorManager.Instance.GetCurrentState(this) == EscalatorManager.GameState.Chase) && !captured)
		{
			if (DialogueManager.Instance == null)
			{
				SwitchCamera();
			}
			else
			{
				if (DialogueManager.Instance.inDialogue == false)
				{
					SwitchCamera();
				}
			}



			if (grassDecoration == null)
			{
				InteractGrass();
			}
			else if (fullMap == false)
			{
				InteractGrass();
			}


		}




		ResumeAfterCapture();

		if (grassDecoration != null && grassDecoration.hiding)
		{
			_input.throwItem = false;
		}

		//DAD
		//if (Input.GetKeyDown(KeyCode.I))
		//{
		//ToggleVisibility();
		//}
	}
	private void LateUpdate()
	{
		if (!isLocalPlayer) { return; }

		CameraRotation();
	}

	private void AssignAnimationIDs()
	{
		_animIDSpeed = Animator.StringToHash("Speed_f");
		_animIDAnimation = Animator.StringToHash("Animation_int");
		_animIDDeath = Animator.StringToHash("Death_b");
		_animIDJump = Animator.StringToHash("Jump_b");
		_animIDCrouch = Animator.StringToHash("Crouch_b");
		_animIDWeaponType = Animator.StringToHash("WeaponType_int");
		_animIDGrounded = Animator.StringToHash("Grounded");
		_animIDFullAuto = Animator.StringToHash("FullAuto_b");
		_animIDShoot = Animator.StringToHash("Shoot_b");
		_animIDReload = Animator.StringToHash("Reload_b");
		_animIDStatic = Animator.StringToHash("Static_b");
		_animIDHeadHorizontal = Animator.StringToHash("Head_Horizontal_f");
		_animIDHeadVertical = Animator.StringToHash("Head_Vertical_f");
		_animIDBlend = Animator.StringToHash("Blend(DontTouch)");
		_animIDDeathType = Animator.StringToHash("DeathType_int");
		_animIDBodyVertical = Animator.StringToHash("Body_Vertical_f");
		_animIDBodyHorizontal = Animator.StringToHash("Body_Horizontal_f");
	}

	private void CameraRotation()
	{
		// if there is an input and camera position is not fixed
		if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
		{
			//Don't multiply mouse input by Time.deltaTime;
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

			_cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
			_cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
		}

		// clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

		// Cinemachine will follow this target
		CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
			_cinemachineTargetYaw, 0.0f);
	}

	bool fullMap = false;
	private void SwitchCamera()
	{
		if (_input.camera && fullMap == false)
		{
			fullMap = true;
			StopMovement();
			FullMapCinemachineCamera.Priority = 12;
			_input.camera = false;
		}
		else if (_input.camera)
		{
			fullMap = false;
			FullMapCinemachineCamera.Priority = 5;
			EnableMovement();
			_input.camera = false;
		}

	}
	private void Move()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (_input.move == Vector2.zero) targetSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset ||
			currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
				Time.deltaTime * SpeedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
		if (_animationBlend < 0.01f) _animationBlend = 0f;

		// normalise input direction
		Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

		// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is a move input rotate player when the player is moving
		if (_input.move != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
							  _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
				RotationSmoothTime);

			// rotate to face input direction relative to camera position
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}


		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		// move the player
		_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
						 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

		// update animator if using character
		if (_hasAnimator)
		{
			animator.SetFloat(_animIDSpeed, _animationBlend);
			//animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
		}
	}


	private Bounds movementBounds;
	private void MoveWhileHiding()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (_input.move == Vector2.zero) targetSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset ||
			currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
				Time.deltaTime * SpeedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
		if (_animationBlend < 0.01f) _animationBlend = 0f;

		// normalise input direction
		Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;



		// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is a move input rotate player when the player is moving
		if (_input.move != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
							  _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
				RotationSmoothTime);

			// rotate to face input direction relative to camera position
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}


		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		// Calculate target position
		Vector3 targetPosition = transform.position + targetDirection.normalized * (_speed * Time.deltaTime);

		// If movement is restricted, clamp the target position within the bounds

		targetPosition = new Vector3(
			Mathf.Clamp(targetPosition.x, movementBounds.min.x, movementBounds.max.x),
			targetPosition.y, // Assuming Y-axis should not be restricted
			Mathf.Clamp(targetPosition.z, movementBounds.min.z, movementBounds.max.z)
		);

		// Move the player
		_controller.Move(targetPosition - transform.position +
						 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

		// update animator if using character
		if (_hasAnimator)
		{
			animator.SetFloat(_animIDSpeed, _animationBlend);
		}
	}
	private void ResumeAfterCapture()
	{
		if (_input.resume)
		{
			_input.resume = false;
			_playerDamageHandler.ResumePlay();
		}
	}


	// Used to set the captured state of the player.
	public void SetCapturedState(bool state)
	{
		captured = state;
		// Depending on the internal workings of your script, you might want to add 
		// additional logic here to handle the captured state.
	}

	// Enable player movement by setting the canMove flag to true.
	public void EnableMovement()
	{
		canMove = true;
		// If you have other initializations or actions to perform when movement is enabled, 
		// do it here.
	}

	public float interactionRange = 1.0f;  // The range within which the player can interact with objects.
	public LayerMask interactableLayer;  // The layer on which interactable objects reside.

	InputPromptUIManager promptUIManager;
	private void Interact()
	{
		if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionRange, interactableLayer))
		{
			// Check if the ray hit an interactable object.
			InventoryItem inventoryItem = hit.transform.GetComponent<InventoryItem>();
			InteractableObject interactableObject = hit.transform.GetComponent<InteractableObject>();

			if (inventoryItem != null)
			{
				if (_inventory.ItemExists(inventoryItem) == false)
				{
					if (interactableObject.pickedUpTimes >= 1)
						promptUIManager.ShowSouthButtonObjectsUI();
				}
			}
		}
		else
		{
			promptUIManager.HideSouthButtonObjectsUI();
		}

		if (_input.interact)
		{
			// Cast a ray forward from the player's position.
			if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit1, interactionRange, interactableLayer))
			{
				// Check if the ray hit an interactable object.
				InteractableObject interactableObject = hit1.transform.GetComponent<InteractableObject>();
				if (interactableObject != null)
				{
					// Interact with the object.
					interactableObject.Interact(GetComponent<PlayerInteractionController>());
					promptUIManager.HideSouthButtonObjectsUI();
				}
			}
		}
	}

	private Register register;

	public void SetNearbyRegister(Register newRegister)
	{
		if (newRegister != null)
		{
			register = newRegister;
			Debug.Log("Register is set nearby.");
		}
	}

	public void ClearNearbyRegister()
	{
		register = null;
	}
	private void InteractRegister()
	{
		if (register != null && register.interacted == false)
		{
			promptUIManager.ShowSouthButtonObjectsUI();
		}
		if (_input.interact && register != null)
		{
			register.Interact();
			promptUIManager.HideSouthButtonObjectsUI();
		}
	}

	private GrassDecorationStealth grassDecoration;

	public void SetNearbyGrass(GrassDecorationStealth newGrass)
	{
		if (newGrass != null)
		{
			grassDecoration = newGrass;
			movementBounds = newGrass.collider.bounds;
			Debug.Log("Grass is set nearby.");
		}
	}

	public void ClearNearbyGrass()
	{
		grassDecoration = null;
	}

	private void InteractGrass()
	{
		if (grassDecoration != null && grassDecoration.hiding == false)
		{
			promptUIManager.ShowSouthButtonObjectsUI();
		}
		if (_input.interact && grassDecoration != null)
		{
			grassDecoration.Interact();
		}
		_input.interact = false;
	}

	private void StopGrassHiding()
	{
		if (_input.interact && grassDecoration.hiding == true)
		{
			grassDecoration.StopHiding();
		}
	}
	private void InteractEscalator()
	{
		if (_input.interact)
		{
			escalatorManager.InteractEscalator();
		}
	}

	GameObject pauseMenuGameObject;
	GameObject resumeButtonGameObject;
	EventSystem eventSystem;
	Outliner outliner;

	EscalatorManager.GameState lastGameState;
	bool isPaused = false;
	private void Pause()
	{

		if (_input.pause && isPaused == false && escalatorManager.GetCurrentState(this) != EscalatorManager.GameState.Defeat && escalatorManager.GetCurrentState(this) != EscalatorManager.GameState.Victory)

		{
			_input.pause = false;
			CmdPause();

		}


	}

	[Command(requiresAuthority = false)]
	private void CmdPause()
	{

		foreach (var player in CustomNetworkManager.connectedPlayers)
		{
			ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
			if (controller != null)
			{
				controller.RpcPause();
				Debug.Log("SERVER - player paused" + controller.transform.name);
			}

		}
		Debug.Log("SERVER - paused");

	}

	[ClientRpc]
	private void RpcPause()
	{
		if (!isLocalPlayer)
			return;

		if (isPaused == false && escalatorManager.GetCurrentState(this) != EscalatorManager.GameState.Defeat && escalatorManager.GetCurrentState(this) != EscalatorManager.GameState.Victory)
		{
			outliner.enabled = false;
			pauseMenuGameObject.SetActive(true);

			lastGameState = EscalatorManager.Instance.GetCurrentState(this);
			EscalatorManager.Instance.SetCurrentState(this, EscalatorManager.GameState.Pause);

			// Access the EventSystem and set the selected GameObject
			EventSystem.current.SetSelectedGameObject(null); // Deselect current selection
			EventSystem.current.SetSelectedGameObject(resumeButtonGameObject); // Set new selection


			isPaused = true;

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Debug.Log("CLIENT - paused" + transform.name);
		}
		//Time.timeScale = 0.0f;
	}

	public void Resume()
	{

		CmdResume();
		Debug.Log("CLIENT - cmd resume" + transform.name);


	}

	[Command(requiresAuthority = false)]
	private void CmdResume()
	{

		foreach (var player in CustomNetworkManager.connectedPlayers)
		{
			ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
			if (controller != null)
			{
				controller.RpcResume();
				Debug.Log("SERVER - player resumed" + controller.transform.name);
			}

		}
		Debug.Log("SERVER - resumed");

	}

	[ClientRpc]
	private void RpcResume()
	{

		if (!isLocalPlayer)
			return;
		pauseMenuGameObject.SetActive(false);
		isPaused = false;
		outliner.enabled = true;
		EscalatorManager.Instance.SetCurrentState(this, lastGameState);

		Debug.Log("CLIENT - resumed" + transform.name);
		//Cursor.visible = false;
		//Cursor.lockState = CursorLockMode.Locked;
		//Time.timeScale = 1.0f;       
	}

	//Sealing Doors

	private SealableDoor nearbyDoor; // This will hold the reference to the nearby door

	private void Seal()
	{
		if (nearbyDoor != null)
		{
			if (_input.interactDoor)
			{
				nearbyDoor.CmdStartSealing();
			}
			else
			{
				nearbyDoor.CmdStopSealing();
			}
		}
	}

	private void Switch()
	{
		if (_input.switchUp)
		{
			_input.switchUp = false;
			_inventory.SwitchItem(1);
		}
		else if (_input.switchDown)
		{
			_input.switchDown = false;
			_inventory.SwitchItem(-1);
		}
	}

	bool resetting = false;
	public void ResetAttackAnimations()
	{
		Debug.Log("reseting");
		resetting = true;
		_input.throwItem = false;
		isMelee = false;
		isThrowing = false;
		animator.SetInteger(_animIDWeaponType, 0);
		SetCantDamage();
		StartCoroutine(AfterReset());
	}

	private IEnumerator AfterReset()
	{
		yield return new WaitForSeconds(.5f); // Wait for the specified delay
		SetCanDamage();
		resetting = false;
	}
	private void Throw()
	{
		if (_inventory.Throwable())
		{
			if (isThrowing == false)
			{
				if (_input.throwItem)
				{
					if (_hasAnimator && !resetting)
					{
						animator.SetInteger(_animIDWeaponType, 10);
						StartCoroutine(ResetWeaponType()); // Reset the weapontype after the animation plays
					}

					isThrowing = true; // Set the flag to true once the throw begins
					_input.throwItem = false;// Disabling throwItem in Starter Assets Inputs
					canMove = false; // Restricting player from moving
					_speed = 0.0f; // Stopping the player
					animator.SetFloat(_animIDSpeed, 0.0f); // Setting animator speed value to 0 for states changing

				}
			}
		}
		else
		{
			//_input.throwItem = false;
		}
	}

	public void StopPlayer()
	{
		canMove = false; // Restricting player from moving
		_speed = 0.0f; // Stopping the player
		animator.SetFloat(_animIDSpeed, 0.0f);
	}

	bool isMelee = false;
	Rigidbody rb = null;
	private void Melee()
	{
		if (_inventory.IsMelee())
		{

			if (isMelee == false)
			{
				if (_input.throwItem)
				{
					if (resetting)
						return;

					Transform itemToThrow = rightArm.GetChild(0);

					Rigidbody rb = itemToThrow.GetComponent<Rigidbody>();

					//canMove = false; // Restricting player from moving
					//_speed = 0.0f; // Stopping the player
					//animator.SetFloat(_animIDSpeed, 0.0f); // Setting animator speed value to 0 for states changing






					if (_hasAnimator)
					{
						animator.SetInteger(_animIDWeaponType, 11);
						StartCoroutine(ResetWeaponTypeMelee()); // Reset the weapontype after the animation plays
					}
					if (rb != null)
					{
						// Apply force to throw the item
						rb.isKinematic = false;
					}
					isMelee = true; // Set the flag to true once the throw begins
					_input.throwItem = false;// Disabling throwItem in Starter Assets Inputs
				}
			}
		}
		else
		{
			//_input.throwItem = false;
		}
	}

	//FOR MELEE WEAPON
	public void SetKinemtaic()
	{
		if (rb != null)
			rb.isKinematic = true;
		else if (_inventory.heldItem != null)
			_inventory.heldItem.GetComponent<Rigidbody>().isKinematic = true;
	}
	//FOR MELEE WEAPON
	public void SetCanDamage()
	{
		if (_inventory.heldItem != null)
		{
			if (_inventory.heldItem.GetComponent<MeleeWeapon>() != null)
				_inventory.heldItem.GetComponent<MeleeWeapon>().canDamage = true;
		}

	}
	//FOR MELEE WEAPON
	public void SetCantDamage()
	{
		if (_inventory.heldItem != null && _inventory.heldItem.GetComponent<MeleeWeapon>() != null)
			_inventory.heldItem.GetComponent<MeleeWeapon>().canDamage = false;
	}

	public void PlayThrowSound()
	{
		audioManager.PlayAudio("ItemThrow");
	}

	private void Place()
	{
		if (Grounded && _inventory.Placeable())
		{
			if (_input.throwItem)
			{
				_input.throwItem = false;
				Debug.Log("3 of place function");
				// Check if there's an item being held
				if (rightArm.childCount > 0)
				{
					Transform itemToThrow = rightArm.GetChild(0);
					if (_inventory.heldItem.GetComponent<TrapItem>().isPlaceable == false)
						return;

					_inventory.heldItem.GetComponent<TrapItem>().isPlaced = true;
					_inventory.heldItem.GetComponent<TrapItem>().CmdDeatach();


					// Detach the item from the player
					_inventory.DecreaseHeldItem();

					Rigidbody rb = itemToThrow.GetComponent<Rigidbody>();
					if (rb != null)
					{
						// Apply force to throw the item
						rb.isKinematic = false;
					}

					if (isServer)
					{
						// Disable collision between trap and placer
						foreach(Collider col in itemToThrow.GetComponents<Collider>())
							Physics.IgnoreCollision(col, GetComponent<Collider>(), true);


						// Call Rpc to sync this change to all clients
						RpcIgnoreCollision(itemToThrow.gameObject,gameObject);

					}

				}
			}
		}
		else
		{
			_input.throwItem = false;
		}
	}


	[ClientRpc]
	private void RpcIgnoreCollision(GameObject trapObject, GameObject placerObject)
	{
		if (placerObject.TryGetComponent(out Collider placerCollider))
		{
			foreach (Collider col in trapObject.GetComponents<Collider>())
				Physics.IgnoreCollision(col, GetComponent<Collider>(), true);

		}
	}

	public Transform rightArm;  // The transform where items to be thrown are held (typically, the right hand or similar)
	public Transform playerTransform; // The transform of the player for the direction of the projectile
	float throwForce = 35.0f;  // Adjust this value as necessary

	void OnThrowAnimationEnd()
	{
		// Check if there's an item being held
		if (rightArm.childCount > 0)
		{
			Transform itemToThrow = rightArm.GetChild(0); // Get the first child. This assumes only one item is held at a time.
			ThrowableItem throwableItemScript = itemToThrow.GetComponent<ThrowableItem>();

			if (throwableItemScript==null)
				return;
			if (!throwableItemScript.isThrowable)
				return;

			// Aim assist to adjust throw direction if a guard is within the aim assist angle
			Vector3 throwDirection = playerTransform.transform.forward;
			Vector3 aimAssistDirection = AimAssist(itemToThrow.position);

			if (aimAssistDirection != Vector3.zero)
			{
				throwDirection = new Vector3(aimAssistDirection.x, playerTransform.transform.forward.y, aimAssistDirection.z);
				throwForce = 40;
				Debug.Log("Player forward then aim assst: " + playerTransform.forward.y + " SPACE " + aimAssistDirection.y);
			}

			// Detach the item from the player
			_inventory.DecreaseHeldItem();

			Rigidbody rb = itemToThrow.GetComponent<Rigidbody>();
			if (rb != null)
			{
				// Apply force to throw the item in the (potentially adjusted) direction
				rb.isKinematic = false;
				Vector3 throwVector = throwDirection * throwForce;
				rb.AddForce(throwVector, ForceMode.Impulse);
				Debug.Log("Force is: " + throwVector);
				float length = Mathf.Sqrt(throwVector.x * throwVector.x + throwVector.y * throwVector.y + throwVector.z * throwVector.z);
				Debug.Log("The length of the vector is: " + length);
			}

			itemToThrow.SetParent(null);
		}
	}

	Vector3 AimAssist(Vector3 itemPosition)
	{
		float detectionRadius = 15f; // Or use a field to adjust the detection radius
		string[] layers = new string[] { "Guards", "PlayerVisible" };
		LayerMask hitLayer = LayerMask.GetMask(layers); // Adjust the layer mask as needed
		float aimAssistAngle = 35f; // Angle within which to assist aim

		Collider[] hits = Physics.OverlapSphere(itemPosition, detectionRadius, hitLayer);
		foreach (var hit in hits)
		{
			Debug.Log("FOUND IN RANGE");
		}
		Transform closestTarget = null;
		float closestAngle = Mathf.Infinity;

		foreach (var hit in hits)
		{
			if (hit.transform == transform)
				continue;//skip self
			Vector3 directionToTarget = (hit.transform.position - itemPosition).normalized;
			float angle = Vector3.Angle(playerTransform.forward, directionToTarget);

			if (angle < aimAssistAngle && angle < closestAngle)
			{
				closestAngle = angle;
				closestTarget = hit.transform;

				Debug.Log("Assisted");
			}
		}

		if (closestTarget != null)
		{
			return (closestTarget.position - itemPosition).normalized;
		}

		return Vector3.zero; // Return zero vector if no aim assist is applied
	}


	private IEnumerator ResetWeaponType()
	{
		yield return new WaitForSeconds(0.3f); // Wait for the throw animation duration (you might need to adjust this time based on your animation duration)
		animator.SetInteger(_animIDWeaponType, 0);
		isThrowing = false; // To let the player throw again
		isMelee = false;
		if (EscalatorManager.Instance.GetCurrentState(this) != EscalatorManager.GameState.Defeat && captured == false)
			canMove = true; // Enabling the player to move
	}

	private IEnumerator ResetWeaponTypeMelee()
	{
		yield return new WaitForSeconds(0.8f); // Wait for the throw animation duration (you might need to adjust this time based on your animation duration)
		animator.SetInteger(_animIDWeaponType, 0);
		isThrowing = false; // To let the player throw again
		isMelee = false;
		if (EscalatorManager.Instance.GetCurrentState(this) != EscalatorManager.GameState.Defeat && captured == false)
			canMove = true; // Enabling the player to move
	}

	// Called by the door when player enters its proximity
	public void SetNearbyDoor(SealableDoor door)
	{
		nearbyDoor = door;
	}

	// Called by the door when player exits its proximity
	public void ClearNearbyDoor(SealableDoor door)
	{
		if (nearbyDoor == door)
			nearbyDoor = null;
	}

	public void StopMovement()
	{
		canMove = false;
		animator.SetFloat(_animIDSpeed, 0);
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmosSelected()
	{
		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

		if (Grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;

		// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
		Gizmos.DrawSphere(
			new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
			GroundedRadius);
	}

	private void OnFootstep(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			if (FootstepAudioClips.Length > 0)
			{
				var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
				audioManager.PlayClipOnce(FootstepAudioClips[index], true);
			}
		}
	}
}