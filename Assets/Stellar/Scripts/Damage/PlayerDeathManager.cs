using UnityEngine;
using System.Collections;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("Referencias necesarias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject objectToDisable;     // Cámara principal, HUD, etc.
    [SerializeField] private GameObject deathCamera;
    [SerializeField] private GameObject deathCanvas;

    [Header("Objetos adicionales a controlar")]
    [SerializeField] private GameObject additionalObject1;   // EnemyManager
    [SerializeField] private GameObject additionalObject2;   // PauseManager

    [Header("Tiempo antes de cambiar cámara tras morir")]
    [SerializeField] private float delayBeforeSwitch = 2f;

    private bool wasDead = false;

    private void Start()
    {
        // Activar todo lo necesario al iniciar
        if (objectToDisable != null)
            objectToDisable.SetActive(true);

        if (deathCamera != null)
            deathCamera.SetActive(false);

        if (deathCanvas != null)
            deathCanvas.SetActive(false);

        if (additionalObject1 != null)
            additionalObject1.SetActive(true);

        if (additionalObject2 != null)
            additionalObject2.SetActive(true);

        // Bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (wasDead || playerHealth == null) return;

        if (playerHealth.IsDead())
        {
            wasDead = true;
            StartCoroutine(HandleDeathTransition());
        }
    }

    private IEnumerator HandleDeathTransition()
    {
        yield return new WaitForSeconds(delayBeforeSwitch);

        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        if (deathCamera != null)
            deathCamera.SetActive(true);

        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        if (additionalObject1 != null)
            additionalObject1.SetActive(false);

        if (additionalObject2 != null)
            additionalObject2.SetActive(false);

        // Liberar y mostrar el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
