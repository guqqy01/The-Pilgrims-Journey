using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Units;
using UnityEngine.UIElements;
using Monsters;
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

            if (spellManager.spawnedObjects == null)
                spellManager.spawnedObjects = new List<GameObject>();

            spellManager.spawnedObjects.Add(obj);
        }
    }
}

public class InvunrabilityEffect : SpellEffect
{
    public float invunrabilityTime;

    public override bool CanUseEffect(Vector2Int position)
    {
        if (!TilePlayerManager.Instance.IsPlaying)
            return false;

        foreach (Collider2D collider in GetColliders2D(position))
        {
            if (collider.TryGetComponent(out Unit unit))
            {
                return true;
            }
        }

        return false;
    }

    public override void Effect(Vector2Int origin)
    {
        if (!CanUseEffect(origin))
        { return; }


        List<Collider2D> colliders =  GetColliders2D(origin);

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out Unit unit))
            {
                Transform tf = unit.transform;
                FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                fx.Init(fXGreyScaleFactor, fXColor);
                unit.MakeInvunrableForTime(invunrabilityTime);
            }
        }

        AudioManager.Instance.PlayClip(effectSound);
    }
}

public class StaticShockEffect : SpellEffect
{
    public float shockTime;

    public override bool CanUseEffect(Vector2Int position)
    {
        if (!TilePlayerManager.Instance.IsPlaying)
            return false;

        foreach (Collider2D collider in GetColliders2D(position))
        {
            if (collider.TryGetComponent(out Monster monster))
            {
                return true;
            }
        }

        return false;
    }

    public override void Effect(Vector2Int origin)
    {
        if(!CanUseEffect(origin))
            { return; }

        List<Collider2D> colliders = GetColliders2D(origin);

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out Monster monster))
            {
                Transform tf = monster.transform;
                FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                fx.Init(fXGreyScaleFactor, fXColor);
                monster.ShockMonster(shockTime);
            }
        }

        AudioManager.Instance.PlayClip(effectSound);
    }
}