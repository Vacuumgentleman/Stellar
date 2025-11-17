using bullet.fx.pack;
using System.Collections.Generic;
using UnityEngine;

namespace bullets
{
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int poolSize = 60;
        [SerializeField] private Transform bulletParent;
        [SerializeField] private float bulletLifetime = 10f;

        private readonly Queue<GameObject> bullets = new();

        private void Awake()
        {
            if (bulletParent == null)
            {
                GameObject parentGO = new GameObject("PooledBullets");
                parentGO.transform.SetParent(transform);
                bulletParent = parentGO.transform;
            }

            for (int i = 0; i < poolSize; i++)
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletParent);
                bullet.SetActive(false);

                if (bullet.TryGetComponent(out Bullet b))
                    b.Init(this, bulletLifetime);

                bullets.Enqueue(bullet);
            }
        }

        public GameObject GetBullet()
        {
            if (bullets.Count == 0)
            {
                GameObject extra = Instantiate(bulletPrefab, bulletParent);
                extra.SetActive(false);

                if (extra.TryGetComponent(out Bullet extraScript))
                    extraScript.Init(this, bulletLifetime);

                bullets.Enqueue(extra);
            }

            GameObject bullet = bullets.Dequeue();
            bullet.SetActive(true);

            if (bullet.TryGetComponent(out Bullet b))
            {
                b.ResetBullet();
                b.SetLifetime(bulletLifetime);
            }

            return bullet;
        }

        public void ReturnBullet(GameObject bullet)
        {
            bullet.SetActive(false);
            bullets.Enqueue(bullet);
        }
    }
}
