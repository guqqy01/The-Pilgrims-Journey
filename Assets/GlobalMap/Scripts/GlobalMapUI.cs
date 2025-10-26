using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Color = UnityEngine.Color;
using AYellowpaper.SerializedCollections;
using System;
using System.Linq;

namespace Map
{
    public class GlobalMapUI : MonoBehaviour
    {
        private GlobalMapGenerator generator;
        public GlobalMapGenerator Generator
        {
            get
            {
                if (generator == null)
                    generator = GlobalMapGenerator.Instance;
                return generator;
            }
        }

        [Header("Settings")]
        [SerializeField] private RectTransform contentTransform;
        [SerializeField] private Vector2 Padding = Vector2.one;
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private bool playAnimationAndUseFirstSeenTrigger;
        public static bool FirstSeen;
        [SerializeField] private GameObject LegendTable;
        [SerializeField] private RectTransform canvasTransform;

        [Header("Map Point UI")]
        [SerializeField] private GlobalMapPointUI mapPointPrefab;
        private readonly List<GlobalMapPointUI> globalMapPoints = new ();
        public float mapPointSize = 3;
        public SerializedDictionary<GlobalMapPointType, GlobalMapPointUISettings> typeAndSettings;
        [SerializeField] private Color takenPointColor;
        [SerializeField] private float minXDistance = 100f;

        [Header("Line UI")]
        [SerializeField] private Transform lineParentTransform;
        [SerializeField] private MapLineUI MapLinePrefab;
        [SerializeField] private float lineColorChangeSpeed = 1;
        private Dictionary<GlobalMapPoint, Dictionary<GlobalMapPoint, MapLineUI>> toPointFromPointLine = new ();
        [SerializeField] private float lineWidth;
        [SerializeField] private Color defaultPathColor;
        [SerializeField] private Color takenPathColor;
        [SerializeField] private float bothDirectionLineOffset = 5f;


        [Header("Arrows")]
        [SerializeField] private Transform arrowsParentTransform;
        [SerializeField] private GlobalMapArrow globalMapArrowPrefab;
        private readonly List<GlobalMapArrow> arrows = new ();
        [SerializeField] private float arrowSize;

        private Vector2 BottomLeft => (Vector2)backgroundRect.position + (new Vector2(backgroundRect.rect.xMin, backgroundRect.rect.yMin) + Padding) * canvasTransform.localScale;
        private Vector2 TopRight => (Vector2)backgroundRect.position + (new Vector2(backgroundRect.rect.xMax, backgroundRect.rect.yMax) - Padding) * canvasTransform.localScale;
        private Vector2 Multiplier
        {
            get
            {
                Vector2 multiplier = (TopRight - BottomLeft) / (Generator.GenerationSettings.MapSize);

                if (multiplier.x < minXDistance * canvasTransform.localScale.x)
                {
                    multiplier.x = minXDistance * canvasTransform.localScale.x;
                }

                return multiplier;
            }
        }
        private Vector3 ModifiedPosition(Vector2 Position) => (Vector3)(Multiplier * (Position + Vector2.one / 2f) + BottomLeft);

