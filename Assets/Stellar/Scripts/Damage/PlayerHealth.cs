using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("UI de Vida")]
    [SerializeField] private Slider vidaSlider;

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
        UpdateSlider();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        PlaySound(damageSound);
        StartDamageEffect();
        UpdateSlider();

        if (currentHealth <= 0f && !isDead)
        {
            KillPlayer();
        }
    }

    public void KillPlayer()
    {
        if (isDead) return;

        isDead = true;

        PlaySound(deathSound);
        StartDeathEffect();

        Debug.Log("Jugador ha muerto");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateSlider();
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;

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

    private void UpdateSlider()
    {
        if (vidaSlider != null)
        {
            int valorRedondeado = Mathf.RoundToInt(currentHealth / 10f);
            vidaSlider.value = valorRedondeado;
        }
    }
}
