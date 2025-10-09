using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using AYellowpaper.SerializedCollections;
using UnityEngine.Rendering.Universal;
using System;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using System.Security.Cryptography;
namespace Map
{
    public class GlobalMapUI : MonoBehaviour
    {
        private GlobalMapGenerator generator;
        public GlobalMapGenerator Generator
        {
            get
            {
                if(generator == null)
                    generator = GlobalMapGenerator.Instance;

                return generator;
            }
        }

        [Header("Settings")]

        [SerializeField]
        private RectTransform contentTransform;
        [SerializeField]
        private Vector2 Padding = Vector2.one;

        [SerializeField]
        private RectTransform backgroundRect;
        [SerializeField]
        private bool playAnimationAndUseFirstSeenTrigger;
        public static bool FirstSeen;

        [SerializeField]
        private GameObject LegendTable;

        [SerializeField]
        private RectTransform canvasTransform;

        [Header("Map Point UI")]
        [SerializeField]
        private GlobalMapPointUI mapPointPrefab;

        private readonly List<GlobalMapPointUI> globalMapPoints = new ();

        public float mapPointSize = 3;
        public SerializedDictionary<GlobalMapPointType, GlobalMapPointUISettings> typeAndSettings;
        [SerializeField]
        private Color takenPointColor;
        [SerializeField]
        private float minXDistance = 100f;

        [Header("Line UI")]
        [SerializeField]
        private Transform lineParentTransform;
        [SerializeField]
        private MapLineUI MapLinePrefab;
        [SerializeField]
        private float lineColorChangeSpeed = 1;
        private Dictionary<GlobalMapPoint, Dictionary<GlobalMapPoint, MapLineUI>> toPointFromPointLine = new ();
        [SerializeField]
        private float lineWidth;
        [SerializeField]
        private Color defaultPathColor;
        [SerializeField]
        private Color takenPathColor;

        [Header("Arrows")]
        [SerializeField]
        private Transform arrowsParentTransform;
        [SerializeField]
        private GlobalMapArrow globalMapArrowPrefab;
        private readonly List<GlobalMapArrow> arrows = new ();
        [SerializeField]
        private float arrowSize;

        

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
        private Vector3 ModifiedPosition(Vector2 Position) => Multiplier * (Position + Vector2.one / 2f) + BottomLeft;

        private void Start()
        {
            GlobalMapGenerator.OnMapGenerated += UpdateUI;
        }

        private void OnEnable()
        {
            if (Generator != null)
            {
                Generator.updateUI += UpdateUI;
                UpdateUI();
            }
        }

        private void OnDisable()
        {
            if (Generator != null)
            {
                Generator.updateUI -= UpdateUI;
            }
        }

        public void ChangeLegendTableActive()
        {
            LegendTable.SetActive(!LegendTable.activeInHierarchy);
        }

