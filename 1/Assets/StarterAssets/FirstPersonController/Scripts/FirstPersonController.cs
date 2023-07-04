using System.Collections.Generic;
using System;
using Unity.FPS.Game;
using Unity.Mathematics;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;
       
		
		[Tooltip("the direction the character is going in")]
        public Vector3 CurrentDirection;

        // player
        private float _speed;


		private PlayerInput _playerInput;
		private CharacterController _CharacterController;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;


        //An attempt to operate on topdownEngine.character2
        protected CharacterAbility[] _characterAbilities;

        /// this character's orientation 3D ability
        public CharacterOrientation3D Orientation3D { get; protected set; }

        protected bool _abilitiesCachedOnce = false;

        [Header("Abilities")]
        /// A list of gameobjects (usually nested under the Character) under which to search for additional abilities
        [Tooltip("A list of gameobjects (usually nested under the Character) under which to search for additional abilities")]
        public List<GameObject> AdditionalAbilityNodes;

        //

        private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
            
			CurrentDirection = transform.forward;

            //A attempt to operate on topdownEngine.character3
            Initialization();
        }

        private void Start()
		{
			_CharacterController = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<PlayerInput>();
		}

		//public void GroundedCheck()
		//{
		//	// set sphere position, with offset
		//	Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		//	Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		//}

		public void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_CharacterController.velocity.x, 0.0f, _CharacterController.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = Vector3.right * _input.move.x + Vector3.forward * _input.move.y;
			}

			// move the player
            //是基于CharacterController实现的
			_CharacterController.Move(inputDirection.normalized * (_speed * Time.deltaTime));
		}

        //A attempt to operate on topdownEngine.character
        public void Update()
        {
            Move();
            //GroundedCheck();
            EveryFrame();

        }


        /// <summary>
        /// We do this every frame. This is separate from Update for more flexibility.
        /// </summary>
        protected virtual void EveryFrame()
        {
            // we process our abilities
            EarlyProcessAbilities();
            ProcessAbilities();
            LateProcessAbilities();

        }

        /// <summary>
        /// Calls all registered abilities' Early Process methods
        /// </summary>
        protected virtual void EarlyProcessAbilities()
        {
            //foreach (CharacterAbility ability in _characterAbilities)
            //{
            //    if (ability.enabled && ability.AbilityInitialized)
            //    {
            //        ability.EarlyProcessAbility();
            //    }
            //}
        }

        /// <summary>
        /// Calls all registered abilities' Process methods
        /// </summary>
        protected virtual void ProcessAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.ProcessAbility();
                }
            }
        }

        /// <summary>
        /// Calls all registered abilities' Late Process methods
        /// </summary>
        protected virtual void LateProcessAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.LateProcessAbility();
                }
            }
        }

        /// <summary>
        /// Gets and stores input manager, camera and components
        /// </summary>
        protected virtual void Initialization()
        {
            //if (this.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
            //{
            //    CharacterDimension = CharacterDimensions.Type2D;
            //}
            //if (this.gameObject.MMGetComponentNoAlloc<TopDownController3D>() != null)
            //{
            //    CharacterDimension = CharacterDimensions.Type3D;
            //}

            //// we initialize our state machines
            //MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject, SendStateChangeEvents);
            //ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject, SendStateChangeEvents);

            //// we get the current input manager
            //SetInputManager();
            // we store our components for further use 
            //CharacterState = new CharacterStates();
            //_controller = this.gameObject.GetComponent<TopDownController>();
            //if (CharacterHealth == null)
            //{
            //    CharacterHealth = this.gameObject.GetComponent<Health>();
            //}

            //CacheAbilitiesAtInit();
            //if (CharacterBrain == null)
            //{
            //    CharacterBrain = this.gameObject.GetComponent<AIBrain>();
            //}

            //if (CharacterBrain != null)
            //{
            //    CharacterBrain.Owner = this.gameObject;
            //}

            Orientation3D = FindAbility<CharacterOrientation3D>();


            //AssignAnimator();

            //// instantiate camera target
            //if (CameraTarget == null)
            //{
            //    CameraTarget = new GameObject();
            //}
            //CameraTarget.transform.SetParent(this.transform);
            //CameraTarget.transform.localPosition = Vector3.zero;
            //CameraTarget.name = "CameraTarget";

            //if (LinkedInputManager != null)
            //{
            //    if (OptimizeForMobile && LinkedInputManager.IsMobile)
            //    {
            //        if (this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>() != null)
            //        {
            //            this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Caches abilities if necessary
        /// </summary>
        protected virtual void CacheAbilitiesAtInit()
        {
            if (_abilitiesCachedOnce)
            {
                return;
            }
            CacheAbilities();
        }

        /// <summary>
        /// Grabs abilities and caches them for further use
        /// Make sure you call this if you add abilities at runtime
        /// Ideally you'll want to avoid adding components at runtime, it's costly,
        /// and it's best to activate/disable components instead.
        /// But if you need to, call this method.
        /// </summary>
        public virtual void CacheAbilities()
        {
            // we grab all abilities at our level
            _characterAbilities = this.gameObject.GetComponents<CharacterAbility>();//没有搞懂这步原理是什么，里面也没有CharacterAbility

            // if the user has specified more nodes
            if ((AdditionalAbilityNodes != null) && (AdditionalAbilityNodes.Count > 0))
            {
                // we create a temp list
                List<CharacterAbility> tempAbilityList = new List<CharacterAbility>();

                // we put all the abilities we've already found on the list
                for (int i = 0; i < _characterAbilities.Length; i++)
                {
                    tempAbilityList.Add(_characterAbilities[i]);
                }

                // we add the ones from the nodes
                for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
                {
                    CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
                    foreach (CharacterAbility ability in tempArray)
                    {
                        tempAbilityList.Add(ability);
                    }
                }

                _characterAbilities = tempAbilityList.ToArray();
            }
            _abilitiesCachedOnce = true;
        }

        /// <summary>
        /// Forces the (re)initialization of the character's abilities
        /// </summary>
        //public virtual void ForceAbilitiesInitialization()
        //{
        //    for (int i = 0; i < _characterAbilities.Length; i++)
        //    {
        //        _characterAbilities[i].ForceInitialization();
        //    }
        //    for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
        //    {
        //        CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
        //        foreach (CharacterAbility ability in tempArray)
        //        {
        //            ability.ForceInitialization();
        //        }
        //    }
        //}

        /// <summary>
        /// A method to check whether a Character has a certain ability or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindAbility<T>() where T : CharacterAbility
        {
            CacheAbilitiesAtInit();

            Type searchedAbilityType = typeof(T);

            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability is T characterAbility)
                {
                    return characterAbility;
                }
            }

            return null;
        }


    }
}