using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using Map;
using Tiles;

public class SpawnEffect : SpellEffect
{
    [SerializeField]
    private GameObject spawnedObject;

    public override bool CanUseEffect(Vector2Int position)
    {
        if (!TilePlayerManager.Instance.IsPlaying)
            return false;
        return true;
    }

    public override void Effect(Vector2Int origin)
    {
        SpellManager spellManager = SpellManager.Instance;

        foreach(Vector2Int position in GetPositions(origin))
        {
            GameObject obj = GameObject.Instantiate(spawnedObject, (Vector2)position, Quaternion.identity, spellManager.transform);

            spellManager.spawnedObjects ??= new List<GameObject>();

            spellManager.spawnedObjects.Add(obj);
        }
    }
}
