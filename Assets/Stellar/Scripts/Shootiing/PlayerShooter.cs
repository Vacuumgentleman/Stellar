using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace bullets
{
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private List<Guns> guns = new();
        [SerializeField] private float fireCooldown = 0.5f;

        [Header("Cooldown Visual")]
        [SerializeField] private Image cooldownImage;
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color cooldownColor = new Color(1, 1, 1, 0);

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSound;

        private float cooldownTimer = 0f;

        private void Update()
        {
            cooldownTimer -= Time.deltaTime;

            if (Input.GetMouseButton(0) && cooldownTimer <= 0f)
            {
                foreach (Guns gun in guns)
                {
                    if (gun != null)
                        gun.Shoot();
                }

                if (audioSource != null && shootSound != null)
                    audioSource.PlayOneShot(shootSound);

                cooldownTimer = fireCooldown;
            }

            UpdateCooldownVisual();
        }

        private void UpdateCooldownVisual()
        {
            if (cooldownImage == null)
                return;

            float t = Mathf.Clamp01(cooldownTimer / fireCooldown);
            cooldownImage.color = Color.Lerp(readyColor, cooldownColor, t);
        }
    }
}
