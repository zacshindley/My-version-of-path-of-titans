using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Transform headTarget;
    public Vector3 followOffset = new Vector3(0f, 7.5f, -18f);
    public float followSmoothness = 5.5f;
    public float rotationSmoothness = 7f;
    public float lookHeight = 2.7f;
    public float lookAheadDistance = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        if (headTarget == null)
        {
            headTarget = FindChildContaining(target, "head");
        }

        Vector3 desiredPosition = target.position + target.TransformDirection(followOffset);
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
