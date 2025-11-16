using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    public Rigidbody shipRigidbody;

    [Header("Movimiento")]
    public float moveAcceleration = 10f;
    public float maxMoveSpeed = 10f;
    public float drag = 3f;

    [Header("Rotación")]
    public float mouseSensitivity = 2f;
    public float rotationAcceleration = 120f;
    public float maxRotationSpeed = 90f;
    public float rotationDrag = 4f;
    public float rollSpeed = 100f;

    [Header("Estabilización")]
    public bool isStabilizing = true;
    public float stabilizationFactor = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip estabilizarClip;
    [SerializeField] private AudioClip desestabilizarClip;

    [SerializeField] private AudioSource motorLoopSource;
    [SerializeField] private AudioClip motorClip;
    [SerializeField] private float minMotorPitch = 0.8f;
    [SerializeField] private float maxMotorPitch = 1.2f;

    [SerializeField] private AudioSource rotationLoopSource;
    [SerializeField] private AudioClip rotationClip;
    [SerializeField] private float maxRotationVolume = 1f;

    [Header("Indicador de Estabilización")]
    [SerializeField] private Image estabilizacionIcono;
    [SerializeField] private Sprite iconoActivado;
    [SerializeField] private Sprite iconoDesactivado;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 currentAngularVelocity = Vector3.zero;


    void Start()
    {
        ActualizarIconoEstabilizacion();

        if (!shipRigidbody)
        {
            Debug.LogError("Debes asignar un Rigidbody a 'shipRigidbody'");
            enabled = false;
            return;
        }

        shipRigidbody.useGravity = false;
        shipRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        shipRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        shipRigidbody.angularDamping = 2f;

        // Inicialización de fuentes de audio loop
        if (motorLoopSource)
        {
            motorLoopSource.clip = motorClip;
            motorLoopSource.loop = true;
            motorLoopSource.playOnAwake = false;
        }

        if (rotationLoopSource)
        {
            rotationLoopSource.clip = rotationClip;
            rotationLoopSource.loop = true;
            rotationLoopSource.playOnAwake = false;
            rotationLoopSource.volume = 0f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isStabilizing = !isStabilizing;
            Debug.Log("Estabilización " + (isStabilizing ? "ACTIVADA" : "DESACTIVADA"));

            if (sfxSource)
            {
                AudioClip clip = isStabilizing ? estabilizarClip : desestabilizarClip;
                if (clip)
                    sfxSource.PlayOneShot(clip);
            }

            ActualizarIconoEstabilizacion(); // ← Aquí
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
        HandleMotorAudio();
        HandleRotationAudio();
    }
    private void ActualizarIconoEstabilizacion()
    {
        if (estabilizacionIcono != null)
        {
            estabilizacionIcono.sprite = isStabilizing ? iconoActivado : iconoDesactivado;
        }
    }

    void HandleRotation()
    {
        Vector3 inputRotation = new Vector3(
            -Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"),
            -Input.GetAxis("Horizontal")
        );

        currentAngularVelocity += inputRotation * rotationAcceleration * Time.fixedDeltaTime;
        currentAngularVelocity = Vector3.ClampMagnitude(currentAngularVelocity, maxRotationSpeed);

        bool isRotating = inputRotation.sqrMagnitude > 0.0001f;

        if (isStabilizing && !isRotating)
        {
            float damping = rotationDrag * Time.fixedDeltaTime * stabilizationFactor;
            currentAngularVelocity = Vector3.Lerp(currentAngularVelocity, Vector3.zero, damping);
        }

        Quaternion deltaRotation = Quaternion.Euler(currentAngularVelocity * Time.fixedDeltaTime);
        shipRigidbody.MoveRotation(shipRigidbody.rotation * deltaRotation);
    }

    void HandleMovement()
    {
        Vector3 desiredDirection = Vector3.zero;

        float forwardInput = Input.GetAxis("Vertical");
        bool movingUp = Input.GetKey(KeyCode.Space);
        bool movingDown = Input.GetKey(KeyCode.LeftShift);

        if (Mathf.Abs(forwardInput) > 0.01f)
            desiredDirection += shipRigidbody.transform.forward * forwardInput;
        if (movingUp)
            desiredDirection += shipRigidbody.transform.up;
        if (movingDown)
            desiredDirection -= shipRigidbody.transform.up;

        if (desiredDirection != Vector3.zero)
        {
            currentVelocity += desiredDirection.normalized * moveAcceleration * Time.fixedDeltaTime;
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxMoveSpeed);
        }
        else if (isStabilizing)
        {
            float damping = drag * Time.fixedDeltaTime * stabilizationFactor;
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, damping);
        }

        shipRigidbody.linearVelocity = currentVelocity;
    }

    void HandleMotorAudio()
    {
        if (!motorLoopSource || !motorClip) return;

        float velocity = shipRigidbody.linearVelocity.magnitude;
        float t = Mathf.InverseLerp(0f, maxMoveSpeed, velocity);

        if (velocity > 0.1f)
        {
            if (!motorLoopSource.isPlaying)
                motorLoopSource.Play();

            motorLoopSource.pitch = Mathf.Lerp(minMotorPitch, maxMotorPitch, t);
        }
        else
        {
            motorLoopSource.Stop();
        }
    }

    void HandleRotationAudio()
    {
        if (!rotationLoopSource || !rotationClip) return;

        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");
        float inputZ = Input.GetAxis("Horizontal");

        float intensidad = new Vector3(inputX, inputY, inputZ).magnitude;

        if (intensidad > 0.01f)
        {
            if (!rotationLoopSource.isPlaying)
                rotationLoopSource.Play();

            rotationLoopSource.volume = Mathf.Clamp(intensidad, 0f, 1f) * maxRotationVolume;
        }
        else
        {
            rotationLoopSource.volume = Mathf.MoveTowards(rotationLoopSource.volume, 0f, Time.fixedDeltaTime * 2f);

            if (rotationLoopSource.volume <= 0.01f && rotationLoopSource.isPlaying)
                rotationLoopSource.Stop();
        }
    }

    public bool IsRotatingInput()
    {
        return Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
               Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f ||
               Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f;
    }

    public Vector3 GetCurrentVelocity() => shipRigidbody.linearVelocity;
    public Vector3 GetAngularVelocity() => shipRigidbody.angularVelocity;
}
