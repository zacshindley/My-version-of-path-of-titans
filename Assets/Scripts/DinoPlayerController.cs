using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DinoPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 13f;
    public float reverseSpeedMultiplier = 0.45f;
    public float turnSpeed = 75f;
    public float jumpHeight = 1.4f;
    public float gravity = -22f;

    [Header("Feel")]
    public Transform cameraTransform;
    public float groundedStickForce = -2f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

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

        Vector3 input = new Vector3(movementInput.x, 0f, movementInput.y).normalized;

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundedStickForce;
        }

        if (jumpPressed && controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Mathf.Abs(input.x) > 0.01f)
        {
            transform.Rotate(Vector3.up, input.x * turnSpeed * Time.deltaTime);
        }

        Vector3 move = Vector3.zero;
        if (Mathf.Abs(input.z) > 0.01f)
        {
            move = transform.forward * input.z;
        }

        float speed = sprintHeld && input.z > 0f ? sprintSpeed : walkSpeed;
        if (input.z < 0f)
        {
            speed *= reverseSpeedMultiplier;
        }
        controller.Move(move * speed * Time.deltaTime);

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}
