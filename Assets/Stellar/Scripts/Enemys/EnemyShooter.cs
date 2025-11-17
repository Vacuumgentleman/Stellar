using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Disparo")]
    public float shootingDistance = 30f;      // Distancia maxima para disparar
    public float shootingInterval = 2f;       // Tiempo entre disparos

    [Header("Referencias")]
    public Transform target;                  // Objetivo; si esta vacio se busca automaticamente
    public bullets.Guns gun;                  // Referencia al script Guns

    private float shootTimer;

    void Start()
    {
        // Si no se asigna objetivo manualmente, buscar el jugador por etiqueta
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
    }

    void Update()
    {
        if (!target || !gun) return;

        float distance = Vector3.Distance(transform.position, target.position);
        shootTimer += Time.deltaTime;

        // Si esta dentro de rango y ha pasado el tiempo suficiente, disparar
        if (distance <= shootingDistance && shootTimer >= shootingInterval)
        {
            shootTimer = 0f;
            gun.Shoot();
        }
    }
}
