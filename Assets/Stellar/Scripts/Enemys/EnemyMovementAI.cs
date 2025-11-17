using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovementAI : MonoBehaviour
{
    public enum CombatBehavior
    {
        Approach,
        StopAndShoot,
        Orbit,
        Ambush
    }

    [Header("Jugador (opcional, se puede asignar en tiempo real)")]
    public Transform target;

    [Header("Movimiento")]
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float stopDistance = 30f;

    [Header("Evitacion de obstaculos")]
    public float detectionDistance = 15f;
    public float avoidTime = 2f;
    public LayerMask obstacleMask;

    [Header("Comportamiento de combate")]
    public CombatBehavior behavior = CombatBehavior.StopAndShoot;

    private Rigidbody rb;
    private bool isAvoiding;
    private Vector3 avoidanceDirection;
    private float avoidTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // Buscar jugador si no esta asignado
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
            return;
        }

        Vector3 toPlayer = (target.position - transform.position);
        float distanceToPlayer = toPlayer.magnitude;
        Vector3 moveDirection = toPlayer.normalized;

        // Evitar obstaculos
        if (isAvoiding)
        {
            avoidTimer -= Time.fixedDeltaTime;
            if (avoidTimer <= 0f) isAvoiding = false;
            else moveDirection = (moveDirection + avoidanceDirection).normalized;
        }
        else if (Physics.Raycast(transform.position, transform.forward, detectionDistance, obstacleMask))
        {
            Vector3[] directions = {
                transform.right, -transform.right,
                transform.up, -transform.up,
                -transform.forward
            };

            foreach (var dir in directions)
            {
                if (!Physics.Raycast(transform.position, dir, detectionDistance * 0.8f, obstacleMask))
                {
                    avoidanceDirection = dir;
                    isAvoiding = true;
                    avoidTimer = avoidTime;
                    moveDirection = (moveDirection + dir).normalized;
                    break;
                }
            }

            if (!isAvoiding) return;
        }

        // Comportamientos de combate
        switch (behavior)
        {
            case CombatBehavior.Approach:
                MoveAndRotate(moveDirection);
                break;

            case CombatBehavior.StopAndShoot:
                if (distanceToPlayer > stopDistance)
                    MoveAndRotate(moveDirection);
                else
                    RotateOnly(moveDirection);
                break;

            case CombatBehavior.Orbit:
                if (distanceToPlayer > stopDistance)
                    MoveAndRotate(moveDirection);
                else
                {
                    Vector3 orbitDir = Vector3.Cross(Vector3.up, toPlayer).normalized;
                    MoveAndRotate(orbitDir);
                }
                break;

            case CombatBehavior.Ambush:
                if (distanceToPlayer > stopDistance * 1.2f)
                    MoveAndRotate(moveDirection);
                else
                {
                    Vector3 ambushDir = (moveDirection + transform.up * 0.5f).normalized;
                    MoveAndRotate(ambushDir);
                }
                break;
        }
    }

    // Mover y rotar el enemigo en una direccion
    void MoveAndRotate(Vector3 direction)
    {
        Quaternion look = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, rotationSpeed * Time.fixedDeltaTime));
        rb.MovePosition(rb.position + rb.transform.forward * speed * Time.fixedDeltaTime);
    }

    // Solo rotar el enemigo hacia una direccion
    void RotateOnly(Vector3 direction)
    {
        Quaternion look = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, rotationSpeed * Time.fixedDeltaTime));
    }

    void OnEnable()
    {
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
    }

    // Gizmos para depuracion en el editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance);
    }
}