        public void UpdateUI()
        {
            globalMapPoints.DestroyAndClearList();
            arrows.DestroyAndClearList();
            foreach(Dictionary<GlobalMapPoint, MapLineUI> dictionary in toPointFromPointLine.Values)
            {
                foreach(KeyValuePair<GlobalMapPoint, MapLineUI> kvp in dictionary)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            toPointFromPointLine = new Dictionary<GlobalMapPoint, Dictionary<GlobalMapPoint, MapLineUI>>();

            if (Generator == null)
                return;
            
            foreach (GlobalMapPoint point in Generator.mapPoints)
            {
                if (point == null)
                    continue;

                GlobalMapPointUI pointUI = Instantiate(mapPointPrefab, ModifiedPosition(point.Position), Quaternion.identity, backgroundRect);
                pointUI.UpdateUI(point, this);
                globalMapPoints.Add(pointUI);
                SetupLinesMainUI(point);
                SetupArrows(point);

                if (Generator.takenPath.Contains(point))
                {
                    pointUI.Color = takenPointColor;
                }
            }

            float width = (ModifiedPosition(new Vector2(Generator.GenerationSettings.MapSize.x, 0)).x - ModifiedPosition(new Vector2(0, 0)).x) / canvasTransform.localScale.x + Padding.x;
            contentTransform.sizeDelta = new Vector2(Mathf.Max(width, 490), contentTransform.sizeDelta.y);


            if (playAnimationAndUseFirstSeenTrigger)
                ColorLinesScene();
            else
                ColorLines();
        }

        private void CenterViewPlacement()
        {

        }

        private void ColorLines()
        {
            List<MapLineUI> linesOnPath = new List<MapLineUI>();

            for (int i = 1; i < Generator.takenPath.Count; i++)
            {
                GlobalMapPoint point = Generator.takenPath[i];

                if (!toPointFromPointLine.ContainsKey(point))
                    continue;

                Dictionary<GlobalMapPoint, MapLineUI> fromPointsAndTheirLineToThePoint = toPointFromPointLine[point];
                foreach (KeyValuePair<GlobalMapPoint, MapLineUI> fromPointAndItsLine in fromPointsAndTheirLineToThePoint)
                {
                    GlobalMapPoint fromPoint = fromPointAndItsLine.Key;
                    MapLineUI lineToPoint = fromPointAndItsLine.Value;

                    if (Generator.takenPath[i - 1] == fromPoint)
                    {
                        linesOnPath.Add(lineToPoint);
                    }
                }
            }

            foreach (KeyValuePair<GlobalMapPoint, Dictionary<GlobalMapPoint, MapLineUI>> toPointAndItsfromPointsAndLines in toPointFromPointLine)
            {
                GlobalMapPoint toPoint = toPointAndItsfromPointsAndLines.Key;

                foreach (KeyValuePair<GlobalMapPoint, MapLineUI> fromPointAndLine in toPointAndItsfromPointsAndLines.Value)
                {
                    MapLineUI line = fromPointAndLine.Value;
                    Color lineColor = linesOnPath.Contains(line) ? takenPathColor : defaultPathColor;
                    line.Color = lineColor;
                }
            }
        }

        private void ColorLinesScene()
        {
            List<MapLineUI> linesOnPath = new List<MapLineUI>();

            for (int i = 1; i < Generator.takenPath.Count; i++)
            {
                GlobalMapPoint point = Generator.takenPath[i];

                if (!toPointFromPointLine.ContainsKey(point))
                    continue;

                Dictionary<GlobalMapPoint, MapLineUI> fromPointsAndTheirLineToThePoint = toPointFromPointLine[point];
                foreach (KeyValuePair<GlobalMapPoint, MapLineUI> fromPointAndItsLine in fromPointsAndTheirLineToThePoint)
                {
                    GlobalMapPoint fromPoint = fromPointAndItsLine.Key;
                    MapLineUI lineToPoint = fromPointAndItsLine.Value;

                    if (Generator.takenPath[i - 1] == fromPoint)
                    {
                        linesOnPath.Add(lineToPoint);
                    }
                }
            }

            if (linesOnPath.Count <= 0)
                return;

            MapLineUI lastLine = linesOnPath[linesOnPath.Count - 1];

            foreach (KeyValuePair<GlobalMapPoint, Dictionary<GlobalMapPoint, MapLineUI>> toPointAndItsfromPointsAndLines in toPointFromPointLine)
            {
                GlobalMapPoint toPoint = toPointAndItsfromPointsAndLines.Key;

                foreach (KeyValuePair<GlobalMapPoint, MapLineUI> fromPointAndLine in toPointAndItsfromPointsAndLines.Value)
                {
                    GlobalMapPoint fromPoint = fromPointAndLine.Key;
                    MapLineUI line = fromPointAndLine.Value;

                    if (lastLine == line && FirstSeen)
                    {
                        line.PlayColorChange(takenPathColor);
                        FirstSeen = false;
                    }
                    else
                    {
                        Color lineColor = linesOnPath.Contains(line) ? takenPathColor : defaultPathColor;
                        line.Color = lineColor;
                    }
                }
            }
        }

        private void SetupLinesMainUI(GlobalMapPoint mapPoint)
        {
            foreach (GlobalMapPoint toMapPoint in mapPoint.connectedToPoints.Values)
            {
                Vector2 fromPos = ModifiedPosition(mapPoint.Position);
                Vector2 toPos = ModifiedPosition(toMapPoint.Position);

                if (toMapPoint.HasPointsConnectedBothWays(mapPoint))
                {
                    Vector2 direction = toPos - fromPos;

                    toPos = fromPos + direction / 2f;
                }

                MapLineUI line = Instantiate(MapLinePrefab, transform);

                if (toPointFromPointLine.ContainsKey(toMapPoint))
                {
                    toPointFromPointLine[toMapPoint].Add(mapPoint, line);
                }
                else
                {
                    toPointFromPointLine.Add(toMapPoint, new Dictionary<GlobalMapPoint, MapLineUI>()
                    {
                        { mapPoint, line }
                    });
                }

                line.Setup(fromPos, toPos, lineWidth, canvasTransform.localScale.x);
                line.transform.SetParent(lineParentTransform);
            }
        }

        private void SetupArrows(GlobalMapPoint mapPoint)
        {
            foreach(GlobalMapPoint toMapPoint in mapPoint.connectedToPoints.Values)
            {
                Vector2 fromPos = ModifiedPosition(mapPoint.Position);
                Vector2 toPos = ModifiedPosition(toMapPoint.Position);
                Vector2 direction = (toPos - fromPos).normalized;

                Vector2 middle = toPos.Middle(fromPos);

                GlobalMapArrow arrow = Instantiate(globalMapArrowPrefab, middle, Quaternion.LookRotation(Vector3.forward, toPos - fromPos), arrowsParentTransform);
                arrow.Size = arrowSize;
                arrows.Add(arrow); 
            }
        }
    }
}
