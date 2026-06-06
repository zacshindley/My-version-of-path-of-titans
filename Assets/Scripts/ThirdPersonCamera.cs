using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("First Person Dino View")]
    public Vector3 eyeOffset = new Vector3(0f, 2.9f, 1.35f);
    public float positionSmoothness = 14f;
    public float rotationSmoothness = 14f;

    private void LateUpdate()
    {
        if (target == null) return;

        // First-person view: camera sits at the dino's eye/head area and looks
        // straight forward in the same direction the dino is facing.
        Vector3 desiredPosition = target.TransformPoint(eyeOffset);
        Quaternion desiredRotation = target.rotation;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);
    }
}
