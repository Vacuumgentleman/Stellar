using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shootingDistance = 30f;
    public float shootingInterval = 2f;

    [Header("References")]
    public Transform target;
    public bullets.Guns gun;

    private float shootTimer;

    private void Start()
    {
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
    }

    private void Update()
    {
        if (!target || !gun) return;

        float distance = Vector3.Distance(transform.position, target.position);
        shootTimer += Time.deltaTime;

        if (distance <= shootingDistance && shootTimer >= shootingInterval)
        {
            shootTimer = 0f;
            gun.Shoot();
        }
    }
}