        private void Start()
        {
            if (Generator != null)
            {
                Generator.updateUI -= UpdateUI;
                Generator.updateUI += UpdateUI;
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            // Ensure everything is unsubscribed when this object is destroyed
            try
            {
                if (Generator != null)
                    Generator.updateUI -= UpdateUI;
            }
            catch { /* defensive: generator may be destroyed first during shutdown */ }
        }

        private void OnValidate()
        {
            // Editor-time quick checks to fail fast if someone forgets to set references
            if (contentTransform == null) Debug.LogWarning("GlobalMapUI.contentTransform is not assigned.", this);
            if (backgroundRect == null) Debug.LogWarning("GlobalMapUI.backgroundRect is not assigned.", this);
            if (canvasTransform == null) Debug.LogWarning("GlobalMapUI.canvasTransform is not assigned.", this);
            if (mapPointPrefab == null) Debug.LogWarning("GlobalMapUI.mapPointPrefab is not assigned.", this);
            if (MapLinePrefab == null) Debug.LogWarning("GlobalMapUI.MapLinePrefab is not assigned.", this);
            if (lineParentTransform == null) Debug.LogWarning("GlobalMapUI.lineParentTransform is not assigned.", this);
            if (globalMapArrowPrefab == null) Debug.LogWarning("GlobalMapUI.globalMapArrowPrefab is not assigned.", this);
            if (arrowsParentTransform == null) Debug.LogWarning("GlobalMapUI.arrowsParentTransform is not assigned.", this);
        }

        public void ChangeLegendTableActive()
        {
            if (LegendTable != null)
                LegendTable.SetActive(!LegendTable.activeInHierarchy);
        }

        public void UpdateUI()
        {
            // Basic safety checks (fail fast and log to make inspector issues obvious)
            if (backgroundRect == null || canvasTransform == null || contentTransform == null)
            {
                Debug.LogError("GlobalMapUI: backgroundRect, canvasTransform or contentTransform is not assigned.", this);
                return;
            }
            if (mapPointPrefab == null || MapLinePrefab == null || lineParentTransform == null || globalMapArrowPrefab == null || arrowsParentTransform == null)
            {
                Debug.LogError("GlobalMapUI: One or more required prefabs / parents are not assigned in the inspector.", this);
                return;
            }

            // Clear previous UI
            globalMapPoints.DestroyAndClearList();
            arrows.DestroyAndClearList();
            foreach (var dictionary in toPointFromPointLine.Values)
            {
                foreach (var kvp in dictionary)
                {
                    if (kvp.Value != null)
                        Destroy(kvp.Value.gameObject);
                }
            }
            toPointFromPointLine.Clear();

            var gen = Generator;
            if (gen == null || gen.mapPoints == null)
                return;

            // cache commonly used values per update
            float canvasScaleX = canvasTransform.localScale.x;

            GlobalMapPointUI lastPathMapPoint = null;

            // Create new UI for points
            foreach (GlobalMapPoint point in gen.mapPoints)
            {
                if (point == null)
                    continue;

                Vector3 worldPos = ModifiedPosition(point.Position);
                GlobalMapPointUI pointUI = Instantiate(mapPointPrefab, worldPos, Quaternion.identity, backgroundRect);
                pointUI.UpdateUI(point, this);
                globalMapPoints.Add(pointUI);
                SetupLinesMainUI(point, canvasScaleX);
                SetupArrows(point);
                if (gen.takenPath.Contains(point))
                {
                    pointUI.Color = takenPointColor;
                    lastPathMapPoint = pointUI;
                }
            }

            float width = (ModifiedPosition(new Vector2(gen.GenerationSettings.MapSize.x, 0)).x - ModifiedPosition(new Vector2(0, 0)).x) / canvasScaleX + Padding.x;
            contentTransform.sizeDelta = new Vector2(Mathf.Max(width, 490), contentTransform.sizeDelta.y);

            ColorPathLines(playAnimationAndUseFirstSeenTrigger);

            if (playAnimationAndUseFirstSeenTrigger)
            {
                CenterViewPlacement(lastPathMapPoint);
            }
        }

        private void CenterViewPlacement(GlobalMapPointUI pointUI)
        {
            if (pointUI == null)
                return;

            // Get viewport width
            RectTransform viewport = contentTransform.parent as RectTransform;
            if (viewport == null)
                return;

            // Convert world position of the point to local position in content space
            Vector3 localPos = contentTransform.InverseTransformPoint(pointUI.transform.position);

            // Target anchored position to center the point horizontally
            float targetX = -localPos.x + (viewport.rect.width / 2f);

            float minX = - contentTransform.rect.width + viewport.rect.width;

            targetX = Mathf.Clamp(targetX, minX, 0);

            contentTransform.anchoredPosition = new Vector2(targetX, contentTransform.anchoredPosition.y);
        }

        private List<MapLineUI> GetTakenPathLines()
        {
            var linesOnPath = new List<MapLineUI>();
            var path = Generator?.takenPath;
            if (path == null)
                return linesOnPath;

            for (int i = 1; i < path.Count; i++)
            {
                var toPoint = path[i];
                if (!toPointFromPointLine.TryGetValue(toPoint, out var fromDict))
                    continue;

                var prev = path[i - 1];
                if (fromDict.TryGetValue(prev, out var line))
                    linesOnPath.Add(line);
            }

            return linesOnPath;
        }


        public void ColorPathLines(bool animateLastNewLine = false)
        {
            if (Generator == null)
                return;

            var linesOnPath = GetTakenPathLines();
            foreach (var kv in toPointFromPointLine)
            {
                foreach (var line in kv.Value.Values)
                {
                    if (animateLastNewLine && FirstSeen && line == linesOnPath.LastOrDefault() && line.isActiveAndEnabled)
                    {
                        line.PlayColorChange(takenPathColor, defaultPathColor);
                        FirstSeen = false;
                    }
                    else
                    {
                        line.Color = linesOnPath.Contains(line)
                            ? takenPathColor
                            : defaultPathColor;
                    }
                }
            }
        }

        private void SetupLinesMainUI(GlobalMapPoint mapPoint, float canvasScaleX)
        {
            foreach (GlobalMapPoint toMapPoint in mapPoint.connectedToPoints.Values)
            {
                Vector2 fromPos = ModifiedPosition(mapPoint.Position);
                Vector2 toPos = ModifiedPosition(toMapPoint.Position);

                if (toMapPoint.HasPointsConnectedBothWays(mapPoint))
                {
                    Vector2 direction = toPos - fromPos;

                    Vector2 offsetDirection = bothDirectionLineOffset * direction.RotateClockwise(90f * Mathf.Deg2Rad).normalized;

                    fromPos += offsetDirection;
                    toPos += offsetDirection;
                }

                MapLineUI line = Instantiate(MapLinePrefab, lineParentTransform);
                if (toPointFromPointLine.TryGetValue(toMapPoint, out var dict))
                {
                    dict[mapPoint] = line;
                }
                else
                {
                    toPointFromPointLine[toMapPoint] = new Dictionary<GlobalMapPoint, MapLineUI>() { { mapPoint, line } };
                }

                line.Setup(fromPos, toPos, lineWidth, canvasScaleX);
            }
        }

        private void SetupArrows(GlobalMapPoint mapPoint)
        {
            foreach (GlobalMapPoint toMapPoint in mapPoint.connectedToPoints.Values)
            {
                Vector2 fromPos = ModifiedPosition(mapPoint.Position);
                Vector2 toPos = ModifiedPosition(toMapPoint.Position);
                Vector2 dir = toPos - fromPos;
                if (dir.sqrMagnitude <= Mathf.Epsilon)
                    continue;

                if (toMapPoint.HasPointsConnectedBothWays(mapPoint))
                {
                    Vector2 direction = toPos - fromPos;

                    Vector2 offsetDirection = bothDirectionLineOffset * direction.RotateClockwise(90f * Mathf.Deg2Rad).normalized;

                    fromPos -= offsetDirection;
                    toPos -= offsetDirection;
                }

                // angle in degrees (0 = +X)
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                Vector2 middle = (toPos + fromPos) * 0.5f;

               

                // Instantiate as child first (keeps hierarchy correct)
                GlobalMapArrow arrow = Instantiate(globalMapArrowPrefab, arrowsParentTransform);

                // If this arrow is a UI element (RectTransform), place using local coordinates
                var rt = arrow.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Convert world-space middle into backgroundRect local space
                    Vector3 localPos = backgroundRect.InverseTransformPoint(middle);

                    // anchoredPosition expects Vector2; localPos uses same units if parent is the backgroundRect
                    rt.anchoredPosition = new Vector2(localPos.x, localPos.y);

                    // Apply rotation around Z: rotate 90° clockwise (subtract 90)
                    rt.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
                }
                else
                {
                    // Non-UI world object: set world position/rotation, 90° clockwise adjustment
                    arrow.transform.position = middle;
                    arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
                }

                arrow.Size = arrowSize;
                arrows.Add(arrow);
            }
        }
    }
}
