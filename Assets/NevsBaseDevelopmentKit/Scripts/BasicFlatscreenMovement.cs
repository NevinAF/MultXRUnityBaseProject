using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class BasicFlatscreenMovement : MonoBehaviour
{
    //Variables
    public float speed = 6.0F;
    public float sprintSpeed = 10.0f;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    private CharacterController _characterController;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>(); 
    }

    void Update()
    {
        // is the controller on the ground?
        if (_characterController.isGrounded)
        {
            //Feed moveDirection with input.
            moveDirection = new Vector3(
                        ((Input.GetKey(PureFlatscreenManager.GetControlls().left) ? -1 : 0) + (Input.GetKey(PureFlatscreenManager.GetControlls().right) ? 1 : 0)),
                        0,
                        ((Input.GetKey(PureFlatscreenManager.GetControlls().down) ? -1 : 0) + (Input.GetKey(PureFlatscreenManager.GetControlls().up) ? 1 : 0))
                        );
            moveDirection = transform.TransformDirection(moveDirection);

            //Multiply it by speed.
            moveDirection *= ((Input.GetKey(PureFlatscreenManager.GetControlls().sprint)) ? sprintSpeed : speed);
            //Jumping
            if ((Input.GetKey(PureFlatscreenManager.GetControlls().jump)))
                moveDirection.y = jumpSpeed;

        }
        //Applying gravity to the controller
        moveDirection.y -= gravity * Time.deltaTime;
        //Making the character move
        _characterController.Move(moveDirection * Time.deltaTime);
    }
}