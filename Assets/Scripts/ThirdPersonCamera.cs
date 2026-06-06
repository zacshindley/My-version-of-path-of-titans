using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Camera Toggle")]
    public bool firstPerson = true;
    public Key toggleViewKey = Key.V;

    [Header("First Person Dino View")]
    public Vector3 firstPersonEyeOffset = new Vector3(0f, 3.2f, 4.6f);
    public float firstPersonFieldOfView = 72f;
    public bool hideOwnDinoMeshInFirstPerson = true;

    [Header("Third Person Dino View")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 4.2f, -9.5f);
    public float thirdPersonLookHeight = 2.6f;
    public float thirdPersonFieldOfView = 58f;

    [Header("Feel")]
    public float positionSmoothness = 14f;
    public float rotationSmoothness = 14f;

    private Renderer[] targetRenderers;
    private bool renderersHidden;
    private Camera attachedCamera;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[toggleViewKey].wasPressedThisFrame)
        {
            firstPerson = !firstPerson;
        }

        if (firstPerson)
        {
            UpdateFirstPersonView();
        }
        else
        {
            UpdateThirdPersonView();
        }
    }

    private void UpdateFirstPersonView()
    {
        Vector3 desiredPosition = target.TransformPoint(firstPersonEyeOffset);
        Quaternion desiredRotation = target.rotation;

        MoveCamera(desiredPosition, desiredRotation, firstPersonFieldOfView);

        if (hideOwnDinoMeshInFirstPerson && !renderersHidden)
        {
            SetTargetRenderersVisible(false);
        }
    }

    private void UpdateThirdPersonView()
    {
        if (renderersHidden)
        {
            SetTargetRenderersVisible(true);
        }

        Vector3 desiredPosition = target.TransformPoint(thirdPersonOffset);
        Vector3 lookPoint = target.position + Vector3.up * thirdPersonLookHeight;
        Vector3 lookDirection = lookPoint - desiredPosition;
        Quaternion desiredRotation = lookDirection.sqrMagnitude > 0.01f
            ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
            : target.rotation;

        MoveCamera(desiredPosition, desiredRotation, thirdPersonFieldOfView);
    }

    private void MoveCamera(Vector3 desiredPosition, Quaternion desiredRotation, float fieldOfView)
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);

        if (attachedCamera != null)
        {
            attachedCamera.fieldOfView = Mathf.Lerp(attachedCamera.fieldOfView, fieldOfView, 10f * Time.deltaTime);
        }
    }

    private void SetTargetRenderersVisible(bool visible)
    {
        if (targetRenderers == null)
        {
            targetRenderers = target.GetComponentsInChildren<Renderer>();
        }

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }

        renderersHidden = !visible;
    }

    private void OnDisable()
    {
        if (renderersHidden)
        {
            SetTargetRenderersVisible(true);
        }
    }
}
