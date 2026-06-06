using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DinoPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 13f;
    public float reverseSpeedMultiplier = 0.45f;
    public float turnSpeed = 70f;
    public float stationaryTurnMultiplier = 0.55f;
    public float jumpHeight = 1.4f;
    public float gravity = -22f;

    [Header("Feel")]
    public Transform cameraTransform;
    public float acceleration = 7f;
    public float deceleration = 10f;
    public float groundedStickForce = -2f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float currentForwardSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Vector2 movementInput = Vector2.zero;
        bool jumpPressed = false;
        bool sprintHeld = false;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                movementInput.x -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                movementInput.x += 1f;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                movementInput.y -= 1f;
            }
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                movementInput.y += 1f;
            }

            jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            sprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }

        float turnInput = Mathf.Clamp(movementInput.x, -1f, 1f);
        float moveInput = Mathf.Clamp(movementInput.y, -1f, 1f);

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundedStickForce;
        }

        if (jumpPressed && controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        float turnMultiplier = Mathf.Abs(currentForwardSpeed) > 0.2f ? 1f : stationaryTurnMultiplier;
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            transform.Rotate(Vector3.up, turnInput * turnSpeed * turnMultiplier * Time.deltaTime);
        }

        float targetSpeed = 0f;
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            targetSpeed = sprintHeld && moveInput > 0f ? sprintSpeed : walkSpeed;
            targetSpeed *= moveInput;

            if (moveInput < 0f)
            {
                targetSpeed *= reverseSpeedMultiplier;
            }
        }

        float speedChangeRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
        controller.Move(transform.forward * currentForwardSpeed * Time.deltaTime);

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}
