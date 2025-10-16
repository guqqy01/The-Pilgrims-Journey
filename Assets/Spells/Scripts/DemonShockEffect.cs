using UnityEngine;
using System.Collections.Generic;
using Monsters;
using Tiles;
using Units;

public class DemonShockEffect : StaticShockEffect
{
    [SerializeField]
    private int initalPowerLevel = 0;
    [SerializeField]
    private int amountOfUsesToLevelUp = 2;

    private int amountUsed;

    private int currentPowerLevel;

    [SerializeField]
    private LayerMask playerLayerMask;

    public override List<Vector2Int> Vector2Ints => GetV2IntList();

    private readonly Dictionary<int, List<Vector2Int>> powerLevelToList = new Dictionary<int, List<Vector2Int>>();

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

    private void DamageEffect(Vector2Int origin)
    {
        List<Collider2D> colliders = GetColliders2D(origin, playerLayerMask);

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out Health health))
            {
                Transform tf = health.transform;
                if (effectFX != null)
                {
                    FX fx = GameObject.Instantiate(effectFX, tf.position, Quaternion.identity, tf);
                    fx.Init(fXGreyScaleFactor, fXColor);
                }
                health.ModifyHealth(-(currentPowerLevel * currentPowerLevel));
            }
        }

        AudioManager.Instance.PlayClip(effectSound);
    }

    public override void Effect(Vector2Int origin)
    {

        if (!CanUseEffect(origin))
        { return; }


        base.Effect(origin);

        DamageEffect(origin);

        amountUsed++;

        if(amountUsed >= amountOfUsesToLevelUp)
        {
            amountUsed = 0;
            currentPowerLevel++;
        }
    }
}
