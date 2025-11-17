using UnityEngine;

public class ThrusterParticles : MonoBehaviour
{
    [Header("Front Thrusters")]
    [SerializeField] private ParticleSystem frontLeft;
    [SerializeField] private ParticleSystem frontRight;

    [Header("Rear Thrusters")]
    [SerializeField] private ParticleSystem rearLeft;
    [SerializeField] private ParticleSystem rearRight;

    [Header("Top Thrusters")]
    [SerializeField] private ParticleSystem topLeft;
    [SerializeField] private ParticleSystem topRight;

    [Header("Bottom Thrusters")]
    [SerializeField] private ParticleSystem bottomLeft;
    [SerializeField] private ParticleSystem bottomRight;

    [Header("Player Controller")]
    [SerializeField] private PlayerController controller;

    private void Update()
    {
        HandleInputThrusters();
        HandleStabilizationThrusters();
    }

    // ---------------- INPUT ----------------

    private void HandleInputThrusters()
    {
        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        bool up = Input.GetKey(KeyCode.Space);
        bool down = Input.GetKey(KeyCode.LeftShift);
        bool rollLeft = Input.GetKey(KeyCode.A);
        bool rollRight = Input.GetKey(KeyCode.D);

        Toggle(rearLeft, forward);
        Toggle(rearRight, forward);

        Toggle(frontLeft, backward);
        Toggle(frontRight, backward);

        Toggle(bottomLeft, up);
        Toggle(bottomRight, up);

        Toggle(topLeft, down);
        Toggle(topRight, down);

        if (rollLeft)
        {
            Toggle(topLeft, true);
            Toggle(bottomRight, true);

            if (controller && controller.HasRotationInput() == false)
            {
                Toggle(topRight, false);
                Toggle(bottomLeft, false);
            }
        }

        if (rollRight)
        {
            Toggle(topRight, true);
            Toggle(bottomLeft, true);

            if (controller && controller.HasRotationInput() == false)
            {
                Toggle(topLeft, false);
                Toggle(bottomRight, false);
            }
        }
    }

    // ---------------- STABILIZATION ----------------

    private void HandleStabilizationThrusters()
    {
        if (!controller || !controller) return;
        if (!controller) return;

        if (!controller) return;

        if (!controller.GetAngularVelocity().Equals(Vector3.zero))
        { }

        if (!controller) return;

        if (!controller) return;

        if (!controller) return;

        Vector3 velocity = controller.GetVelocity();
        Vector3 angularVelocity = controller.GetAngularVelocity();

        bool rotating = controller.HasRotationInput();
        bool inertiaRotation = Mathf.Abs(angularVelocity.z) > 0.1f;

        // Forward/back stabilization
        bool stabilizeZ = Mathf.Abs(velocity.z) > 0.1f && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S);
        Toggle(rearLeft, stabilizeZ);
        Toggle(rearRight, stabilizeZ);
        Toggle(frontLeft, stabilizeZ);
        Toggle(frontRight, stabilizeZ);

        // Up/down stabilization
        bool stabilizeY = Mathf.Abs(velocity.y) > 0.1f && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift);
        Toggle(bottomLeft, stabilizeY);
        Toggle(bottomRight, stabilizeY);
        Toggle(topLeft, stabilizeY);
        Toggle(topRight, stabilizeY);

        // Rotational stabilization
        if (!rotating && inertiaRotation)
        {
            if (angularVelocity.z > 0)
            {
                Toggle(bottomLeft, true);
                Toggle(topRight, true);

                Toggle(bottomRight, false);
                Toggle(topLeft, false);
            }
            else
            {
                Toggle(bottomRight, true);
                Toggle(topLeft, true);

                Toggle(bottomLeft, false);
                Toggle(topRight, false);
            }
        }
        else if (!rotating && !inertiaRotation)
        {
            Toggle(topLeft, false);
            Toggle(topRight, false);
            Toggle(bottomLeft, false);
            Toggle(bottomRight, false);
        }
    }

    // ---------------- HELPERS ----------------

    private void Toggle(ParticleSystem ps, bool active)
    {
        if (!ps) return;

        if (active && !ps.isPlaying) ps.Play();
        else if (!active && ps.isPlaying) ps.Stop();
    }
}
