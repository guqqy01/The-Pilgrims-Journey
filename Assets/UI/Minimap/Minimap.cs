using Heros;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LocalMap;
using Unity.Burst;
using Monsters;
using System.Linq;
using Units;
public class Minimap : MonoBehaviour
{
    [SerializeField]
    private Image pointPrefab;

    [SerializeField]
    private RectTransform cameraDisplayRect;

    [SerializeField]
    private RectTransform minimapTransform;

    private Image heroPoint;
    private readonly Dictionary<Transform, Image> monsterPoints = new Dictionary<Transform, Image>();
    private readonly Dictionary<Transform, Image> unitPoints = new Dictionary<Transform, Image>();

    [SerializeField] private Color heroColor;
    [SerializeField] private Color monsterColor = Color.red;
    [SerializeField] private Color unitColor = Color.green;


    private void Update()
    {
        HandlePointPlacements();
        HandleCameraDisplayRect();
    }

    private void HandlePointPlacements()
    {
        HandleHeroPoint();
        HandleMonsterPoints();
        HandleUnitPoints();
    }

    private void HandleCameraDisplayRect()
    {
        if (Camera.main == null || LocalMapGenerator.Instance == null)
            return;

        Camera cam = Camera.main;

        // Get camera world-space bounds (orthographic assumed)
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;

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

    private void HandleHeroPoint()
    {
        if (HeroManager.Instance == null)
            return;

        if(HeroManager.Instance.ActiveHero == null)
        {
            if(heroPoint != null)
                Destroy(heroPoint.gameObject);

            return;
        }

        if(heroPoint == null)
        {
            heroPoint = Instantiate(pointPrefab, minimapTransform);
        }

        heroPoint.color = heroColor;
        heroPoint.rectTransform.anchoredPosition = GetPositionOnMinimap(HeroManager.Instance.ActiveHero.transform);
    }

    private void HandleMonsterPoints()
    {
        if (MonsterSpawnManager.Instance == null)
            return;

        var monsters = MonsterSpawnManager.Instance.Monsters;

        // Remove dead monsters
        var deadKeys = monsterPoints.Keys.Where(m => m == null || !monsters.Any(x => x.transform == m)).ToList();
        foreach (var dead in deadKeys)
        {
            Destroy(monsterPoints[dead].gameObject);
            monsterPoints.Remove(dead);
        }

        // Add new monsters
        foreach (var monster in monsters)
        {
            var t = monster.transform;
            if (!monsterPoints.ContainsKey(t))
            {
                var point = Instantiate(pointPrefab, minimapTransform);
                point.color = monsterColor;
                monsterPoints.Add(t, point);
            }
        }

        // Update positions
        foreach (var kvp in monsterPoints)
        {
            if (kvp.Key != null)
                kvp.Value.rectTransform.anchoredPosition = GetPositionOnMinimap(kvp.Key);
        }

    }

    private void HandleUnitPoints()
    {
        // Example: if you have a UnitManager
        if (UnitManager.Instance == null)
            return;

        var units = UnitSpawningManager.Instance.SpawnedUnits;

        // Remove dead units
        var deadKeys = unitPoints.Keys.Where(u => u == null || !units.Any(x => x.transform == u)).ToList();
        foreach (var dead in deadKeys)
        {
            Destroy(unitPoints[dead].gameObject);
            unitPoints.Remove(dead);
        }

        // Add new units
        foreach (var unit in units)
        {
            var t = unit.transform;
            if (!unitPoints.ContainsKey(t))
            {
                var point = Instantiate(pointPrefab, minimapTransform);
                point.color = unitColor;
                unitPoints.Add(t, point);
            }
        }

        // Update positions
        foreach (var kvp in unitPoints)
        {
            if (kvp.Key != null)
                kvp.Value.rectTransform.anchoredPosition = GetPositionOnMinimap(kvp.Key);
        }

    }

    private Vector2 GetPositionOnMinimap(Transform targetTransform)
    {
        return GetPositionOnMinimap(targetTransform.position);
    }

    private Vector2 GetPositionOnMinimap(Vector3 worldPos)
    {
        float minX = -0.5f;
        float maxX = LocalMapGenerator.Instance.MapSizeX - 0.5f;
        float minY = -0.5f;
        float maxY = LocalMapGenerator.Instance.MapSizeY - 0.5f;

        float relativeXPos = Mathf.InverseLerp(minX, maxX, worldPos.x);
        float relativeYPos = Mathf.InverseLerp(minY, maxY, worldPos.y);

        float halfWidth = minimapTransform.sizeDelta.x / 2f;
        float halfHeight = minimapTransform.sizeDelta.y / 2f;

        float xPosOnMinimap = Mathf.Lerp(-halfWidth, halfWidth, relativeXPos);
        float yPosOnMinimap = Mathf.Lerp(-halfHeight, halfHeight, relativeYPos);

        return new Vector2(xPosOnMinimap, yPosOnMinimap);
    }
}
