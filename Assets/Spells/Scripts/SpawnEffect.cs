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

public class DemonShockEffect : StaticShockEffect
{
    [SerializeField]
    private int initalPowerLevel = 0;
    [SerializeField]
    private int amountOfUsesToLevelUp = 1;

    private int amountUsed;

    private int currentPowerLevel;


    public override List<Vector2Int> Vector2Ints => GetV2IntList();

    Dictionary<int, List<Vector2Int>> powerLevelToList = new Dictionary<int, List<Vector2Int>>();

    public override void Reset()
    {
        currentPowerLevel = initalPowerLevel;
    }

    private List<Vector2Int> GetV2IntList()
    {
        if (powerLevelToList.ContainsKey(currentPowerLevel))
        {
            return powerLevelToList[currentPowerLevel];
        }

        List<Vector2Int> returnList = new List<Vector2Int>();

        for (int i = 0; i <= currentPowerLevel; i++)
        {
            Vector2Int up = new Vector2Int(0, i);
            if (!returnList.Contains(up))
                returnList.Add(up);

            Vector2Int down = new Vector2Int(0, -i);
            if (!returnList.Contains(down))
                returnList.Add(down);

            Vector2Int left = new Vector2Int(-i, 0);
            if (!returnList.Contains(left))
                returnList.Add(left);

            Vector2Int right = new Vector2Int(i, 0);
            if (!returnList.Contains(right))
                returnList.Add(right);

            for (int j = 0; j < i - 1; j++)
            {
                //Clockwise

                Vector2Int upRight = new Vector2Int(j + 1, i - j - 1);
                if (!returnList.Contains(upRight))
                    returnList.Add(upRight);

                Vector2Int downRight = new Vector2Int(i - j - 1, -j - 1);
                if (!returnList.Contains(downRight))
                    returnList.Add(downRight);

                Vector2Int downLeft = new Vector2Int(-j - 1, -i + j + 1);
                if (!returnList.Contains(downLeft))
                    returnList.Add(downLeft);

                Vector2Int upLeft = new Vector2Int(-i + j + 1, j + 1);
                if (!returnList.Contains(upLeft))
                    returnList.Add(upLeft);
            }
        }

        powerLevelToList.Add(currentPowerLevel, returnList);

        return returnList;
    }

    public override void Effect(Vector2Int origin)
    {
        base.Effect(origin);

        amountUsed++;

        if(amountUsed >= amountOfUsesToLevelUp)
        {
            amountUsed = 0;
            currentPowerLevel++;
        }
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
                if(effectFX != null)
                {
                    FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                    fx.Init(fXGreyScaleFactor, fXColor);
                }
                monster.ShockMonster(shockTime);
            }
        }

        AudioManager.Instance.PlayClip(effectSound);
    }
}