using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo a seguir")]
    public Transform target;

    [Header("Offset de la cámara (local al jugador)")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Suavizado")]
    [Range(0f, 1f)]
    public float positionSmoothSpeed = 0.125f;
    [Range(0f, 1f)]
    public float rotationSmoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada relativa a la rotación del jugador
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);

        // Rotación suave mirando al jugador
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed);
    }
}
