using StarterAssets;
using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;



public class PlayerWeaponsManagerBeta: CharacterAbility
{
    [Header("Weapon Bob")]
    [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
    public float BobFrequency = 1f;

    [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
    public float BobSharpness = 10f;

    [Tooltip("Distance the weapon bobs when not aiming")]
    public float DefaultBobAmount = 0.05f;

    [Header("Weapon Recoil")]
    [Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
    public float RecoilSharpness = 50f;

    [Tooltip("Maximum distance the recoil can affect the weapon")]
    public float MaxRecoilDistance = 0.5f;

    [Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
    public float RecoilRestitutionSharpness = 10f;

    #region ÎäÆ÷ÇÐ»»Ä£¿é
    //ÎäÆ÷ÇÐ»»Ä£¿é
    public enum WeaponSwitchState
    {
        Up,
        Down,
        PutDownPrevious,
        PutUpNew,
    }
    
    [Tooltip("Delay before switching weapon a second time, to avoid recieving multiple inputs from mouse wheel")]
    public float WeaponSwitchDelay = 1f;
   
    public UnityAction<WeaponController> OnSwitchedToWeapon;

    [Header("Position Reference")]
    [Tooltip("Position for innactive weapons")]
    public Transform DownWeaponPosition;
    
    [Tooltip("Position for weapons when active but not actively aiming")]
    public Transform DefaultWeaponPosition;
    
    [Tooltip("Parent transform where all weapon will be added in the hierarchy")]
    public Transform WeaponParentSocket;

    [Header("Weapon Prefabs")]
    [Tooltip("List of weapon the player will start with")]
    public List<WeaponController> StartingWeapons = new List<WeaponController>();

    private float m_TimeStartedWeaponSwitch;
    
    private WeaponSwitchState m_WeaponSwitchState;
    
    private int m_WeaponSwitchNewWeaponIndex;

    private WeaponController[] m_WeaponSlots = new WeaponController[9]; // 9 available weapon slots

    public int ActiveWeaponIndex { get; private set; }

    [Tooltip("Layer to set FPS weapon gameObjects to")]
    public LayerMask FpsWeaponLayer;
    
    private Vector3 m_WeaponMainLocalPosition;
    //
    #endregion

    #region  (tmp)CharacterRotation
    //(tmp)CharacterRotation


    public WeaponAim WeaponAimComponent { get { return _weaponAim; } }

    protected WeaponAim _weaponAim;
    //
    #endregion
    public bool IsPointingAtEnemy { get; private set; }

    float m_WeaponBobFactor;
    Vector3 m_LastCharacterPosition;
    Vector3 m_WeaponBobLocalPosition;
    Vector3 m_WeaponRecoilLocalPosition;
    Vector3 m_AccumulatedRecoil;

    private WeaponController activeWeapon;

    StarterAssetsInputs _input;
    CharacterController _CharacterController;

    protected  override void Awake()
    {
        //ÎäÆ÷ÇÐ»»Ä£¿é14
        ActiveWeaponIndex = -1;
        m_WeaponSwitchState = WeaponSwitchState.Down;
        // shoot handling
        activeWeapon = GetActiveWeapon();

        //m_WeaponSwitchState = WeaponSwitchState.Down;

        _input = this.gameObject.GetComponentInParent<StarterAssetsInputs>();//ÕâÐ©·Åµ½AbillitiesÀïÃæ¾ÍÕÒ²»µ½ÁË

        _CharacterController = this.gameObject.GetComponentInParent<CharacterController>();

        //ÎäÆ÷ÇÐ»»Ä£¿é13
        OnSwitchedToWeapon += OnWeaponSwitched;

        //ÎäÆ÷ÇÐ»»Ä£¿é1
        // Add starting weapons
        foreach (var weapon in StartingWeapons)
        {
            AddWeaponUI(weapon);
        }

        SwitchWeapon(true);


    }

    void Update()
    {
        //ÎäÆ÷ÇÐ»»Ä£¿é15
        // shoot handling
        WeaponController activeWeapon = GetActiveWeapon();
        //// shoot handling
        //WeaponController activeWeapon = GetActiveWeapon();

        //if (activeWeapon != null && activeWeapon.IsReloading)
        //    return;
        //Check if the active weapon is reloading.If it is, the method returns early, skipping the rest of the update logic for shooting and weapon switching.

        if (activeWeapon != null )
        {
            //if (!activeWeapon.AutomaticReload && activeWeapon.CurrentAmmoRatio < 1.0f)
            //{
            //    activeWeapon.InputReload = true;

            //    activeWeapon.StartReloadAnimation();
            //    //ADD SOMETHING
            //    return;
            //}
            //This reload action will prevent shooting until completed.

            // handle aiming down sights

            // handle shooting
            bool hasFired = activeWeapon.HandleShootInputs(_input.shoot,_input.shoot,false);

            // Handle accumulating recoil
            //if (hasFired)
            //{
            //    m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
            //    m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
            //}
        }

        //ÎäÆ÷ÇÐ»»Ä£¿é10
        // weapon switch handling
        if ((m_WeaponSwitchState == WeaponSwitchState.Up || m_WeaponSwitchState == WeaponSwitchState.Down))
        {
            bool switchWeaponInput = _input.switchWeapon;
            if (switchWeaponInput)
            {
                SwitchWeapon(true);
                _input.switchWeapon = false;
            }

        }



        // Pointing at enemy handling
        IsPointingAtEnemy = false;
        if (activeWeapon)
        {
            if (Physics.Raycast(activeWeapon.WeaponMuzzle.position,activeWeapon.WeaponMuzzle.forward, out RaycastHit hit,
                1000, -1, QueryTriggerInteraction.Ignore))
            {

                if (hit.collider.GetComponentInParent<Health>() != null)
                {
                    IsPointingAtEnemy = true;
                }
            }
        }
    }
    // Update various animated features in LateUpdate because it needs to override the animated arm position
    void LateUpdate()
    {
        //ÎäÆ÷ÇÐ»»Ä£¿é8
        UpdateWeaponSwitching();

        // Set final weapon socket position based on all the combined animation influences
        WeaponParentSocket.localPosition =
            m_WeaponMainLocalPosition ;
    }

    //ÎäÆ÷ÇÐ»»Ä£¿é2
    // Adds a weapon to our inventory
    public bool AddWeaponUI(WeaponController weaponPrefab)
    {
        // if we already hold this weapon type (a weapon coming from the same source prefab), don't add the weapon
        if (HasWeapon(weaponPrefab) != null)
        {
            return false;
        }

        // search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // only add the weapon if the slot is free
            if (m_WeaponSlots[i] == null)
            {
                // spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab, WeaponParentSocket);
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;

                // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
                weaponInstance.Owner = gameObject;
                weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);
                
                _weaponAim = weaponInstance.gameObject.GetComponent<WeaponAim>();  

                // Assign the first person layer to the weapon
                int layerIndex =
                    Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value,
                        2)); // This function converts a layermask to a layer index
                foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer = layerIndex;
                }

                m_WeaponSlots[i] = weaponInstance;


                //OnAddedWeapon?.Invoke(weaponInstance, i);


                return true;
            }
        }

