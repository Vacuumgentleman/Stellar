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

    [Header("Target")]
    public Transform target;

    [Header("Movement")]
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float stopDistance = 30f;

    [Header("Obstacle Avoidance")]
    public float detectionDistance = 15f;
    public float avoidTime = 2f;
    public LayerMask obstacleMask;

    [Header("Combat Behavior")]
    public CombatBehavior behavior = CombatBehavior.StopAndShoot;

    private Rigidbody rb;
    private bool isAvoiding;
    private Vector3 avoidanceDirection;
    private float avoidTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
            return;
        }

        Vector3 toPlayer = target.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        Vector3 moveDir = toPlayer.normalized;

        if (isAvoiding)
        {
            avoidTimer -= Time.fixedDeltaTime;
            if (avoidTimer <= 0f) isAvoiding = false;
            else moveDir = (moveDir + avoidanceDirection).normalized;
        }
        else if (Physics.Raycast(transform.position, transform.forward, detectionDistance, obstacleMask))
        {
            Vector3[] dirs =
            {
                transform.right, -transform.right,
                transform.up, -transform.up,
                -transform.forward
            };

            foreach (var dir in dirs)
            {
                if (!Physics.Raycast(transform.position, dir, detectionDistance * 0.8f, obstacleMask))
                {
                    avoidanceDirection = dir;
                    isAvoiding = true;
                    avoidTimer = avoidTime;
                    moveDir = (moveDir + dir).normalized;
                    break;
                }
            }

            if (!isAvoiding) return;
        }

        switch (behavior)
        {
            case CombatBehavior.Approach:
                MoveAndRotate(moveDir);
                break;

            case CombatBehavior.StopAndShoot:
                if (distanceToPlayer > stopDistance)
                    MoveAndRotate(moveDir);
                else
                    RotateOnly(moveDir);
                break;

            case CombatBehavior.Orbit:
                if (distanceToPlayer > stopDistance)
                    MoveAndRotate(moveDir);
                else
                {
                    Vector3 orbitDir = Vector3.Cross(Vector3.up, toPlayer).normalized;
                    MoveAndRotate(orbitDir);
                }
                break;

            case CombatBehavior.Ambush:
                if (distanceToPlayer > stopDistance * 1.2f)
                    MoveAndRotate(moveDir);
                else
                {
                    Vector3 ambushDir = (moveDir + transform.up * 0.5f).normalized;
                    MoveAndRotate(ambushDir);
                }
                break;
        }
    }

    private void MoveAndRotate(Vector3 dir)
    {
        Quaternion look = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, rotationSpeed * Time.fixedDeltaTime));
        rb.MovePosition(rb.position + rb.transform.forward * speed * Time.fixedDeltaTime);
    }

    private void RotateOnly(Vector3 dir)
    {
        Quaternion look = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, rotationSpeed * Time.fixedDeltaTime));
    }

    private void OnEnable()
    {
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance);
    }
}
