using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody shipRigidbody;

    [Header("Movement")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float drag = 3f;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float rotationAcceleration = 120f;
    [SerializeField] private float maxRotationSpeed = 90f;
    [SerializeField] private float rotationDrag = 4f;
    [SerializeField] private float rollSpeed = 100f;

    [Header("Stabilization")]
    [SerializeField] private bool stabilizationEnabled = true;
    [SerializeField] private float stabilizationFactor = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip stabilizationOnClip;
    [SerializeField] private AudioClip stabilizationOffClip;

    [SerializeField] private AudioSource engineLoopSource;
    [SerializeField] private AudioClip engineClip;
    [SerializeField] private float minEnginePitch = 0.8f;
    [SerializeField] private float maxEnginePitch = 1.2f;

    [SerializeField] private AudioSource rotationLoopSource;
    [SerializeField] private AudioClip rotationClip;
    [SerializeField] private float maxRotationVolume = 1f;

    [Header("UI")]
    [SerializeField] private Image stabilizationIcon;
    [SerializeField] private Sprite iconOn;
    [SerializeField] private Sprite iconOff;

    private Vector3 velocity = Vector3.zero;
    private Vector3 angularVelocity = Vector3.zero;

    private void Start()
    {
        if (!shipRigidbody)
        {
            Debug.LogError("Missing Rigidbody reference.");
            enabled = false;
            return;
        }

        ConfigureRigidbody();
        ConfigureAudio();
        UpdateStabilizationIcon();
    }

    private void Update()
    {
        HandleStabilizationToggle();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
        HandleEngineAudio();
        HandleRotationAudio();
    }

    // ---------------- CONFIG ----------------

    private void ConfigureRigidbody()
    {
        shipRigidbody.useGravity = false;
        shipRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        shipRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        shipRigidbody.angularDamping = 2f;
    }

    private void ConfigureAudio()
    {
        if (engineLoopSource)
        {
            engineLoopSource.clip = engineClip;
            engineLoopSource.loop = true;
            engineLoopSource.playOnAwake = false;
        }

        if (rotationLoopSource)
        {
            rotationLoopSource.clip = rotationClip;
            rotationLoopSource.loop = true;
            rotationLoopSource.playOnAwake = false;
            rotationLoopSource.volume = 0f;
        }
    }

    // ---------------- INPUT ACTIONS ----------------

    private void HandleStabilizationToggle()
    {
        if (!Input.GetKeyDown(KeyCode.LeftControl)) return;

        stabilizationEnabled = !stabilizationEnabled;

        if (sfxSource)
        {
            AudioClip clip = stabilizationEnabled ? stabilizationOnClip : stabilizationOffClip;
            if (clip) sfxSource.PlayOneShot(clip);
        }

        UpdateStabilizationIcon();
    }

    private void UpdateStabilizationIcon()
    {
        if (stabilizationIcon)
            stabilizationIcon.sprite = stabilizationEnabled ? iconOn : iconOff;
    }

    // ---------------- MOVEMENT ----------------

    private void HandleRotation()
    {
        Vector3 input = new Vector3(
            -Input.GetAxis("Mouse Y") * mouseSensitivity,
            Input.GetAxis("Mouse X") * mouseSensitivity,
            -Input.GetAxis("Horizontal")
        );

        angularVelocity += input * rotationAcceleration * Time.fixedDeltaTime;
        angularVelocity = Vector3.ClampMagnitude(angularVelocity, maxRotationSpeed);

        bool hasInput = input.sqrMagnitude > 0.0001f;

        if (stabilizationEnabled && !hasInput)
        {
            float damping = rotationDrag * Time.fixedDeltaTime * stabilizationFactor;
            angularVelocity = Vector3.Lerp(angularVelocity, Vector3.zero, damping);
        }

        Quaternion deltaRotation = Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);
        shipRigidbody.MoveRotation(shipRigidbody.rotation * deltaRotation);
    }

    private void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        float forward = Input.GetAxis("Vertical");

        if (Mathf.Abs(forward) > 0.01f)
            direction += shipRigidbody.transform.forward * forward;

        if (Input.GetKey(KeyCode.Space)) direction += shipRigidbody.transform.up;
        if (Input.GetKey(KeyCode.LeftShift)) direction -= shipRigidbody.transform.up;

        if (direction != Vector3.zero)
        {
            velocity += direction.normalized * acceleration * Time.fixedDeltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }
        else if (stabilizationEnabled)
        {
            float damping = drag * Time.fixedDeltaTime * stabilizationFactor;
            velocity = Vector3.Lerp(velocity, Vector3.zero, damping);
        }

        shipRigidbody.linearVelocity = velocity;
    }

    // ---------------- AUDIO ----------------

    private void HandleEngineAudio()
    {
        if (!engineLoopSource || !engineClip) return;

        float speed = shipRigidbody.linearVelocity.magnitude;
        float t = Mathf.InverseLerp(0f, maxSpeed, speed);

        if (speed > 0.1f)
        {
            if (!engineLoopSource.isPlaying)
                engineLoopSource.Play();

            engineLoopSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, t);
        }
        else
        {
            engineLoopSource.Stop();
        }
    }

    private void HandleRotationAudio()
    {
        if (!rotationLoopSource || !rotationClip) return;

        float inputMagnitude = new Vector3(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Horizontal")
        ).magnitude;

        if (inputMagnitude > 0.01f)
        {
            if (!rotationLoopSource.isPlaying)
                rotationLoopSource.Play();

            rotationLoopSource.volume = Mathf.Clamp01(inputMagnitude) * maxRotationVolume;
        }
        else
        {
            rotationLoopSource.volume = Mathf.MoveTowards(rotationLoopSource.volume, 0f, Time.fixedDeltaTime * 2f);
            if (rotationLoopSource.volume <= 0.01f && rotationLoopSource.isPlaying)
                rotationLoopSource.Stop();
        }
    }

    // ---------------- PUBLIC GETTERS ----------------

    public bool HasRotationInput()
    {
        return Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
               Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f ||
               Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f;
    }

    public Vector3 GetVelocity() => shipRigidbody.linearVelocity;
    public Vector3 GetAngularVelocity() => shipRigidbody.angularVelocity;
}
