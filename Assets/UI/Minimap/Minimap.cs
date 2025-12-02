using Heros;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LocalMap;
using Unity.Burst;
using Monsters;
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

    [SerializeField]
    private Color heroColor;

    private void Update()
    {
        HandlePointPlacements();
    }

    private void HandlePointPlacements()
    {
        HandleHeroPoint();
        HandleMonsterPoints();
        HandleUnitPoints();
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

        if(heroPoint.color != heroColor)
            heroPoint.color = heroColor;

        heroPoint.rectTransform.anchoredPosition = GetPositionOnMinimap(HeroManager.Instance.ActiveHero.transform);
    }

    private void HandleMonsterPoints()
    {
        if (MonsterSpawnManager.Instance == null)
            return;


        foreach(var point in MonsterSpawnManager.Instance.Monsters)
        {
            if (!monsterPoints.ContainsKey(point.transform))
            {
                Image monsterPoint = Instantiate(pointPrefab, minimapTransform);

                monsterPoints.Add(point.transform, monsterPoint);
            }
        }
    }

    private void HandleUnitPoints()
    {

    }

    private void HandleMinimap()
    {

    }

    private Vector2 GetPositionOnMinimap(Transform targetTransform)
    {
        float minX = -0.5f;
        float maxX = LocalMapGenerator.Instance.MapSizeX - 0.5f;
        float minY = -0.5f;
        float maxY = LocalMapGenerator.Instance.MapSizeY - 0.5f;

        float relativeXPos = Mathf.InverseLerp(minX, maxX, targetTransform.position.x);
        float relativeYPos = Mathf.InverseLerp(minY, maxY, targetTransform.position.y);

        float halfWidth = minimapTransform.sizeDelta.x / 2f;
        float halfHeight = minimapTransform.sizeDelta.y / 2f;

        float xPosOnMinimap = Mathf.Lerp(-halfWidth, halfWidth, relativeXPos);
        float yPosOnMinimap = Mathf.Lerp(-halfHeight, halfHeight, relativeYPos);

        return new Vector2(xPosOnMinimap, yPosOnMinimap);
    }
}
