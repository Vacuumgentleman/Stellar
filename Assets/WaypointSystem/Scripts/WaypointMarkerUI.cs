using UnityEngine;
using UnityEngine.UI; // Requerido para el componente Image
using TMPro; // Requerido para TextMeshProUGUI

namespace WrightAngle.Waypoint
{
    /// <summary>
    /// Controla el estado visual de un marcador de waypoint individual en el Canvas de la UI.
    /// Este script debe ir en el prefab del marcador. Maneja su posicionamiento en pantalla,
    /// lo fija a los bordes si está fuera de la vista, lo rota para apuntar al objetivo,
    /// y ajusta su escala y texto de distancia según la configuración.
    /// </summary>
    [AddComponentMenu("WrightAngle/Waypoint Marker UI")]
    [RequireComponent(typeof(RectTransform))]
    public class WaypointMarkerUI : MonoBehaviour
    {
        [Header("Referencias de elementos UI")]
        [Tooltip("Elemento visual principal del marcador (flecha, punto, ícono, etc). Debe tener un componente Image.")]
        [SerializeField] private Image markerIcon;

        [Tooltip("Elemento TextMeshProUGUI para mostrar la distancia. Opcional.")]
        [SerializeField] private TextMeshProUGUI distanceTextElement;

        // Componentes cacheados
        private RectTransform rectTransform;
        private Vector3 initialScale; // Escala original del marcador (desde el prefab)
        private Quaternion initialTextRotation = Quaternion.identity; // Rotación inicial para mantener texto vertical

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            initialScale = rectTransform.localScale;

            // Validación del ícono (requerido para visibilidad)
            if (markerIcon != null)
            {
                markerIcon.raycastTarget = false;
            }

            if (distanceTextElement != null)
            {
                distanceTextElement.raycastTarget = false;
            }
        }

        /// <summary>
        /// Actualiza posición, rotación, escala y texto de distancia del marcador.
        /// Llamado frecuentemente por el WaypointUIManager.
        /// </summary>
        public void UpdateDisplay(Vector3 screenPosition, bool isOnScreen, bool isBehindCamera, Camera cam, WaypointSettings settings, float distanceToTarget)
        {
            if (settings == null || rectTransform == null || cam == null)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }

            bool isMarkerVisible = ApplyDistanceScaling(settings, distanceToTarget, isOnScreen);

            if (!isMarkerVisible && settings.MinScaleFactor == 0f && settings.EnableDistanceScaling)
            {
                if (distanceTextElement != null) distanceTextElement.gameObject.SetActive(false);
            }

            if (isOnScreen)
            {
                rectTransform.position = screenPosition;
                rectTransform.rotation = Quaternion.identity;

                if (markerIcon != null && !markerIcon.gameObject.activeSelf && isMarkerVisible)
                    markerIcon.gameObject.SetActive(true);
            }
            else
            {
                if (!settings.UseOffScreenIndicators)
                {
                    if (markerIcon != null && markerIcon.gameObject.activeSelf) markerIcon.gameObject.SetActive(false);
                    if (distanceTextElement != null && distanceTextElement.gameObject.activeSelf) distanceTextElement.gameObject.SetActive(false);
                    return;
                }

                if (markerIcon != null && !markerIcon.gameObject.activeSelf && isMarkerVisible)
                    markerIcon.gameObject.SetActive(true);

                float margin = settings.ScreenEdgeMargin;
                Vector2 screenCenter = new Vector2(cam.pixelWidth * 0.5f, cam.pixelHeight * 0.5f);
                Rect screenBounds = new Rect(margin, margin, cam.pixelWidth - (margin * 2f), cam.pixelHeight - (margin * 2f));
                Vector3 positionToClamp;
                Vector2 directionForRotation;

                if (isBehindCamera)
                {
                    Vector2 screenPos2D = new Vector2(screenPosition.x, screenPosition.y);
                    Vector2 directionFromCenter = screenPos2D - screenCenter;
                    directionFromCenter.x *= -1;
                    directionFromCenter.y = -Mathf.Abs(directionFromCenter.y);
                    if (directionFromCenter.sqrMagnitude < 0.001f) directionFromCenter = Vector2.down;
                    directionFromCenter.Normalize();
                    float farDistance = cam.pixelWidth + cam.pixelHeight;
                    positionToClamp = new Vector3(screenCenter.x + directionFromCenter.x * farDistance, screenCenter.y + directionFromCenter.y * farDistance, 0);
                    directionForRotation = directionFromCenter;
                }
                else
                {
                    positionToClamp = screenPosition;
                    directionForRotation = (new Vector2(screenPosition.x, screenPosition.y) - screenCenter).normalized;
                }

                Vector2 clampedPosition = IntersectWithScreenBounds(screenCenter, positionToClamp, screenBounds);
                rectTransform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0f);

