using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("True First Person Dino View")]
    public Vector3 eyeOffset = new Vector3(0f, 3.2f, 4.6f);
    public float positionSmoothness = 18f;
    public float rotationSmoothness = 18f;
    public bool hideOwnDinoMesh = true;

    private Renderer[] targetRenderers;
    private bool renderersHidden;

    private void LateUpdate()
    {
        if (target == null) return;

        // True first person: place the camera just in front of the dino's face,
        // looking exactly where the dino is facing. This avoids seeing body/legs.
        Vector3 desiredPosition = target.TransformPoint(eyeOffset);
        Quaternion desiredRotation = target.rotation;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness * Time.deltaTime);

        if (hideOwnDinoMesh && !renderersHidden)
        {
            HideTargetRenderers();
        }
    }

    private void HideTargetRenderers()
    {
        targetRenderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in targetRenderers)
        {
            renderer.enabled = false;
        }

        renderersHidden = true;
    }

    private void OnDisable()
    {
        if (targetRenderers == null) return;

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        renderersHidden = false;
    }
}
