using Heros;
using LocalMap;
using Monsters;
using System.Collections.Generic;
using System.Linq;
using Units;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UI;
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

    private readonly Dictionary<Transform, Image> monsterPoints = new();
    private readonly Dictionary<Transform, Image> unitPoints = new();

    private readonly Stack<Image> pointPool = new();

    private float minX, maxX, minY, maxY;
    private float halfWidth, halfHeight;


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
        updateTimer += Time.unscaledDeltaTime;
        if (updateTimer < UPDATE_INTERVAL)
            return;

        updateTimer = 0f;

        HandleHeroPoint();
        UpdateEntityPoints(
            MonsterSpawnManager.Instance?.Monsters,
            monsterPoints,
            monsterPointSprite
        );

        UpdateEntityPoints(
            UnitSpawningManager.Instance?.SpawnedUnits,
            unitPoints,
            unitPointSprite
        );

        HandleCameraDisplayRect();
    }

    private void CacheMapBounds()
    {
        if (LocalMapGenerator.Instance == null)
            return;

        minX = -0.5f;
        maxX = LocalMapGenerator.Instance.MapSizeX - 0.5f;
        minY = -0.5f;
        maxY = LocalMapGenerator.Instance.MapSizeY - 0.5f;

        halfWidth = minimapTransform.sizeDelta.x * 0.5f;
        halfHeight = minimapTransform.sizeDelta.y * 0.5f;
    }

    private float GetVerticalWorldPadding(float pixelPadding)
    {
        if (cachedCam == null)
            return 0f;
        
        float unitsPerPixel = (cachedCam.orthographicSize * 2f) / (float)Screen.height;
        return pixelPadding * unitsPerPixel;
    }

    private void HandleCameraDisplayRect()
    {
        if (cachedCam == null || LocalMapGenerator.Instance == null)
            return;

        // Get camera world-space bounds (orthographic assumed)

        float verticalPadding = GetVerticalWorldPadding(LocalCameraMovementManager.Instance.BottomPaddingPixels + LocalCameraMovementManager.Instance.TopPaddingPixels);

        float camHeight = cachedCam.orthographicSize * 2f - verticalPadding;
        float camWidth = (camHeight + verticalPadding) * cachedCam.aspect;

        Vector3 camPos = cachedCam.transform.position;

        // World-space corners
        Vector3 bottomLeft = new Vector3(camPos.x - camWidth / 2f, camPos.y - camHeight / 2f);
        Vector3 topRight = new Vector3(camPos.x + camWidth / 2f, camPos.y + camHeight / 2f);

        // Convert to minimap space
        Vector2 bottomLeftOnMap = GetPositionOnMinimap(bottomLeft);
        Vector2 topRightOnMap = GetPositionOnMinimap(topRight);

        // Calculate rect size and position
        Vector2 size = topRightOnMap - bottomLeftOnMap;
        Vector2 center = (topRightOnMap + bottomLeftOnMap) / 2f;

        cameraDisplayRect.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        cameraDisplayRect.anchoredPosition = center;
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

    private void UpdateEntityPoints<T>(
    IReadOnlyList<T> entities,
    Dictionary<Transform, Image> pointMap,
    Sprite sprite
) where T : MonoBehaviour
    {
        if (entities == null)
            return;

        // Mark all existing points as unused
        foreach (var kvp in pointMap)
            kvp.Value.enabled = false;

        // Add / update
        for (int i = 0; i < entities.Count; i++)
        {
            Transform t = entities[i].transform;

            if (!pointMap.TryGetValue(t, out var img))
            {
                img = GetPoint();
                img.sprite = sprite;
                img.NativeRatioScaleHeightToSize(20f);
                pointMap.Add(t, img);
            }

            img.enabled = true;
            img.rectTransform.anchoredPosition = GetPositionOnMinimap(t.position);
        }

        // Cleanup dead transforms
        RemoveDeadEntries(pointMap);
    }

    private Vector2 GetPositionOnMinimap(Transform targetTransform)
    {
        return GetPositionOnMinimap(targetTransform.position);
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
