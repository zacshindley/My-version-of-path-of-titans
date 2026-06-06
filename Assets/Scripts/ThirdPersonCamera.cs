using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Third Person View")]
    public Vector3 followOffset = new Vector3(0f, 4.2f, -9.5f);
    public float lookHeight = 2.6f;
    public float followSmoothness = 8f;
    public float rotationSmoothness = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        // Standard third-person chase camera: behind the dino, slightly above it,
        // looking at the upper body so the player sees the world ahead.
        Vector3 desiredPosition = target.TransformPoint(followOffset);
        Vector3 lookPoint = target.position + Vector3.up * lookHeight;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);

        Vector3 lookDirection = lookPoint - transform.position;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);
        }
    }
}