                if (markerIcon != null && directionForRotation.sqrMagnitude > 0.001f)
                {
                    float angle = Vector2.SignedAngle(Vector2.right, directionForRotation);
                    float flipAngle = settings.FlipOffScreenMarkerY ? 180f : 0f;
                    rectTransform.rotation = Quaternion.Euler(0, 0, angle + flipAngle - 90f);
                }
                else if (markerIcon != null)
                {
                    float flipAngle = settings.FlipOffScreenMarkerY ? 180f : 0f;
                    rectTransform.rotation = Quaternion.Euler(0, 0, -180f + flipAngle);
                }
            }

            UpdateDistanceText(settings, distanceToTarget, isMarkerVisible);
        }

        /// <summary>
        /// Aplica escalado por distancia si está habilitado en la configuración.
        /// Retorna true si el marcador debe ser visible, false si no.
        /// </summary>
        private bool ApplyDistanceScaling(WaypointSettings settings, float distanceToTarget, bool isOnScreen)
        {
            float currentVisualScaleFactor = settings.DefaultScaleFactor;

            if (settings.EnableDistanceScaling)
            {
                if (distanceToTarget <= settings.DistanceForDefaultScale)
                {
                    currentVisualScaleFactor = settings.DefaultScaleFactor;
                }
                else if (distanceToTarget >= settings.MaxScalingDistance)
                {
                    currentVisualScaleFactor = settings.MinScaleFactor;
                }
                else
                {
                    float t = (distanceToTarget - settings.DistanceForDefaultScale) / (settings.MaxScalingDistance - settings.DistanceForDefaultScale);
                    currentVisualScaleFactor = Mathf.Lerp(settings.DefaultScaleFactor, settings.MinScaleFactor, t);
                }
                rectTransform.localScale = initialScale * currentVisualScaleFactor;
            }
            else
            {
                rectTransform.localScale = initialScale * settings.DefaultScaleFactor;
                currentVisualScaleFactor = settings.DefaultScaleFactor;
            }

            bool shouldBeVisible = currentVisualScaleFactor > 0.001f || (settings.EnableDistanceScaling && settings.MinScaleFactor > 0f) || !settings.EnableDistanceScaling;

            if (markerIcon != null)
            {
                markerIcon.enabled = shouldBeVisible;
            }

            return shouldBeVisible;
        }

        /// <summary>
        /// Actualiza el texto de distancia según configuración y visibilidad del marcador.
        /// </summary>
        private void UpdateDistanceText(WaypointSettings settings, float distanceToTarget, bool isMarkerVisuallyScaledToShow)
        {
            if (distanceTextElement == null) return;

            if (settings.DisplayDistanceText && isMarkerVisuallyScaledToShow)
            {
                distanceTextElement.gameObject.SetActive(true);

                string distanceString;
                string suffix;

                if (settings.UnitSystem == WaypointSettings.DistanceUnitSystem.Metric)
                {
                    if (distanceToTarget < WaypointSettings.METERS_PER_KILOMETER)
                    {
                        distanceString = distanceToTarget.ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixMeters;
                    }
                    else
                    {
                        distanceString = (distanceToTarget / WaypointSettings.METERS_PER_KILOMETER).ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixKilometers;
                    }
                }
                else
                {
                    float distanceInFeet = distanceToTarget * WaypointSettings.FEET_PER_METER;
                    if (distanceInFeet < WaypointSettings.FEET_PER_MILE)
                    {
                        distanceString = distanceInFeet.ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixFeet;
                    }
                    else
                    {
                        distanceString = (distanceInFeet / WaypointSettings.FEET_PER_MILE).ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixMiles;
                    }
                }

                distanceTextElement.text = $"{distanceString}{suffix}";
                distanceTextElement.rectTransform.rotation = Quaternion.identity;
            }
            else
            {
                distanceTextElement.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Calcula la intersección de una línea desde el centro de pantalla hacia un punto, con los bordes de la pantalla.
        /// Se usa para mantener el marcador dentro de los márgenes visibles.
        /// </summary>
        private Vector2 IntersectWithScreenBounds(Vector2 center, Vector2 targetPoint, Rect bounds)
        {
            Vector2 direction = (targetPoint - center).normalized;
            if (direction.sqrMagnitude < 0.0001f) return new Vector2(bounds.center.x, bounds.yMin);

            float tXMin = (direction.x != 0) ? (bounds.xMin - center.x) / direction.x : Mathf.Infinity;
            float tXMax = (direction.x != 0) ? (bounds.xMax - center.x) / direction.x : Mathf.Infinity;
            float tYMin = (direction.y != 0) ? (bounds.yMin - center.y) / direction.y : Mathf.Infinity;
            float tYMax = (direction.y != 0) ? (bounds.yMax - center.y) / direction.y : Mathf.Infinity;

            float minT = Mathf.Infinity;
            if (tXMin > 0 && center.y + tXMin * direction.y >= bounds.yMin && center.y + tXMin * direction.y <= bounds.yMax) minT = Mathf.Min(minT, tXMin);
            if (tXMax > 0 && center.y + tXMax * direction.y >= bounds.yMin && center.y + tXMax * direction.y <= bounds.yMax) minT = Mathf.Min(minT, tXMax);
            if (tYMin > 0 && center.x + tYMin * direction.x >= bounds.xMin && center.x + tYMin * direction.x <= bounds.xMax) minT = Mathf.Min(minT, tYMin);
            if (tYMax > 0 && center.x + tYMax * direction.x >= bounds.xMin && center.x + tYMax * direction.x <= bounds.xMax) minT = Mathf.Min(minT, tYMax);

            if (float.IsInfinity(minT))
            {
                return new Vector2(
                    Mathf.Clamp(targetPoint.x, bounds.xMin, bounds.xMax),
                    Mathf.Clamp(targetPoint.y, bounds.yMin, bounds.yMax)
                );
            }

            return center + direction * minT;
        }

    } // Fin de la clase
} // Fin del namespace
