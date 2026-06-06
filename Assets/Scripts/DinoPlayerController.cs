using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

[RequireComponent(typeof(CharacterController))]
public class DinoPlayerController : MonoBehaviour
{
    private const string QuaterniusTrexPath = "Assets/External/Quaternius/Dinosaur Animated Pack - Dec 2018/FBX/Trex.fbx";
    private const string GeneratedAnimatorPath = "Assets/Generated/TedTrexAnimator.controller";

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
#if UNITY_EDITOR
            if (TryReplacePrimitiveVisualWithImportedTrex())
            {
                return;
            }
#endif
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

#if UNITY_EDITOR
        TryReplacePrimitiveVisualWithImportedTrex();
#endif
    }

#if UNITY_EDITOR
    private bool TryReplacePrimitiveVisualWithImportedTrex()
    {
        // In Zac's current workflow he may still be opening the old saved scene.
        // This upgrades that old primitive placeholder automatically in Editor Play mode.
        GameObject trexPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuaterniusTrexPath);
        if (trexPrefab == null)
        {
            return false;
        }

        foreach (Transform child in transform)
        {
            if (child.name.ToLowerInvariant().Contains("quaternius"))
            {
                visualRoot = child;
                dinoAnimator = visualRoot.GetComponentInChildren<Animator>();
                AssignTrexAnimatorController();
                return true;
            }
        }

        List<GameObject> oldVisualObjects = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Camera>() != null) continue;
            oldVisualObjects.Add(child.gameObject);
        }

        GameObject trex = (GameObject)PrefabUtility.InstantiatePrefab(trexPrefab);
        trex.name = "Quaternius Animated T-Rex - runtime upgraded visual";
        trex.transform.SetParent(transform, false);
        trex.transform.localPosition = Vector3.zero;
        trex.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        trex.transform.localScale = Vector3.one * 1.8f;
        visualRoot = trex.transform;

        foreach (GameObject oldObject in oldVisualObjects)
        {
            Destroy(oldObject);
        }

        dinoAnimator = visualRoot.GetComponentInChildren<Animator>();
        if (dinoAnimator == null)
        {
            dinoAnimator = trex.AddComponent<Animator>();
        }
        AssignTrexAnimatorController();
        return true;
    }

    private void AssignTrexAnimatorController()
    {
        if (dinoAnimator == null) return;

        RuntimeAnimatorController existingController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(GeneratedAnimatorPath);
        if (existingController == null)
        {
            existingController = BuildTrexAnimatorController();
        }

        if (existingController != null)
        {
            dinoAnimator.runtimeAnimatorController = existingController;
        }
    }

    private RuntimeAnimatorController BuildTrexAnimatorController()
    {
        const string generatedFolder = "Assets/Generated";
        if (!AssetDatabase.IsValidFolder(generatedFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(GeneratedAnimatorPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimationClip idle = FindTrexClip("Idle", "Idl");
        AnimationClip walk = FindTrexClip("Walk");
        AnimationClip run = FindTrexClip("Run");

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idleState = machine.AddState("Idle");
        idleState.motion = idle != null ? idle : walk;
        machine.defaultState = idleState;

        AnimatorState walkState = machine.AddState("Walk");
        walkState.motion = walk != null ? walk : idleState.motion;

        AnimatorState runState = machine.AddState("Run");
        runState.motion = run != null ? run : walkState.motion;

        AddSpeedTransition(idleState, walkState, 0.25f, true);
        AddSpeedTransition(walkState, idleState, 0.2f, false);
        AddSpeedTransition(walkState, runState, 8f, true);
        AddSpeedTransition(runState, walkState, 7.5f, false);

        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void AddSpeedTransition(AnimatorState from, AnimatorState to, float threshold, bool greater)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.18f;
        transition.AddCondition(greater ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, "Speed");
    }

    private static AnimationClip FindTrexClip(params string[] nameParts)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(QuaterniusTrexPath);
        foreach (Object asset in assets)
        {
            AnimationClip clip = asset as AnimationClip;
            if (clip == null) continue;
            string lower = clip.name.ToLowerInvariant();
            foreach (string part in nameParts)
            {
                if (lower.Contains(part.ToLowerInvariant()))
                {
                    return clip;
                }
            }
        }

        return null;
    }
#endif

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
