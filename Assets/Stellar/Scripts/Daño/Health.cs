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
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDamageTaken;
    public UnityEvent onDeath;

    [Header("Opciones")]
    [SerializeField] private float disableDelay = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSourceOverride;

    [Header("Partículas")]
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private float damageEffectDuration = 1f;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private float deathEffectDuration = 2f;

    private AudioSource audioSource;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        audioSource = audioSourceOverride != null ? audioSourceOverride : GetComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        onHealthChanged.Invoke(currentHealth);
        onDamageTaken.Invoke();

        PlaySound(damageSound);
        StartDamageEffect();

        if (currentHealth <= 0f && !isDead)
        {
            Kill();
        }
    }

    public void Kill()
    {
        if (isDead) return;

        isDead = true;
        onDeath.Invoke();
        ScoreManager.Instance?.AddEnemyKillScore();


        PlaySound(deathSound);
        StartDeathEffect();

        float delay = Mathf.Max(deathSound != null ? deathSound.length : 0f, disableDelay, deathEffectDuration);
        if (delay > 0f)
            StartCoroutine(DisableAfterDelay(delay));
        else
            gameObject.SetActive(false);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        onHealthChanged.Invoke(currentHealth);
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    private void StartDamageEffect()
    {
        if (damageEffect == null) return;

        damageEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        damageEffect.Play();

        CancelInvoke(nameof(StopDamageEffect));
        Invoke(nameof(StopDamageEffect), damageEffectDuration);
    }

    private void StopDamageEffect()
    {
        if (damageEffect != null)
            damageEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void StartDeathEffect()
    {
        if (deathEffect == null) return;

        deathEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        deathEffect.Play();

        CancelInvoke(nameof(StopDeathEffect));
        Invoke(nameof(StopDeathEffect), deathEffectDuration);
    }

    private void StopDeathEffect()
    {
        if (deathEffect != null)
            deathEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
