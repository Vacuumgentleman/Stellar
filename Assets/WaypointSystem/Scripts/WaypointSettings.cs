using UnityEngine;

namespace WrightAngle.Waypoint
{
    /// <summary>
    /// ScriptableObject that stores global configuration for the waypoint system.
    /// Create via: Assets -> Create -> WrightAngle -> Waypoint Settings
    /// </summary>
    [CreateAssetMenu(fileName = "WaypointSettings", menuName = "WrightAngle/Waypoint Settings", order = 1)]
    public class WaypointSettings : ScriptableObject
    {
        public enum ProjectionMode { Mode3D, Mode2D }
        public enum DistanceUnitSystem { Metric, Imperial }

        [Header("Core Functionality")]
        [Tooltip("Update frequency (seconds) for the waypoint manager.")]
        [Range(0.01f, 1.0f)]
        public float UpdateFrequency = 0.1f;

        [Tooltip("Choose Mode3D for perspective cameras, Mode2D for orthographic.")]
        public ProjectionMode GameMode = ProjectionMode.Mode3D;

        [Tooltip("Prefab used as the marker UI element.")]
        public GameObject MarkerPrefab;

        [Tooltip("Maximum world distance at which a marker is visible.")]
        public float MaxVisibleDistance = 1000f;

        [Tooltip("When in Mode2D, ignore Z axis for distance checks.")]
        public bool IgnoreZAxisForDistance2D = true;

        [Header("Off-Screen Indicator")]
        [Tooltip("Show clamped markers at screen edges when targets are outside view.")]
        public bool UseOffScreenIndicators = true;

        [Tooltip("Pixels margin from screen edges for off-screen indicators.")]
        [Range(0f, 100f)]
        public float ScreenEdgeMargin = 50f;

        [Tooltip("Flip the off-screen marker vertically (useful if icon points down).")]
        public bool FlipOffScreenMarkerY = false;

        [Header("Distance Scaling")]
        [Tooltip("Enable marker scaling by distance.")]
        public bool EnableDistanceScaling = false;

        [Tooltip("Distance where marker uses DefaultScaleFactor.")]
        public float DistanceForDefaultScale = 50f;

        [Tooltip("Distance beyond which marker reaches MinScaleFactor.")]
        public float MaxScalingDistance = 200f;

        [Tooltip("Minimum scale factor (0 = invisible).")]
        [Range(0f, 1f)]
        public float MinScaleFactor = 0.5f;

        [Tooltip("Default scale factor when near.")]
        [Range(0.1f, 5f)]
        public float DefaultScaleFactor = 1.0f;

        [Header("Distance Text (TMPro)")]
        [Tooltip("Show numeric distance using TextMeshPro.")]
        public bool DisplayDistanceText = false;

        [Tooltip("Unit system for distance display.")]
        public DistanceUnitSystem UnitSystem = DistanceUnitSystem.Metric;

        [Tooltip("Decimal places for distance formatting.")]
        [Range(0, 3)]
        public int DistanceDecimalPlaces = 0;

        [Tooltip("Suffix for meters.")]
        public string SuffixMeters = "m";
        [Tooltip("Suffix for kilometers.")]
        public string SuffixKilometers = "km";
        [Tooltip("Suffix for feet.")]
        public string SuffixFeet = "ft";
        [Tooltip("Suffix for miles.")]
        public string SuffixMiles = "mi";

        // Conversion constants
        public const float METERS_PER_KILOMETER = 1000f;
        public const float FEET_PER_METER = 3.28084f;
        public const float FEET_PER_MILE = 5280f;

        /// <summary>
        /// Return assigned marker prefab and warn if missing.
        /// </summary>
        public GameObject GetMarkerPrefab()
        {
            if (MarkerPrefab == null)
            {
                Debug.LogError("WaypointSettings: MarkerPrefab is not assigned. Please assign one in the Waypoint Settings asset.", this);
            }
            return MarkerPrefab;
        }
    }
}
