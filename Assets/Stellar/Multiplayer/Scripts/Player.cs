using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    // =======================
    //     INSPECTOR FIELDS
    // =======================
    [Header("References")]
    [SerializeField] private Ball ballPrefab;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Camera followCamera;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSensitivity = 2.5f;
    [SerializeField] private float rotationAcceleration = 120f;
    [SerializeField] private float maxRotationSpeed = 90f;

    [Header("Vertical Thrust")]
    [SerializeField] private float verticalThrustSpeed = 2f;
    [SerializeField] private float verticalSmoothing = 8f;

    [Header("Stabilizer")]
    [SerializeField] private KeyCode stabilizerKey = KeyCode.LeftControl;
    [SerializeField] private float stabilizerBrakeStrength = 10f;

    [Header("Gameplay")]
    [SerializeField] private float localFireCooldown = 0.5f; // cooldown base

    [Header("Cooldown Visual")]
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(1, 1, 1, 0);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;

    // =======================
    //     NETWORKED FIELDS
    // =======================
    [Networked] public int Hits { get; set; }
    [Networked] public byte ColorIndex { get; set; }
    [Networked] private TickTimer FireDelay { get; set; }

    // =======================
    //     PRIVATE STATE
    // =======================
    private NetworkCharacterController controller;
    private Vector3 forwardDirection;
    private Vector3 angularVelocity = Vector3.zero;
    private float verticalVelocity = 0f;
    private bool stabilizerEnabled = false;

    private Renderer[] renderers;
    private readonly List<Color> defaultColors = new List<Color>();
    private int lastHits = -1;

    private float cooldownTimer = 0f;
    private double lastLocalFireTime = -9999;

    private static readonly Color[] ColorPalette =
    {
        Color.blue, Color.green, Color.yellow, Color.magenta,
        Color.cyan, new Color(1f, 0.5f, 0f), Color.white, Color.gray
    };

    // =======================
    //         UNITY
    // =======================
    private void Awake()
    {
        controller = GetComponent<NetworkCharacterController>();

        if (followCamera != null)
            followCamera.gameObject.SetActive(false);

        renderers = GetComponentsInChildren<Renderer>();
        defaultColors.Clear();

        foreach (var r in renderers)
            defaultColors.Add(r.material.color);

        forwardDirection = transform.forward;

        UpdateCooldownVisual();
    }

    public override void Spawned()
    {
        if (followCamera != null)
            followCamera.gameObject.SetActive(Object.HasInputAuthority);

        Ball.RegisterPlayer(this);
        ApplyColorFromIndex(ColorIndex);
        lastHits = Hits;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Ball.UnregisterPlayer(this);
    }

    private void Update()
    {
        // Cooldown timer
        cooldownTimer -= Time.deltaTime;
        UpdateCooldownVisual();

        // Flash damage if hit
        if (lastHits != Hits)
        {
            lastHits = Hits;
            StartCoroutine(FlashDamageEffect(0.25f));
        }
    }

    private IEnumerator FlashDamageEffect(float duration)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = Color.red;

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = defaultColors[i];
    }

    // =======================
    //       COLOR SYSTEM
    // =======================
    private void ApplyColorFromIndex(byte index)
    {
        int i = Mathf.Clamp(index, 0, ColorPalette.Length - 1);
        Color color = ColorPalette[i];

        foreach (var r in renderers)
            r.material.color = color;

        for (int j = 0; j < defaultColors.Count; j++)
            defaultColors[j] = color;
    }

    public void ApplyHit()
    {
        if (Object.HasStateAuthority)
            Hits++;
    }

    // =======================
    //   INPUT & MOVEMENT
    // =======================
    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input))
            return;

        HandleRotation();
        HandleMovement(input);
        HandleVerticalMovement();
        HandleStabilizerToggle();

        // -------- SHOOTING --------
        if (Object.HasInputAuthority && input.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
        {
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = localFireCooldown;
                RPC_RequestSpawnBall();
                if (audioSource != null && shootSound != null)
                    audioSource.PlayOneShot(shootSound);
            }
        }

        // -------- COLOR & END GAME --------
        if (ColorIndex >= 0)
            ApplyColorFromIndex(ColorIndex);

        if (Object.HasStateAuthority && Hits >= 3)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
            Runner.Shutdown();
        }
    }

    private void HandleRotation()
    {
        float pitch = -Input.GetAxis("Mouse Y") * rotationSensitivity;
        float yaw = Input.GetAxis("Mouse X") * rotationSensitivity;
        float roll = -Input.GetAxis("Horizontal");

        Vector3 rotationInput = new Vector3(pitch, yaw, roll);
        angularVelocity += rotationInput * rotationAcceleration * Runner.DeltaTime;
        angularVelocity = Vector3.ClampMagnitude(angularVelocity, maxRotationSpeed);

        bool hasRotationInput = rotationInput.sqrMagnitude > 0.001f;
        if (stabilizerEnabled && !hasRotationInput)
            angularVelocity = Vector3.Lerp(angularVelocity, Vector3.zero, 4f * Runner.DeltaTime * 3f);

        transform.rotation *= Quaternion.Euler(angularVelocity * Runner.DeltaTime);
    }

    private void HandleMovement(NetworkInputData input)
    {
        Vector3 moveVector = transform.forward * input.direction.z + transform.right * input.direction.x;
        if (moveVector.sqrMagnitude > 1f)
            moveVector.Normalize();

        if (!stabilizerEnabled)
            controller.Move(moveVector * moveSpeed * Runner.DeltaTime);

        if (moveVector.sqrMagnitude > 0.01f)
            forwardDirection = transform.forward;
    }

    private void HandleVerticalMovement()
    {
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput = -1f;
        if (Input.GetKey(KeyCode.LeftShift)) verticalInput = 1f;

        verticalVelocity = stabilizerEnabled
            ? Mathf.Lerp(verticalVelocity, 0f, stabilizerBrakeStrength * Runner.DeltaTime)
            : Mathf.Lerp(verticalVelocity, verticalInput * verticalThrustSpeed, verticalSmoothing * Runner.DeltaTime);

        transform.position += new Vector3(0f, verticalVelocity * Runner.DeltaTime, 0f);
    }

    private void HandleStabilizerToggle()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(stabilizerKey))
            stabilizerEnabled = !stabilizerEnabled;
    }

    // =======================
    //      COOLDOWN UI
    // =======================
    private void UpdateCooldownVisual()
    {
        if (cooldownImage == null) return;
        float t = Mathf.Clamp01(cooldownTimer / localFireCooldown);
        cooldownImage.color = Color.Lerp(readyColor, cooldownColor, t);
    }

    // =======================
    //        RPC SPAWN
    // =======================
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSpawnBall()
    {
        if (!FireDelay.ExpiredOrNotRunning(Runner))
            return;

        FireDelay = TickTimer.CreateFromSeconds(Runner, 0.5f);

        Runner.Spawn(
            ballPrefab,
            ballSpawnPoint ? ballSpawnPoint.position : transform.position,
            Quaternion.LookRotation(forwardDirection),
            Object.InputAuthority,
            (runner, obj) => obj.GetComponent<Ball>().Init()
        );
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateColorIndex(byte index)
    {
        ColorIndex = index;
        ApplyColorFromIndex(index);
    }
}
