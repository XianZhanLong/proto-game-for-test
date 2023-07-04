using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : Singleton<StarterAssetsInputs>
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public bool jump;
		public bool sprint;
		public bool shoot;
		public bool switchWeapon;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		private PlayerInput playerInput;


#if ENABLE_INPUT_SYSTEM
        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        public void OnMove()
		{
			move = playerInput.actions["Move"].ReadValue<Vector2>();
		}

		public void OnJump()
		{
            jump = playerInput.actions["Jump"].ReadValue<bool>();
        }

		public void OnSprint()
		{
            sprint = playerInput.actions["Sprint"].ReadValue<bool>();
        }

        public void OnShoot()
		{
            shoot = playerInput.actions["Shoot"].ReadValue<float>() > 0;
        }

        public void OnSwitchWeapon()
        {
			switchWeapon = playerInput.actions["SwitchWeapon"].ReadValue<bool>();
        }
#endif

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}