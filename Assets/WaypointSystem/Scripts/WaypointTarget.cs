using UnityEngine;
using System;

namespace WrightAngle.Waypoint
{
    /// <summary>
    /// Marca un GameObject como objetivo del sistema de Waypoints. Adjunta este componente
    /// a cualquier objeto en tu escena al que desees que apunte un marcador de waypoint.
    /// Se registra automáticamente con el WaypointUIManager si 'ActivateOnStart' está activado,
    /// o puede ser controlado manualmente desde un script.
    /// </summary>
    [AddComponentMenu("WrightAngle/Waypoint Target")]
    public class WaypointTarget : MonoBehaviour
    {
        [Tooltip("Nombre opcional para este waypoint, útil para identificarlo en el editor o scripts.")]
        public string DisplayName = "";

        [Tooltip("Si está activado, este waypoint se registrará automáticamente al iniciar la escena (requiere un WaypointUIManager en la escena). Desactívalo para controlar la activación manualmente usando el método ActivateWaypoint().")]
        public bool ActivateOnStart = true;

        /// <summary>
        /// Indica si este objetivo está actualmente registrado y siendo rastreado por el WaypointUIManager. (Solo lectura)
        /// </summary>
        public bool IsRegistered { get; private set; } = false;

        // --- Eventos estáticos para comunicación con el WaypointUIManager ---
        // Estos eventos permiten que el objetivo notifique al administrador cuando su estado cambia,
        // desacoplando los componentes.

        /// <summary>Se dispara cuando este objetivo debe activarse y ser rastreado por el administrador.</summary>
        public static event Action<WaypointTarget> OnTargetEnabled;
        /// <summary>Se dispara cuando este objetivo debe desactivarse y dejar de ser rastreado.</summary>
        public static event Action<WaypointTarget> OnTargetDisabled;

        // --- Métodos de Unity ---

        // OnEnable: El registro automático lo maneja WaypointUIManager durante su fase Start
        // para evitar problemas de orden de ejecución de scripts. La activación manual mediante
        // ActivateWaypoint() puede llamarse después de OnEnable.

        private void OnDisable()
        {
            // Asegura que el objetivo siempre se desregistre si se desactiva el componente o el GameObject.
            ProcessDeactivation();
        }

        // --- API pública ---

        /// <summary>
        /// Registra manualmente este waypoint en el sistema, haciéndolo rastreable.
        /// Úsalo si 'ActivateOnStart' está desactivado. No hace nada si ya está registrado o inactivo.
        /// </summary>
        public void ActivateWaypoint()
        {
            // Solo continuar si el GameObject está activo y no está ya registrado.
            if (!gameObject.activeInHierarchy || IsRegistered)
            {
                return;
            }

            // Notifica al WaypointUIManager (u otros oyentes) para comenzar a rastrear este objetivo.
            OnTargetEnabled?.Invoke(this);
            IsRegistered = true; // Actualiza el estado interno.
        }

        /// <summary>
        /// Desregistra manualmente este waypoint, ocultando su marcador.
        /// Permite ocultar el marcador sin desactivar el GameObject.
        /// No hace nada si no está actualmente registrado.
        /// </summary>
        public void DeactivateWaypoint()
        {
            // Lógica compartida de desactivación.
            ProcessDeactivation();
        }

        // --- Lógica interna ---

        /// <summary>
        /// Contiene la lógica compartida para desregistrar el objetivo. Actualiza el estado interno
        /// y notifica a los oyentes a través del evento OnTargetDisabled. Evita notificaciones duplicadas.
        /// </summary>
        private void ProcessDeactivation()
        {
            // Solo continuar si el objetivo estaba registrado.
            if (!IsRegistered) return;

            // Notifica al WaypointUIManager (u otros oyentes) para dejar de rastrear este objetivo.
            OnTargetDisabled?.Invoke(this);
            IsRegistered = false; // Actualiza el estado interno.
        }

        // --- Visualización en el editor ---
        // Proporciona retroalimentación visual en la escena cuando se selecciona el objeto.
        private void OnDrawGizmosSelected()
        {
            // Dibuja una esfera de alambre alrededor del objetivo.
            // El color cambia dependiendo si está registrado con el administrador.
            Gizmos.color = IsRegistered ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
            // Muestra una etiqueta útil sobre el objetivo en la vista de escena.
            string label = $"Waypoint: {gameObject.name}";
            if (!ActivateOnStart) label += " (Activación manual)";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, label);
#endif
        }
    } // Fin de la clase
} // Fin del namespace
