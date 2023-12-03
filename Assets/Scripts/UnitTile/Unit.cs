using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Unit : ScriptableObject
{
    public float baseSpawnRate = 1f;

    public Movement movement;

    public Sprite unitSprite;
}
