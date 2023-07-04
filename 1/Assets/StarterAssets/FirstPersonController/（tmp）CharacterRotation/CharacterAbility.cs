using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;

public class CharacterAbility : MonoBehaviour
{
    /// the sound fx to play when the ability starts
    [Tooltip("the sound fx to play when the ability starts")]
    public AudioClip AbilityStartSfx;
    /// the sound fx to play while the ability is running
    [Tooltip("the sound fx to play while the ability is running")]
    public AudioClip AbilityInProgressSfx;
    /// the sound fx to play when the ability stops
    [Tooltip("the sound fx to play when the ability stops")]
    public AudioClip AbilityStopSfx;

    [Header("Permission")]
    /// if true, this ability can perform as usual, if not, it'll be ignored. You can use this to unlock abilities over time for example
    [Tooltip("if true, this ability can perform as usual, if not, it'll be ignored. You can use this to unlock abilities over time for example")]
    public bool AbilityPermitted = true;



    public virtual bool AbilityAuthorized
    {
        get
        {
            return AbilityPermitted;
        }
    }

    /// whether or not this ability has been initialized
    public bool AbilityInitialized { get { return _abilityInitialized; } }

    public delegate void AbilityEvent();
    public AbilityEvent OnAbilityStart;
    public AbilityEvent OnAbilityStop;

    protected FirstPersonController _character;
    protected FirstPersonController _controller;

    protected GameObject _model;
    //protected Health _health;
    //protected CharacterMovement _characterMovement;
    protected Animator _animator = null;
    protected SpriteRenderer _spriteRenderer;
    protected AudioSource _abilityInProgressSfx;
    protected bool _abilityInitialized = false;
    protected float _verticalInput;
    protected float _horizontalInput;
    protected bool _startFeedbackIsPlaying = false;
    protected List<PlayerWeaponsManagerBeta> _handleWeaponList;

    /// This method is only used to display a helpbox text at the beginning of the ability's inspector
    public virtual string HelpBoxText() { return ""; }

    /// <summary>
    /// On awake we proceed to pre initializing our ability
    /// </summary>
    protected virtual void Awake()
    {
        PreInitialization();
    }

    /// <summary>
    /// On Start(), we call the ability's intialization
    /// </summary>
    protected virtual void Start()
    {
        Initialization();
    }

    /// <summary>
    /// A method you can override to have an initialization before the actual initialization
    /// </summary>
    protected virtual void PreInitialization()
    {
        _character = this.gameObject.GetComponentInParent<FirstPersonController>();
    }

    /// <summary>
    /// Gets and stores components for further use
    /// </summary>
    protected virtual void Initialization()
    {
        _controller = this.gameObject.GetComponentInParent<FirstPersonController>();
        _spriteRenderer = this.gameObject.GetComponentInParent<SpriteRenderer>();
        _abilityInitialized = true;
    }

    /// <summary>
    /// Call this any time you want to force this ability to initialize (again)
    /// </summary>
    public virtual void ForceInitialization()
    {
        Initialization();
    }


    /// <summary>
    /// Adds required animator parameters to the animator parameters list if they exist
    /// </summary>
    protected virtual void InitializeAnimatorParameters()
    {

    }


    /// <summary>
    /// Called at the very start of the ability's cycle, and intended to be overridden, looks for input and calls methods if conditions are met
    /// </summary>
    protected virtual void HandleInput()
    {

    }

    /// <summary>
    /// Resets all input for this ability. Can be overridden for ability specific directives
    /// </summary>
    public virtual void ResetInput()
    {
        _horizontalInput = 0f;
        _verticalInput = 0f;
    }


    /// <summary>
    /// The second of the 3 passes you can have in your ability. Think of it as Update()
    /// </summary>
    public virtual void ProcessAbility()
    {

    }

    /// <summary>
    /// The last of the 3 passes you can have in your ability. Think of it as LateUpdate()
    /// </summary>
    public virtual void LateProcessAbility()
    {

    }

    /// <summary>
    /// Override this to send parameters to the character's animator. This is called once per cycle, by the Character class, after Early, normal and Late process().
    /// </summary>
    public virtual void UpdateAnimator()
    {

    }

    /// <summary>
    /// Changes the status of the ability's permission
    /// </summary>
    /// <param name="abilityPermitted">If set to <c>true</c> ability permitted.</param>
    public virtual void PermitAbility(bool abilityPermitted)
    {
        AbilityPermitted = abilityPermitted;
    }

    /// <summary>
    /// Override this to specify what should happen in this ability when the character flips
    /// </summary>
    public virtual void Flip()
    {

    }

    /// <summary>
    /// Override this to reset this ability's parameters. It'll be automatically called when the character gets killed, in anticipation for its respawn.
    /// </summary>
    public virtual void ResetAbility()
    {

    }


    /// <summary>
    /// Override this to describe what should happen to this ability when the character respawns
    /// </summary>
    protected virtual void OnRespawn()
    {
    }

    /// <summary>
    /// Override this to describe what should happen to this ability when the character respawns
    /// </summary>
    protected virtual void OnDeath()
    {

    }

    /// <summary>
    /// Override this to describe what should happen to this ability when the character takes a hit
    /// </summary>
    protected virtual void OnHit()
    {

    }

    /// <summary>
    /// On enable, we bind our respawn delegate
    /// </summary>
    protected virtual void OnEnable()
    {


        //if (_health == null)
        //{
        //    _health = this.gameObject.GetComponentInParent<Health>();
        //}

        //if (_health != null)
        //{
        //    _health.OnRevive += OnRespawn;
        //    _health.OnDeath += OnDeath;
        //    _health.OnHit += OnHit;
        //}
    }

    /// <summary>
    /// On disable, we unbind our respawn delegate
    /// </summary>
    protected virtual void OnDisable()
    {
        //if (_health != null)
        //{
        //    _health.OnRevive -= OnRespawn;
        //    _health.OnDeath -= OnDeath;
        //    _health.OnHit -= OnHit;
        //}
    }
}    
