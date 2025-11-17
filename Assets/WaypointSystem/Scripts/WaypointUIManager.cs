using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace WrightAngle.Waypoint
{
    /// <summary>
    /// Core manager for the waypoint UI system. Place one instance in the scene.
    /// Discovers WaypointTargets, manages a pool of marker UI elements and updates them.
    /// </summary>
    [AddComponentMenu("WrightAngle/Waypoint UI Manager")]
    [DisallowMultipleComponent]
    public class WaypointUIManager : MonoBehaviour
    {
        [Header("Essential References")]
        [Tooltip("WaypointSettings asset that configures this system.")]
        [SerializeField] private WaypointSettings settings;

        [Tooltip("Primary camera used for screen projection.")]
        [SerializeField] private Camera waypointCamera;

        [Tooltip("RectTransform parent inside the Canvas where markers will be instantiated.")]
        [SerializeField] private RectTransform markerParentCanvas;

        // internal state
        private ObjectPool<WaypointMarkerUI> markerPool;
        private List<WaypointTarget> activeTargetList = new List<WaypointTarget>();
        private HashSet<WaypointTarget> activeTargetSet = new HashSet<WaypointTarget>();
        private Dictionary<WaypointTarget, WaypointMarkerUI> activeMarkers = new Dictionary<WaypointTarget, WaypointMarkerUI>();

        private Camera _cachedWaypointCamera;
        private float lastUpdateTime = -1f;
        private bool isInitialized = false;

        private void Awake()
        {
            bool setupError = ValidateSetup();
            if (setupError)
            {
                enabled = false;
                Debug.LogError($"[{gameObject.name}] WaypointUIManager disabled due to setup errors.", this);
                return;
            }

            _cachedWaypointCamera = waypointCamera;
            InitializePool();

            WaypointTarget.OnTargetEnabled += HandleTargetEnabled;
            WaypointTarget.OnTargetDisabled += HandleTargetDisabled;

            isInitialized = true;
        }

        private void Start()
        {
            if (!isInitialized) return;
            FindAndRegisterInitialTargets();
        }

        private void OnDestroy()
        {
            WaypointTarget.OnTargetEnabled -= HandleTargetEnabled;
            WaypointTarget.OnTargetDisabled -= HandleTargetDisabled;

            markerPool?.Clear();
            markerPool?.Dispose();
            activeTargetList.Clear();
            activeTargetSet.Clear();
            activeMarkers.Clear();
        }

        private bool ValidateSetup()
        {
            bool error = false;

            if (waypointCamera == null) { Debug.LogError("WaypointUIManager Error: waypointCamera not assigned!", this); error = true; }
            if (settings == null) { Debug.LogError("WaypointUIManager Error: settings not assigned!", this); error = true; }
            else if (settings.GetMarkerPrefab() == null) { Debug.LogError($"WaypointUIManager Error: Marker prefab missing in settings '{settings.name}'!", this); error = true; }
            if (markerParentCanvas == null) { Debug.LogError("WaypointUIManager Error: markerParentCanvas not assigned!", this); error = true; }
            else if (markerParentCanvas.GetComponentInParent<Canvas>() == null) { Debug.LogError("WaypointUIManager Error: markerParentCanvas must be a child of a Canvas!", this); error = true; }

            return error;
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (Time.time < lastUpdateTime + settings.UpdateFrequency) return;
            lastUpdateTime = Time.time;

            Vector3 cameraPosition = _cachedWaypointCamera.transform.position;
            float camPixelWidth = _cachedWaypointCamera.pixelWidth;
            float camPixelHeight = _cachedWaypointCamera.pixelHeight;

            for (int i = activeTargetList.Count - 1; i >= 0; i--)
            {
                WaypointTarget target = activeTargetList[i];

                if (target == null || !target.gameObject.activeInHierarchy)
                {
                    RemoveTargetCompletely(target, i);
                    continue;
                }

                Transform targetTransform = target.transform;
                Vector3 targetWorldPos = targetTransform.position;

                float distanceToTarget = CalculateDistance(cameraPosition, targetWorldPos);

                if (distanceToTarget > settings.MaxVisibleDistance)
                {
                    TryReleaseMarker(target);
                    continue;
                }

                Vector3 screenPos = _cachedWaypointCamera.WorldToScreenPoint(targetWorldPos);
                bool isBehindCamera = screenPos.z <= 0f;
                bool isOnScreen = !isBehindCamera && screenPos.x > 0f && screenPos.x < camPixelWidth && screenPos.y > 0f && screenPos.y < camPixelHeight;

                bool shouldShowMarker = isOnScreen || (settings.UseOffScreenIndicators && !isOnScreen);

                if (shouldShowMarker)
                {
                    if (!activeMarkers.TryGetValue(target, out WaypointMarkerUI markerInstance))
                    {
                        markerInstance = markerPool.Get();
                        activeMarkers.Add(target, markerInstance);
                    }

                    if (!markerInstance.gameObject.activeSelf) markerInstance.gameObject.SetActive(true);

                    markerInstance.UpdateDisplay(screenPos, isOnScreen, isBehindCamera, _cachedWaypointCamera, settings, distanceToTarget);
                }
                else
                {
                    TryReleaseMarker(target);
                }
            }
        }

        private float CalculateDistance(Vector3 camPos, Vector3 targetPos)
        {
            if (settings.GameMode == WaypointSettings.ProjectionMode.Mode2D && settings.IgnoreZAxisForDistance2D)
            {
                return Vector2.Distance(new Vector2(camPos.x, camPos.y), new Vector2(targetPos.x, targetPos.y));
            }
            return Vector3.Distance(camPos, targetPos);
        }

        private void FindAndRegisterInitialTargets()
        {
#if UNITY_2023_1_OR_NEWER
            WaypointTarget[] allTargets = FindObjectsByType<WaypointTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            WaypointTarget[] allTargets = FindObjectsOfType<WaypointTarget>(true);
#endif
            int activationCount = 0;
            foreach (WaypointTarget target in allTargets)
            {
                if (target.ActivateOnStart && target.gameObject.activeInHierarchy)
                {
                    RegisterTarget(target);
                    activationCount++;
                }
                else
                {
                    // Keep non-intrusive log for debugging activation state
                    Debug.Log($"[{gameObject.name}] WaypointUIManager: Target '{target.gameObject.name}' has ActivateOnStart={target.ActivateOnStart} and activeInHierarchy={target.gameObject.activeInHierarchy}.", target.gameObject);
                }
            }
            Debug.Log($"[{gameObject.name}] WaypointUIManager: Found {allTargets.Length} targets, activated {activationCount}.");
        }

        private void RegisterTarget(WaypointTarget target)
        {
            if (target != null && activeTargetSet.Add(target))
            {
                activeTargetList.Add(target);
            }
        }

        private void TryReleaseMarker(WaypointTarget target)
        {
            if (target != null && activeMarkers.TryGetValue(target, out WaypointMarkerUI markerToRelease))
            {
                markerPool.Release(markerToRelease);
                activeMarkers.Remove(target);
            }
        }

        private void RemoveTargetCompletely(WaypointTarget target, int listIndex = -1)
        {
            TryReleaseMarker(target);

            if (target != null) activeTargetSet.Remove(target);

            if (listIndex >= 0 && listIndex < activeTargetList.Count && activeTargetList[listIndex] == target)
            {
                activeTargetList.RemoveAt(listIndex);
            }
            else if (target != null)
            {
                activeTargetList.Remove(target);
            }
            else
            {
                activeTargetList.RemoveAll(item => item == null);
            }
        }

        private void InitializePool()
        {
            GameObject prefab = settings.GetMarkerPrefab();
            if (prefab == null) return;

            markerPool = new ObjectPool<WaypointMarkerUI>(
                createFunc: () =>
                {
                    GameObject go = Instantiate(prefab, markerParentCanvas);
                    WaypointMarkerUI ui = go.GetComponent<WaypointMarkerUI>();
                    if (ui == null)
                    {
                        ui = go.AddComponent<WaypointMarkerUI>();
                        Debug.LogWarning($"WaypointUIManager: Added missing WaypointMarkerUI to instantiated prefab '{prefab.name}'.", go);
                    }
                    go.SetActive(false);
                    return ui;
                },
                actionOnGet: (marker) => marker.gameObject.SetActive(true),
                actionOnRelease: (marker) => marker.gameObject.SetActive(false),
                actionOnDestroy: (marker) => { if (marker != null) Destroy(marker.gameObject); },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 100
            );
        }

        private void HandleTargetEnabled(WaypointTarget target) => RegisterTarget(target);

        private void HandleTargetDisabled(WaypointTarget target)
        {
            int index = activeTargetList.IndexOf(target);
            RemoveTargetCompletely(target, index);
        }
    }
}
