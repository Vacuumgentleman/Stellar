using UnityEngine;

namespace bullets
{
    public sealed class Guns : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform mira;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float gravityForce = 5f;
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private int weaponDamage = 20; // <- DAÑO configurado desde el inspector

        public void Shoot()
        {
            if (firePoint == null || mira == null || bulletPool == null) return;

            Vector3 direction = mira.position - firePoint.position;

            GameObject bullet = bulletPool.GetBullet();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);

            if (bullet.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = direction.normalized * bulletSpeed;
                rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
            }

            // Asignar daño si el componente Bullet existe
            if (bullet.TryGetComponent(out bullet.fx.pack.Bullet bulletScript))
            {
                bulletScript.SetDamage(weaponDamage);
            }
        }
    }
}
