using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Transform headTarget;
    public Transform tailTarget;
    public Vector3 tailOffset = new Vector3(0f, 4.8f, -14f);
    public float followSmoothness = 5.5f;
    public float rotationSmoothness = 7f;
    public float lookHeight = 1.9f;
    public float lookAheadDistance = 3f;

    private void LateUpdate()
    {
        if (target == null) return;

        if (headTarget == null)
        {
            headTarget = FindChildContaining(target, "head");
        }

        if (tailTarget == null)
        {
            tailTarget = FindChildContaining(target, "tail");
        }

        Vector3 tailPoint = tailTarget != null ? tailTarget.position : target.position - target.forward * 2.8f;
        Vector3 desiredPosition = tailPoint + target.TransformDirection(tailOffset);
        Vector3 lookPoint = headTarget != null
            ? headTarget.position + target.forward * lookAheadDistance
            : target.position + Vector3.up * lookHeight + target.forward * lookAheadDistance;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);

        Vector3 lookDirection = lookPoint - transform.position;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);
        }
    }

    private static Transform FindChildContaining(Transform parent, string namePart)
    {
        string lowerPart = namePart.ToLowerInvariant();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLowerInvariant().Contains(lowerPart))
            {
                return child;
            }
        }

        return null;
    }
}
