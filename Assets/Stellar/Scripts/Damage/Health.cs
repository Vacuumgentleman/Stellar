using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDeath;

    [Header("Options")]
    [SerializeField] private float disableDelay = 0f;

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
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke();

        PlaySound(damageSound);
        PlayEffect(damageEffect, damageEffectDuration);

        if (currentHealth <= 0f)
        {
            Kill();
        }
    }

    public void Kill()
    {
        if (isDead) return;

        isDead = true;

        OnDeath?.Invoke();
        ScoreManager.Instance?.AddEnemyKillScore();

        PlaySound(deathSound);
        PlayEffect(deathEffect, deathEffectDuration);

        float delay = Mathf.Max(
            deathSound != null ? deathSound.length : 0f,
            disableDelay,
            deathEffectDuration
        );

        if (delay > 0f)
            StartCoroutine(DisableAfterDelay(delay));
        else
            gameObject.SetActive(false);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
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
}