        // Handle auto-switching to weapon if no weapons currently
        if (GetActiveWeapon() == null)
        {
            SwitchWeapon(true);
        }

        return false;
    }

    //ÎäÆ÷ÇÐ»»Ä£¿é3
    public WeaponController HasWeapon(WeaponController weaponPrefab)
    {
        // Checks if we already have a weapon coming from the specified prefab
        for (var index = 0; index < m_WeaponSlots.Length; index++)
        {
            var w = m_WeaponSlots[index];
            if (w != null && w.SourcePrefab == weaponPrefab.gameObject)
            {
                return w;
            }
        }

        return null;
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é4
    public void SwitchWeapon(bool ascendingOrder)
    {
        int newWeaponIndex = -1;
        int closestSlotDistance = m_WeaponSlots.Length;
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
            // and select it if it's the closest distance yet
            if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
            {
                int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);

                if (distanceToActiveIndex < closestSlotDistance)
                {
                    closestSlotDistance = distanceToActiveIndex;
                    newWeaponIndex = i;
                }
            }
        }

        // Handle switching to the new weapon index
        SwitchToWeaponIndex(newWeaponIndex);
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é5
    // Switches to the given weapon index in weapon slots if the new index is a valid weapon that is different from our current one
    public void SwitchToWeaponIndex(int newWeaponIndex, bool force = true)
    {
        if (force || (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0))
        {
            // Store data related to weapon switching animation
            m_WeaponSwitchNewWeaponIndex = newWeaponIndex;
            m_TimeStartedWeaponSwitch = Time.time;

            // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
            if (GetActiveWeapon() == null)
            {
                m_WeaponMainLocalPosition = DownWeaponPosition.localPosition;

                m_WeaponSwitchState = WeaponSwitchState.PutUpNew;

                //indicating that the new weapon is being brought up.
                ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;

                WeaponController newWeapon = GetWeaponAtSlotIndex(m_WeaponSwitchNewWeaponIndex);

                OnSwitchedToWeapon?.Invoke(newWeapon);

            }
            // otherwise, remember we are putting down our current weapon for switching to the next one
            else
            {
                m_WeaponSwitchState = WeaponSwitchState.PutDownPrevious;
            }
        }
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é6
    public WeaponController GetWeaponAtSlotIndex(int index)
    {
        // find the active weapon in our weapon slots based on our active weapon index
        if (index >= 0 &&
            index < m_WeaponSlots.Length)
        {
            return m_WeaponSlots[index];
        }

        // if we didn't find a valid active weapon in our weapon slots, return null
        return null;
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é7
    // Calculates the "distance" between two weapon slot indexes
    // For example: if we had 5 weapon slots, the distance between slots #2 and #4 would be 2 in ascending order, and 3 in descending order
    int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
    {
        int distanceBetweenSlots = 0;

        if (ascendingOrder)
        {
            distanceBetweenSlots = toSlotIndex - fromSlotIndex;
        }
        else
        {
            distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);
        }

        if (distanceBetweenSlots < 0)
        {
            distanceBetweenSlots = m_WeaponSlots.Length + distanceBetweenSlots;
        }

        return distanceBetweenSlots;
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é9
    void UpdateWeaponSwitching()
    {
        // Calculate the time ratio (0 to 1) since weapon switch was triggered
        float switchingTimeFactor = 0f;
        if (WeaponSwitchDelay == 0f)
        {
            switchingTimeFactor = 1f;
        }
        else
        {
            switchingTimeFactor = Mathf.Clamp01((Time.time - m_TimeStartedWeaponSwitch) / WeaponSwitchDelay);
        }

        // Handle transiting to new switch state
        if (switchingTimeFactor >= 1f)
        {
            if (m_WeaponSwitchState == WeaponSwitchState.PutDownPrevious)
            {
                // Deactivate old weapon
                WeaponController oldWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                if (oldWeapon != null)
                {
                    oldWeapon.ShowWeapon(false);
                }

                ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;
                switchingTimeFactor = 0f;

                // Activate new weapon
                WeaponController newWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);

                OnSwitchedToWeapon?.Invoke(newWeapon);

                if (newWeapon)
                {
                    m_TimeStartedWeaponSwitch = Time.time;
                    m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
                }
                else
                {
                    // if new weapon is null, don't follow through with putting weapon back up
                    m_WeaponSwitchState = WeaponSwitchState.Down;
                }
            }
            else if (m_WeaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                m_WeaponSwitchState = WeaponSwitchState.Up;
            }
        }

        // Handle moving the weapon socket position for the animated weapon switching
        if (m_WeaponSwitchState == WeaponSwitchState.PutDownPrevious)
        {
            m_WeaponMainLocalPosition = Vector3.Lerp(DefaultWeaponPosition.localPosition,
                DownWeaponPosition.localPosition, switchingTimeFactor);
        }
        else if (m_WeaponSwitchState == WeaponSwitchState.PutUpNew)
        {
            m_WeaponMainLocalPosition = Vector3.Lerp(DownWeaponPosition.localPosition,
                DefaultWeaponPosition.localPosition, switchingTimeFactor);
        }
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é10
    public WeaponController GetActiveWeapon()
    {
        return GetWeaponAtSlotIndex(ActiveWeaponIndex);
    }
    //ÎäÆ÷ÇÐ»»Ä£¿é11
    void OnWeaponSwitched(WeaponController newWeapon)
    {
        if (newWeapon != null)
        {
            newWeapon.ShowWeapon(true);
        }
    }




    // Updates the weapon bob animation based on character speed
    void UpdateWeaponBob()
    {
        if (Time.deltaTime > 0f)
        {
            //Vector3 playerCharacterVelocity = m_PlayerCharacterController.CharacterVelocity;
            //(m_PlayerCharacterController.transform.position - m_LastCharacterPosition) / Time.deltaTime;
            // you can just take charactervelocity in PlayerCharacterController.cs

            // calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
            float characterMovementFactor = 0f;
            //if (m_PlayerCharacterController.IsGrounded)
            //{
            //    characterMovementFactor =
            //        Mathf.Clamp01(playerCharacterVelocity.magnitude /
            //                      (m_PlayerCharacterController.MaxSpeedOnGround *
            //                       m_PlayerCharacterController.SprintSpeedModifier));
            }

            //m_WeaponBobFactor =
            //    Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

            //// Calculate vertical and horizontal weapon bob values based on a sine function
            //float bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
        //    float frequency = BobFrequency;
        //    float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
        //    float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount *
        //                      m_WeaponBobFactor;

        //    // Apply weapon bob
        //    m_WeaponBobLocalPosition.x = hBobValue;
        //    m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

        //    m_LastCharacterPosition = m_PlayerCharacterController.transform.position;
        //}
    }

    // Updates the weapon recoil animation
    void UpdateWeaponRecoil()
    {
        // if the accumulated recoil is further away from the current position, make the current position move towards the recoil target
        if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil,
                RecoilSharpness * Time.deltaTime);
        }
        // otherwise, move recoil position to make it recover towards its resting pose
        else
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero,
                RecoilRestitutionSharpness * Time.deltaTime);
            m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
        }
    }

}
