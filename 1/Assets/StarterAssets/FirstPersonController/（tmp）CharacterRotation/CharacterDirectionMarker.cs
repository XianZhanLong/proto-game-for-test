using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;

public class CharacterDirectionMarker : CharacterAbility
{
    /// the possible modes rotation can be based on
    public enum Modes { MovementDirection, AimDirection, None }

    [Header("Direction Marker")]
    /// the object to rotate
    [Tooltip("the object to rotate")]
    public Transform DirectionMarker;
    /// a unique ID used to reference the marker ability
    //[Tooltip("a unique ID used to reference the marker ability")]
    //public int DirectionMarkerID;
    /// the selected mode to pick direction on
    [Tooltip("the selected mode to pick direction on")]
    public Modes Mode = Modes.MovementDirection;

    [Header("Position")]
    /// the offset to apply as the center of rotation
    [Tooltip("the offset to apply as the center of rotation")]
    public Vector3 RotationCenterOffset = Vector3.zero;
    /// the axis to consider as up when aiming
    [Tooltip("the axis to consider as up when aiming")]
    public Vector3 UpVector = Vector3.up;
    /// the axis to consider as forward when aiming
    [Tooltip("the axis to consider as forward when aiming")]
    public Vector3 ForwardVector = Vector3.forward;
    /// if this is true, the marker won't be able to rotate on its X axis
    [Tooltip("if this is true, the marker won't be able to rotate on its X axis")]
    public bool PreventXRotation = false;
    /// if this is true, the marker won't be able to rotate on its Y axis
    [Tooltip("if this is true, the marker won't be able to rotate on its Y axis")]
    public bool PreventYRotation = false;
    /// if this is true, the marker won't be able to rotate on its Z axis
    [Tooltip("if this is true, the marker won't be able to rotate on its Z axis")]
    public bool PreventZRotation = false;

    [Header("Offset along magnitude")]
    /// whether or not to offset the position along the direction's magnitude (for example, moving faster could move the marker further away from the character)
    [Tooltip("whether or not to offset the position along the direction's magnitude (for example, moving faster could move the marker further away from the character)")]
    public bool OffsetAlongMagnitude = false;
    /// the minimum bounds of the velocity's magnitude
    [Tooltip("the minimum bounds of the velocity's magnitude")]
    public float MinimumVelocity = 0f;
    /// the maximum bounds of the velocity's magnitude
    [Tooltip("the maximum bounds of the velocity's magnitude")]
    public float MaximumVelocity = 7f;
    /// the distance at which to position the marker when at the lowest velocity
    [Tooltip("the distance at which to position the marker when at the lowest velocity")]
    public float OffsetRemapMin = 0f;
    /// the distance at which to position the marker when at the highest velocity
    [Tooltip("the distance at which to position the marker when at the highest velocity")]
    public float OffsetRemapMax = 1f;

    [Header("Auto Disable")]
    /// whether or not to disable the marker when the movement magnitude is under a certain threshold
    [Tooltip("whether or not to disable the marker when the movement magnitude is under a certain threshold")]
    public bool DisableBelowThreshold = false;
    /// the threshold below which to disable the marker
    [Tooltip("the threshold below which to disable the marker")]
    public float DisableThreshold = 0.1f;

    [Header("Interpolation")]
    /// whether or not to interpolate the rotation
    [Tooltip("whether or not to interpolate the rotation")]
    public bool Interpolate = false;
    /// the rate at which to interpolate the rotation
    [Tooltip("the rate at which to interpolate the rotation")]
    public float InterpolateRate = 5f;

    [Header("Interpolation")]
    protected PlayerWeaponsManagerBeta _characterHandleWeapon;
    protected WeaponAim _weaponAim;
    protected Vector3 _direction;
    protected Quaternion _newRotation;
    protected Vector3 _newPosition;
    protected Vector3 _newRotationVector;

    /// <summary>
    /// On init we store our CharacterHandleWeapon
    /// </summary>
    protected override void Initialization()
    {
        base.Initialization();
    }

    /// <summary>
    /// On Process, we aim our object
    /// </summary>
    public override void ProcessAbility()
    {
        base.ProcessAbility();
        AimMarker();
    }

    /// <summary>
    /// Rotates the object to match the selected direction
    /// </summary>
    protected virtual void AimMarker()
    {
        if (!AbilityAuthorized)
        {
            return;
        }

        if (DirectionMarker == null)
        {
            return;
        }

        switch (Mode)
        {
            //case Modes.MovementDirection:
            //        AimAt(_weaponAim.CurrentAim.normalized);
            //        break;


            case Modes.AimDirection:
                if (_weaponAim == null)
                {
                    GrabWeaponAim();
                }
                else
                {
                    AimAt(_weaponAim.CurrentAim.normalized);
                }
                break;
        }
    }

    /// <summary>
    /// Rotates the target object, interpolating the rotation if needed
    /// </summary>
    /// <param name="direction"></param>
    protected virtual void AimAt(Vector3 direction)
    {
        if (Interpolate)
        {
            _direction = Vector3.Slerp(_direction, direction, InterpolateRate* Time.deltaTime);
        }
        else
        {
            _direction = direction;
        }

        if (_direction == Vector3.zero)
        {
            return;
        }

        _newRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction, UpVector), InterpolateRate * Time.time);

        _newRotationVector.x = PreventXRotation ? 0f : _newRotation.eulerAngles.x;
        _newRotationVector.y = PreventYRotation ? 0f : _newRotation.eulerAngles.y;
        _newRotationVector.z = PreventZRotation ? 0f : _newRotation.eulerAngles.z;
        _newRotation.eulerAngles = _newRotationVector;

        DirectionMarker.transform.rotation = _newRotation;
    }


    /// <summary>
    /// Caches the weapon aim comp
    /// </summary>
    protected virtual void GrabWeaponAim()
    {
        if ((_characterHandleWeapon != null) && (_characterHandleWeapon.GetActiveWeapon() != null))
        {
            _weaponAim = _characterHandleWeapon.GetActiveWeapon().gameObject.GetComponent<WeaponAim>();//‘⁄weapon¿ÔÃÌº”weaponAim
        }
    }
}
