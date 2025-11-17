using UnityEngine;

public class ParticleTest : MonoBehaviour
{
    [SerializeField] private ParticleSystem testParticle;
    [SerializeField] private float duration = 1.5f;

    private void Start()
    {
        if (testParticle == null) return;

        testParticle.Play();
        Invoke(nameof(StopParticle), duration);
    }

    private void StopParticle()
    {
        testParticle?.Stop();
    }
}
