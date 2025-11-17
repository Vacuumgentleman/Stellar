using UnityEngine;

public class ThrusterParticles : MonoBehaviour
{
    [Header("Propulsores - frontales")]
    public ParticleSystem frontalIzqMotor;
    public ParticleSystem frontalRightMotor;

    [Header("Propulsores - traseros")]
    public ParticleSystem traseroIzqMotor;
    public ParticleSystem traseroRightMotor;

    [Header("Propulsores - superiores")]
    public ParticleSystem superiorIzqMotor;
    public ParticleSystem superiorRightMotor;

    [Header("Propulsores - inferiores")]
    public ParticleSystem inferioresIzqMotor;
    public ParticleSystem inferioresRightMotor;

    [Header("Controlador de la nave")]
    public PlayerController playerController;

    void Update()
    {
        // Entradas directas del teclado
        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        bool up = Input.GetKey(KeyCode.Space);
        bool down = Input.GetKey(KeyCode.LeftShift);
        bool rollLeft = Input.GetKey(KeyCode.A);
        bool rollRight = Input.GetKey(KeyCode.D);

        // Propulsion hacia adelante
        ToggleMotor(forward, traseroIzqMotor);
        ToggleMotor(forward, traseroRightMotor);

        // Propulsion hacia atras
        ToggleMotor(backward, frontalIzqMotor);
        ToggleMotor(backward, frontalRightMotor);

        // Propulsion hacia arriba
        ToggleMotor(up, inferioresIzqMotor);
        ToggleMotor(up, inferioresRightMotor);

        // Propulsion hacia abajo
        ToggleMotor(down, superiorIzqMotor);
        ToggleMotor(down, superiorRightMotor);

        // Giro hacia la izquierda: activar propulsores adecuados
        if (rollLeft)
        {
            ToggleMotor(true, superiorIzqMotor);
            ToggleMotor(true, inferioresRightMotor);

            if (playerController != null && playerController.isStabilizing)
            {
                ToggleMotor(false, superiorRightMotor);
                ToggleMotor(false, inferioresIzqMotor);
            }
        }

        // Giro hacia la derecha: activar propulsores adecuados
        if (rollRight)
        {
            ToggleMotor(true, superiorRightMotor);
            ToggleMotor(true, inferioresIzqMotor);

            if (playerController != null && playerController.isStabilizing)
            {
                ToggleMotor(false, superiorIzqMotor);
                ToggleMotor(false, inferioresRightMotor);
            }
        }

        // Estabilizacion automatica si esta activa
        if (playerController != null && playerController.isStabilizing)
        {
            Vector3 velocity = playerController.GetCurrentVelocity();
            Vector3 angularVelocity = playerController.GetAngularVelocity();
            bool inputRotando = playerController.IsRotatingInput();
            bool rotandoPorInercia = Mathf.Abs(angularVelocity.z) > 0.1f;

            // Estabilizacion en el eje Z (adelante/atras)
            bool estabilizarZ = !forward && !backward && Mathf.Abs(velocity.z) > 0.1f;
            ToggleMotor(estabilizarZ, traseroIzqMotor);
            ToggleMotor(estabilizarZ, traseroRightMotor);
            ToggleMotor(estabilizarZ, frontalIzqMotor);
            ToggleMotor(estabilizarZ, frontalRightMotor);

            // Estabilizacion en el eje Y (arriba/abajo)
            bool estabilizarY = !up && !down && Mathf.Abs(velocity.y) > 0.1f;
            ToggleMotor(estabilizarY, inferioresIzqMotor);
            ToggleMotor(estabilizarY, inferioresRightMotor);
            ToggleMotor(estabilizarY, superiorIzqMotor);
            ToggleMotor(estabilizarY, superiorRightMotor);

            // Estabilizacion de rotacion por inercia
            if (!inputRotando && rotandoPorInercia)
            {
                if (angularVelocity.z > 0f)
                {
                    ToggleMotor(true, inferioresIzqMotor);
                    ToggleMotor(true, superiorRightMotor);

                    ToggleMotor(false, inferioresRightMotor);
                    ToggleMotor(false, superiorIzqMotor);
                }
                else
                {
                    ToggleMotor(true, inferioresRightMotor);
                    ToggleMotor(true, superiorIzqMotor);

                    ToggleMotor(false, inferioresIzqMotor);
                    ToggleMotor(false, superiorRightMotor);
                }
            }
            else if (!inputRotando && !rotandoPorInercia)
            {
                // Si ya esta estabilizado, apagar los motores de rotacion
                ToggleMotor(false, superiores: true);
                ToggleMotor(false, superiores: false);
            }
        }
    }

    // Metodo para encender o apagar un motor individual
    void ToggleMotor(bool active, ParticleSystem motor)
    {
        if (motor == null) return;

        if (active && !motor.isPlaying) motor.Play();
        else if (!active && motor.isPlaying) motor.Stop();
    }

    // Metodo para encender o apagar todos los motores superiores o inferiores
    void ToggleMotor(bool active, bool superiores)
    {
        if (superiores)
        {
            ToggleMotor(active, superiorIzqMotor);
            ToggleMotor(active, superiorRightMotor);
        }
        else
        {
            ToggleMotor(active, inferioresIzqMotor);
            ToggleMotor(active, inferioresRightMotor);
        }
    }
}
