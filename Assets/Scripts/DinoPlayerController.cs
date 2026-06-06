using System.Collections.Generic;
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

    [Header("Creature Feel")]
    public Transform cameraTransform;
    public float acceleration = 7f;
    public float deceleration = 10f;
    public float groundedStickForce = -2f;
    public float slopeLeanAmount = 10f;
    public float legSwingAmount = 18f;
    public float legSwingSpeed = 4.5f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float currentForwardSpeed;
    private Transform visualRoot;
    private Animator dinoAnimator;
    private Quaternion visualBaseRotation;
    private Transform[] animatedLimbs;
    private Quaternion[] animatedLimbBaseRotations;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        EnsureVisualRootFacesUnityForward();
        if (visualRoot != null)
        {
            visualBaseRotation = visualRoot.localRotation;
            dinoAnimator = visualRoot.GetComponentInChildren<Animator>();
            CacheAnimatedLimbs();
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

        if (dinoAnimator != null)
        {
            dinoAnimator.SetFloat("Speed", Mathf.Abs(currentForwardSpeed));
        }

        UpdateBodyAndLegs();
    }

    private void EnsureVisualRootFacesUnityForward()
    {
        visualRoot = transform.Find("Dino Visual - nose points Unity forward");
        if (visualRoot != null)
        {
            return;
        }

        foreach (Transform child in transform)
        {
            if (child.GetComponentInChildren<Animator>() != null || child.name.ToLowerInvariant().Contains("quaternius"))
            {
                visualRoot = child;
                return;
            }
        }

        List<Transform> visualChildren = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Camera>() != null) continue;
            visualChildren.Add(child);
        }

        if (visualChildren.Count == 0)
        {
            return;
        }

        GameObject wrapper = new GameObject("Dino Visual - nose points Unity forward");
        visualRoot = wrapper.transform;
        visualRoot.SetParent(transform, false);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.Euler(0f, -90f, 0f);
        visualRoot.localScale = Vector3.one;

        foreach (Transform child in visualChildren)
        {
            child.SetParent(visualRoot, false);
        }
    }

    private void CacheAnimatedLimbs()
    {
        Transform[] allChildren = visualRoot.GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<Transform> limbs = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child == visualRoot) continue;
            if (child.name.Contains("Thigh") || child.name.Contains("Shin") || child.name.Contains("Foot"))
            {
                limbs.Add(child);
            }
        }

        animatedLimbs = limbs.ToArray();
        animatedLimbBaseRotations = new Quaternion[animatedLimbs.Length];
        for (int i = 0; i < animatedLimbs.Length; i++)
        {
            animatedLimbBaseRotations[i] = animatedLimbs[i].localRotation;
        }
    }

    private void UpdateBodyAndLegs()
    {
        if (visualRoot == null) return;

        Vector3 groundNormal = Vector3.up;
        if (Physics.Raycast(transform.position + Vector3.up * 1.2f, Vector3.down, out RaycastHit hit, 4f))
        {
            groundNormal = hit.normal;
        }

        Vector3 localNormal = transform.InverseTransformDirection(groundNormal);
        float pitch = Mathf.Clamp(localNormal.z * -slopeLeanAmount, -slopeLeanAmount, slopeLeanAmount);
        float roll = Mathf.Clamp(localNormal.x * slopeLeanAmount, -slopeLeanAmount, slopeLeanAmount);
        Quaternion slopeTilt = Quaternion.Euler(pitch, 0f, roll);
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, visualBaseRotation * slopeTilt, 8f * Time.deltaTime);

        if (animatedLimbs == null || animatedLimbs.Length == 0) return;

        float speed01 = Mathf.Clamp01(Mathf.Abs(currentForwardSpeed) / sprintSpeed);
        float gait = Time.time * legSwingSpeed * Mathf.Lerp(0.6f, 1.6f, speed01);
        for (int i = 0; i < animatedLimbs.Length; i++)
        {
            float sidePhase = i % 2 == 0 ? 0f : Mathf.PI;
            float swing = Mathf.Sin(gait + sidePhase) * legSwingAmount * speed01;
            animatedLimbs[i].localRotation = animatedLimbBaseRotations[i] * Quaternion.Euler(swing, 0f, 0f);
        }
    }
}
