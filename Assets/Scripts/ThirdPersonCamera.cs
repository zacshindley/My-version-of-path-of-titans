using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Transform headTarget;
    public Vector3 offset = new Vector3(0f, 4.8f, -9f);
    public float followSmoothness = 6f;
    public float rotationSmoothness = 8f;
    public float lookHeight = 1.7f;

    private void LateUpdate()
    {
        if (target == null) return;

        if (headTarget == null)
        {
            headTarget = target.Find("Head");
        }

        Vector3 lookPoint = headTarget != null ? headTarget.position : target.position + Vector3.up * lookHeight;
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);

        Vector3 lookDirection = lookPoint - transform.position;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);
        }
    }
}
