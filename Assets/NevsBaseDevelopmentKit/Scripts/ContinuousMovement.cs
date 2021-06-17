using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/**
* Author: Nevin Foster
* ContinuousMovement is intended to be attached to a SteamVR controlled player object.
* Uses VR Input and look dirction to update a CharacterController.
*/
[RequireComponent(typeof(XRRig))]
[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    [Tooltip("Float input for move strength and direction")]
    public InputFeatureUsage<Vector2> moveDirection = CommonUsages.primary2DAxis;
    [Tooltip("The max speed will toggle, but turn off once player stops moving")]
    public InputFeatureUsage<bool> sprintButton = CommonUsages.primary2DAxisClick;

    [Header("Interaction")]
    [Tooltip("The max speed will toggle, but turn off once player stops moving")]
    public XRNode inputSource;


    [Header("Movement")]
    [Tooltip("The max player speed in meters/second")]
    public float speed = 1;
    [Tooltip("The player sprint speed in meters/second")]
    public float sprintSpeed = 1;
    [Tooltip("Gravity is calculated in the update function of this class. Gravity in meters/sec^2")]
    public float gravity = -9.81f;
    [Tooltip("Ground coliders. Helpful if the ground is different than walls")]
    public LayerMask groundLayer;
    [Tooltip("Increases the character collider height, intended to simulate the forehead")]
    public float additionalHeight;
    [Tooltip("The distance at which the character controller detects ground. Useful for padding and preventing unwanted ground sliding")]
    public float groundRaycastLength = 0.003f;
    [Tooltip("The angle at which the player no longer is affected by gravity. Useful for ground terrains without physics materials")]
    [Range(0, 90)]
    public float groundedAngle = 5f;

    [Header("Audio")]
    [Tooltip("Audio Clip that plays only when moving.")]
    public AudioClip audioClipOnMove = null;
    [Range(0, 1)]
    [Tooltip("Volume at which the movement clip plays")]
    public float audioVolume;

    // Attached Components
    private XRRig _XRRig;
    private CharacterController _characterController;
    private AudioSource _audioSource;

    // private variables
    private InputDeviceWrapper targetDevice;
    private float fallingSpeed;
    private Vector2 inputAxis;
    private bool isSprinting;
    private bool audioIsPlaying = false;

    void Start()
    {
        InputDeviceWrapper.FindInputDevice(inputSource, out targetDevice, this);
        _XRRig = GetComponent<XRRig>();
        _characterController = GetComponent<CharacterController>();
        if (audioClipOnMove) // Checks for attacked audio clip and audio source.
            if ((_audioSource = GetComponent<AudioSource>()) == null)
            {
                Debug.LogWarning("There is an audio clip but no Audio Source Not found on object. Disabling audio on move");
                audioClipOnMove = null;
            }

        audioIsPlaying = false;
        isSprinting = false;
    }

    void Update()
    {
        // We update the input value on updates but do all physics during the fixed update.
        targetDevice.TryGetFeatureValue(moveDirection, out inputAxis);
        targetDevice.TryGetFeatureValue(sprintButton, out bool sprintButtonValue);

        if (inputAxis.x + inputAxis.y < Mathf.Epsilon)
            isSprinting = false;

        else if (sprintButtonValue)
            isSprinting = !isSprinting;

        // play audio if needed
        if (audioClipOnMove)
            UpdateMoveAudio();
    }

    // TODO:: When audio stops, All audio coming from this source is stopped. Fix that.
    /// <summary>
    /// Checks if input values are not 0 and if so, then the player is moving and audio is played.
    /// </summary>
    private void UpdateMoveAudio()
    {
        if (inputAxis.x + inputAxis.y > Mathf.Epsilon)
        {
            if (!audioIsPlaying)
            {
                audioIsPlaying = true;
                _audioSource.PlayOneShot(audioClipOnMove, 0.7f);
            }
        }
        else if (audioIsPlaying)
        {
            audioIsPlaying = false;
            _audioSource.Stop();
        }
    }

    private void FixedUpdate()
    {
        CapsuleFollowHeadset();

        Quaternion rotation = Quaternion.Euler(0, _XRRig.cameraGameObject.transform.eulerAngles.y, 0);
        Vector3 direction = rotation * new Vector3(inputAxis.x, 0, inputAxis.y);
        _characterController.Move(direction * Time.fixedDeltaTime * (isSprinting ? sprintSpeed : speed));

        if (CheckGrounded())
            fallingSpeed = 0;
        else
            fallingSpeed += gravity * Time.fixedDeltaTime;

        _characterController.Move(Vector3.up * fallingSpeed * Time.fixedDeltaTime);


    }

    /// <summary>
    /// Updates the center of the capsule collider to the headset. The y position is set instead by using the half the eye height.
    /// </summary>
    void CapsuleFollowHeadset()
    {
        _characterController.height = _XRRig.cameraInRigSpaceHeight + additionalHeight;
        Vector3 campos = _XRRig.cameraInRigSpacePos;
        _characterController.center = new Vector3(campos.x, (_characterController.height) / 2 + _characterController.skinWidth, campos.z);
    }

    bool CheckGrounded()
    {
        Vector3 rayStart = transform.TransformPoint(_characterController.center);
        float rayLenth = _characterController.center.y + groundRaycastLength;
        if (Physics.SphereCast(rayStart, _characterController.radius, Vector3.down, out RaycastHit hitInfo, rayLenth, groundLayer))
            return groundedAngle < Vector3.Angle(hitInfo.normal, Vector3.down);
        return false;

    }

    /// <summary>
    /// This check the current values for any possible flaws.
    /// </summary>
    private void OnValidate()
    {
        if (speed > sprintSpeed)
            Debug.LogWarning("Sprint speed is lower than the normal walk speed. Consider changing these values.");
    }
}
