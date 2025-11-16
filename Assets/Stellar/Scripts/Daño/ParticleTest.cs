using UnityEngine;

public class ParticleTest : MonoBehaviour
{
    public ParticleSystem testParticle;
    public float duration = 1.5f;

    private void Start()
    {
        if (testParticle != null)
        {
            Debug.Log("Llamando .Play() en testParticle");
            testParticle.Play();
            Invoke(nameof(StopParticle), duration);
        }
    }

    private void StopParticle()
    {
        if (testParticle != null)
            testParticle.Stop();
    }
}
