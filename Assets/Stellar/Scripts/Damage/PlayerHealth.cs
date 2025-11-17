using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSourceOverride;

    [Header("Particles")]
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private float damageEffectDuration = 1f;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private float deathEffectDuration = 2f;

    private AudioSource audioSource;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        audioSource = audioSourceOverride != null ? audioSourceOverride : GetComponent<AudioSource>();
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        PlaySound(damageSound);
        PlayEffect(damageEffect, damageEffectDuration);
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;

        PlaySound(deathSound);
        PlayEffect(deathEffect, deathEffectDuration);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider == null) return;

        healthSlider.value = Mathf.RoundToInt(currentHealth / 10f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private void PlayEffect(ParticleSystem effect, float duration)
    {
        if (effect == null) return;

        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.Play();

        CancelInvoke(nameof(StopEffect));
        Invoke(nameof(StopEffect), duration);
    }

    private void StopEffect()
    {
        damageEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        deathEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
}
