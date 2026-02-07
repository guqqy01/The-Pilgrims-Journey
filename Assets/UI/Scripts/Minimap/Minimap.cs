using Heros;
using LocalMap;
using Monsters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Minimap
{
    public class Minimap : MonoBehaviour
    {
        [SerializeField] private Image pointPrefab;
        [SerializeField] private Sprite heroPointSprite;
        [SerializeField] private Sprite monsterPointSprite;
        [SerializeField] private Sprite unitPointSprite;
        [SerializeField] private RectTransform cameraDisplayRect;
        [SerializeField] private RectTransform minimapTransform;

        private Camera cachedCam;

        private Image heroPoint;

        private readonly Dictionary<Transform, MinimapEntity> monsterEntities = new();
        private readonly Dictionary<Transform, MinimapEntity> unitEntities = new();

        private readonly Stack<Image> pointPool = new();

        private float minX, maxX, minY, maxY;
        private float halfWidth, halfHeight;

        private Vector2 heroTargetPos;

        private readonly Dictionary<Image, Vector2> monsterTargets = new();
        private readonly Dictionary<Image, Vector2> unitTargets = new();

        private Vector2 cameraTargetPos;
        private Vector2 cameraTargetSize;

        private static readonly List<Transform> _deadBuffer = new();

        private float updateTimer;
        private const float UPDATE_INTERVAL = 0.1f; // 10 FPS

        private void RemoveDeadEntries(Dictionary<Transform, Image> map)
        {
            _deadBuffer.Clear();

            foreach (var kvp in map)
            {
                if (kvp.Key == null)
                    _deadBuffer.Add(kvp.Key);
            }

            for (int i = 0; i < _deadBuffer.Count; i++)
            {
                var img = map[_deadBuffer[i]];
                ReturnPoint(img);
                map.Remove(_deadBuffer[i]);
            }
        }

        private void Awake()
        {
            cachedCam = Camera.main;
        }

        private void Start()
        {
            CacheMapBounds();
        }

        private void Update()
        {
            // Always smooth visuals
            SmoothVisuals();

            // Only compute targets periodically
            updateTimer += Time.unscaledDeltaTime;
            if (updateTimer < UPDATE_INTERVAL)
                return;

            updateTimer = 0f;

            UpdateTargets();
        }

        private void UpdateTargets()
        {
            CalculateHeroTarget();

            UpdateEntityTargets(
                MonsterSpawnManager.Instance?.Monsters,
                monsterEntities,
                monsterPointSprite
);

            UpdateEntityTargets(
                UnitSpawningManager.Instance?.SpawnedUnits,
                unitEntities,
                unitPointSprite
            );

            CalculateCameraTargets();
        }

        private void UpdateEntityTargets<T>(
            IReadOnlyList<T> entities,
            Dictionary<Transform, MinimapEntity> entityMap,
            Sprite sprite
        ) where T : MonoBehaviour
        {
            if (entities == null)
                return;

            // Disable all first
            foreach (var kvp in entityMap)
            {
                if (kvp.Value.image != null)
                    kvp.Value.image.enabled = false;
            }

            for (int i = 0; i < entities.Count; i++)
            {
                Transform t = entities[i].transform;

                if (!entityMap.TryGetValue(t, out var entity))
                {
                    var img = GetPoint();
                    img.sprite = sprite;
                    img.NativeRatioScaleHeightToSize(20f);

                    entity = new MinimapEntity
                    {
                        transform = t,
                        image = img,
                        previousPos = img.rectTransform.anchoredPosition
                    };

                    entityMap.Add(t, entity);
                }

                entity.image.enabled = true;

                entity.previousPos = entity.targetPos;
                entity.targetPos = GetPositionOnMinimap(t.position);
                entity.lastWorldPos = t.position;
            }

            RemoveDeadEntities(entityMap);
        }

        private void RemoveDeadEntities(Dictionary<Transform, MinimapEntity> map)
        {
            _deadBuffer.Clear();

            foreach (var kvp in map)
            {
                if (kvp.Key == null)
                    _deadBuffer.Add(kvp.Key);
            }

            for (int i = 0; i < _deadBuffer.Count; i++)
            {
                var entity = map[_deadBuffer[i]];
                ReturnPoint(entity.image);
                map.Remove(_deadBuffer[i]);
            }
        }

        private void SmoothEntityMap(Dictionary<Transform, MinimapEntity> entities, float t)
        {
            foreach (var kvp in entities)
            {
                var entity = kvp.Value;

                if (entity.image == null)
                    continue;

                var rt = entity.image.rectTransform;

                rt.anchoredPosition =
                    Vector2.Lerp(rt.anchoredPosition, entity.targetPos, t);
            }
        }

        private void CalculateHeroTarget()
        {
            var hero = HeroManager.Instance?.ActiveHero;

            if (hero == null)
            {
                if (heroPoint != null)
                {
                    ReturnPoint(heroPoint);
                    heroPoint = null;
                }
                return;
            }

            if (heroPoint == null)
            {
                heroPoint = GetPoint();
                heroPoint.sprite = heroPointSprite;
                heroPoint.NativeRatioScaleHeightToSize(20f);
            }

            heroTargetPos = GetPositionOnMinimap(hero.transform.position);
        }

        private void SmoothVisuals()
        {
            float t = 12f * Time.unscaledDeltaTime; // smoothing speed

            if (heroPoint != null)
            {
                heroPoint.rectTransform.anchoredPosition =
                    Vector2.Lerp(heroPoint.rectTransform.anchoredPosition, heroTargetPos, t);
            }

            SmoothEntityMap(monsterEntities, t);
            SmoothEntityMap(unitEntities, t);

            cameraDisplayRect.anchoredPosition =
                Vector2.Lerp(cameraDisplayRect.anchoredPosition, cameraTargetPos, t);

            cameraDisplayRect.sizeDelta =
                Vector2.Lerp(cameraDisplayRect.sizeDelta, cameraTargetSize, t);
        }

        private void CacheMapBounds()
        {
            if (LocalMapGenerator.Instance == null)
            {
                Debug.Log("LocalMapGenerator.Instance is null");
                return;
            }

            minX = -0.5f;
            maxX = LocalMapGenerator.Instance.MapSizeX - 0.5f;
            minY = -0.5f;
            maxY = LocalMapGenerator.Instance.MapSizeY - 0.5f;


            halfWidth = minimapTransform.rect.width * 0.5f;
            halfHeight = minimapTransform.rect.height * 0.5f;
        }

        private float GetVerticalWorldPadding(float pixelPadding)
        {
            if (cachedCam == null)
                return 0f;

            float unitsPerPixel = (cachedCam.orthographicSize * 2f) / (float)Screen.height;
            return pixelPadding * unitsPerPixel;
        }

        private void CalculateCameraTargets()
        {
            if (cachedCam == null || LocalMapGenerator.Instance == null)
                return;

            // Get camera world-space bounds (orthographic assumed)

            float bottomPadding = GetVerticalWorldPadding(LocalCameraMovementManager.Instance.BottomPaddingPixels);
            float topPadding = GetVerticalWorldPadding(LocalCameraMovementManager.Instance.TopPaddingPixels);

            float fullHeight = cachedCam.orthographicSize * 2f;
            float fullWidth = fullHeight * cachedCam.aspect;

            float camHeight = fullHeight - bottomPadding - topPadding;
            float camWidth = fullWidth;

            /*float camHeight = cachedCam.orthographicSize * 2f - verticalPadding;
            float camWidth = (camHeight + verticalPadding) * cachedCam.aspect;*/
            /*float camHeight = cachedCam.orthographicSize * 2f - verticalPadding;
            float camWidth = cachedCam.orthographicSize * 2f * cachedCam.aspect;*/


            Vector3 camPos = cachedCam.transform.position;

            // World-space corners
            Vector3 bottomLeft = new Vector3(
                camPos.x - camWidth * 0.5f,
                camPos.y - fullHeight * 0.5f + bottomPadding);

            Vector3 topRight = new Vector3(
                camPos.x + camWidth * 0.5f,
                camPos.y + fullHeight * 0.5f - topPadding);

            // Convert to minimap space
            Vector2 bottomLeftOnMap = GetPositionOnMinimap(bottomLeft);
            Vector2 topRightOnMap = GetPositionOnMinimap(topRight);

            // Calculate rect size and position
            Vector2 size = topRightOnMap - bottomLeftOnMap;
            Vector2 center = (topRightOnMap + bottomLeftOnMap) / 2f;

            cameraTargetSize = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            cameraTargetPos = center;
        }

        private Image GetPoint()
        {
            if (pointPool.Count > 0)
            {
                var img = pointPool.Pop();
                img.gameObject.SetActive(true);
                return img;
            }

            return Instantiate(pointPrefab, minimapTransform);
        }

        private void ReturnPoint(Image img)
        {
            img.gameObject.SetActive(false);
            pointPool.Push(img);
        }

        private void HandleHeroPoint()
        {
            var hero = HeroManager.Instance?.ActiveHero;

            if (hero == null)
            {
                if (heroPoint != null)
                {
                    ReturnPoint(heroPoint);
                    heroPoint = null;
                }
                return;
            }

            if (heroPoint == null)
            {
                heroPoint = GetPoint();
                heroPoint.sprite = heroPointSprite;
                heroPoint.NativeRatioScaleHeightToSize(20f);
            }

            heroPoint.rectTransform.anchoredPosition =
                GetPositionOnMinimap(hero.transform.position);
        }

        private Vector2 GetPositionOnMinimap(Vector3 worldPos)
        {
            float x = Mathf.InverseLerp(minX, maxX, worldPos.x);
            float y = Mathf.InverseLerp(minY, maxY, worldPos.y);

            return new Vector2(
                Mathf.Lerp(-halfWidth, halfWidth, x),
                Mathf.Lerp(-halfHeight, halfHeight, y)
            );
        }
    }

    class MinimapEntity
    {
        public Transform transform;
        public Image image;

        public Vector2 previousPos;
        public Vector2 targetPos;

        public Vector3 lastWorldPos;
    }
}


