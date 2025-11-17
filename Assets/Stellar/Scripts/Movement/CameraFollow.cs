using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset (local to target rotation)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Smoothing")]
    [Range(0f, 1f)]
    [SerializeField] private float positionSmooth = 0.125f;
    [Range(0f, 1f)]
    [SerializeField] private float rotationSmooth = 0.1f;

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmooth);

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmooth);
    }
}
