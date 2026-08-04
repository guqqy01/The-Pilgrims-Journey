using UnityEngine;
using System.Collections.Generic;
using GameManagerCalls;
using GlobalMap;
using Random = UnityEngine.Random;
namespace LocalMap
{
    public class DynamicMapObjectManager : Singleton<DynamicMapObjectManager>, IOnBeforeCreateLocalMap, IOnAfterCreateLocalMap, IOnClear
    {
        private List<DynamicMapObject> _dynamicMapObjects = new List<DynamicMapObject>();
        public IReadOnlyList<DynamicMapObject> DynamicMapObjects => _dynamicMapObjects;

        private Dictionary<DynamicMapObject, float> _objectAndNextSpawnTime;

        public List<GameObject> spawnedObjects = new List<GameObject>();

        private bool _spawning = false;

        public void SetupDynmaicList(List<DynamicMapObject> dynamicMapObjects)
        {
            _dynamicMapObjects = dynamicMapObjects;

            _objectAndNextSpawnTime = new Dictionary<DynamicMapObject, float>();

            foreach (DynamicMapObject obj in _dynamicMapObjects)
            {
                _objectAndNextSpawnTime.Add(obj, obj.GetNextSpawnTime());
            }
        }

        private void Update()
        {
            if (!_spawning)
                return;

            Dictionary<DynamicMapObject, float> toBeChanged = new Dictionary<DynamicMapObject, float>();

            foreach (var obj in _objectAndNextSpawnTime)
            {
                DynamicMapObject dynamicMapObject = obj.Key;
                float timeToNextSpawn = obj.Value;

                if(Time.time >= timeToNextSpawn)
                {
                    toBeChanged.Add(dynamicMapObject, SpawnDynamicMapObject(dynamicMapObject));
                }
            }

            foreach (var obj in toBeChanged)
            {
                _objectAndNextSpawnTime[obj.Key] = obj.Value;
            }
        }

        public void OnBeforeCreateLocalMap()
        {
            _spawning = false;
            spawnedObjects.DestroyAndClearList();
        }

        public void AfterCreateLocalMap()
        {
            SetupDynmaicList(GlobalMapGenerator.Instance.CurrentMapPoint.GenerationSettings.dynamicMapObjects);

            BeginSpawn();
        }

        private Vector2 GetSpawnPosition(DynamicMapObject dynamicMapObject)
        {
            LocalMapGenerator lMG = LocalMapGenerator.Instance;

            bool randomHorizontal = Random.value > 0.5f;
            bool randomVertical = Random.value > 0.5f;

            float outerXPos = randomHorizontal ? -1f : lMG.MapSizeX + 1f;
            float outerYPos = randomVertical ? -1f : lMG.MapSizeY + 1f;

            float innerXPos = Random.Range(-0.5f, lMG.MapSizeX + 0.5f);
            float innerYPos = Random.Range(-0.5f, lMG.MapSizeX + 0.5f);

            switch (dynamicMapObject.dynamicMapObjectSpawnMethod)
            {
                case DynamicMapObjectSpawnMethod.RandomInMap:
                    return new Vector2(innerXPos, innerYPos);
                case DynamicMapObjectSpawnMethod.SpecificPosition:
                    return dynamicMapObject.GetPosition();
                case DynamicMapObjectSpawnMethod.OutsideOfMap:
                    return new Vector2(outerXPos, outerYPos);
                case DynamicMapObjectSpawnMethod.OutsideOfMapHorizontal:
                    return new Vector2(outerXPos, innerYPos);
                case DynamicMapObjectSpawnMethod.OutsideOfMapVertical:
                    return new Vector2(innerXPos, outerYPos);
                default: 
                    return Vector2.zero;
            }
        }

        private void BeginSpawn()
        {
            _spawning = true;
        }

        private float SpawnDynamicMapObject(DynamicMapObject dynamicMapObject)
        {
            if (dynamicMapObject == null || dynamicMapObject.prefab == null)
                return 0;

            GameObject spawnedObj = Instantiate(dynamicMapObject.prefab, GetSpawnPosition(dynamicMapObject), Quaternion.identity, transform);
            spawnedObjects.Add(spawnedObj);

            return dynamicMapObject.GetNextSpawnTime();
        }

        public void Clear()
        {
            spawnedObjects.DestroyAndClearList();
            _dynamicMapObjects.Clear();
        }
    }
}


