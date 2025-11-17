using UnityEngine;
using System;

namespace WrightAngle.Waypoint
{
    /// <summary>
    /// Marks a GameObject as a waypoint target. Attach this to scene objects you want to track.
    /// It can auto-register at start (ActivateOnStart) or be registered manually via ActivateWaypoint().
    /// </summary>
    [AddComponentMenu("WrightAngle/Waypoint Target")]
    public class WaypointTarget : MonoBehaviour
    {
        [Tooltip("Optional display name for editor or debugging.")]
        public string DisplayName = "";

        [Tooltip("If true, the target registers itself on scene start (if active).")]
        public bool ActivateOnStart = true;

        /// <summary>True when registered with the WaypointUIManager.</summary>
        public bool IsRegistered { get; private set; } = false;

        /// <summary>Fired to request registration with the manager.</summary>
        public static event Action<WaypointTarget> OnTargetEnabled;

        /// <summary>Fired to request unregistration from the manager.</summary>
        public static event Action<WaypointTarget> OnTargetDisabled;

        private void OnDisable()
        {
            // Ensure we notify manager if this target is disabled.
            ProcessDeactivation();
        }

        /// <summary>
        /// Manually register this target with the waypoint system.
        /// Safe to call when ActivateOnStart is false.
        /// </summary>
        public void ActivateWaypoint()
        {
            if (!gameObject.activeInHierarchy || IsRegistered) return;
            OnTargetEnabled?.Invoke(this);
            IsRegistered = true;
        }

        /// <summary>
        /// Manually unregister this target.
        /// </summary>
        public void DeactivateWaypoint()
        {
            ProcessDeactivation();
        }

        private void ProcessDeactivation()
        {
            if (!IsRegistered) return;
            OnTargetDisabled?.Invoke(this);
            IsRegistered = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsRegistered ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
            string label = $"Waypoint: {gameObject.name}";
            if (!ActivateOnStart) label += " (manual activation)";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, label);
#endif
        }
    }
}
