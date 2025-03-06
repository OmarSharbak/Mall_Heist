using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

    public class InputSystem : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool camera;
        public bool sprint;
        public bool crouch;
        public bool throwItem;
        public bool interact;
        public bool resume;
        public bool switchUp;
        public bool switchDown;
        public bool pause;

    [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

        public void OnCamera(InputValue value)
        {
            CameraInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnCrouch(InputValue value)
		{
			CrouchInput(value.isPressed);
		}

		public void OnThrow(InputValue value)
		{
			ThrowInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnResume(InputValue value)
		{
			ResumeInput(value.isPressed);
		}

        public void OnPause(InputValue value)
		{
			PauseInput(value.isPressed);
		}

		public void OnSwitchUp(InputValue value)
		{
			SwitchUpInput(value.isPressed);
		}

		public void OnSwitchDown(InputValue value)
		{
			SwitchDownInput(value.isPressed);
		}
#endif


    public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void CameraInput(bool newCameraState)
        {
            camera = newCameraState;
        }
    public void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void ThrowInput(bool newThrowState)
        {
            throwItem = newThrowState;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void ResumeInput(bool newResumeState)
        {
            resume = newResumeState;
        }

        public void PauseInput(bool newPauseState)
        {
            pause = newPauseState;
        }


        public void SwitchUpInput(bool newSwitchUpState)
        {
            switchUp = newSwitchUpState;
        }

        public void SwitchDownInput(bool newSwitchDownState)
        {
            switchDown = newSwitchDownState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
