using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace bullets
{
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private List<Guns> armas = new List<Guns>();
        [SerializeField] private float fireCooldown = 0.5f;

        [Header("Cooldown Visual")]
        [SerializeField] private Image cooldownImage;
        [SerializeField] private Color colorSinCooldown = Color.white;
        [SerializeField] private Color colorConCooldown = new Color(1, 1, 1, 0); // Transparente

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSound;

        private float cooldownTimer = 0f;

        private void Update()
        {
            cooldownTimer -= Time.deltaTime;

            // Disparo
            if (Input.GetMouseButton(0) && cooldownTimer <= 0f)
            {
                foreach (Guns arma in armas)
                {
                    if (arma != null)
                        arma.Shoot();
                }

                if (audioSource != null && shootSound != null)
                {
                    audioSource.PlayOneShot(shootSound);
                }

                cooldownTimer = fireCooldown;
            }

            // Actualizar color según cooldown
            UpdateCooldownVisual();
        }

        private void UpdateCooldownVisual()
        {
            if (cooldownImage == null) return;

            // Normalizamos el tiempo: 0 = listo, 1 = recién disparado
            float t = Mathf.Clamp01(cooldownTimer / fireCooldown);
            cooldownImage.color = Color.Lerp(colorSinCooldown, colorConCooldown, t);
        }
    }
}
