using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Vector2ListWrapper
{
    public List<Vector2Int> v2IntList;

    public static implicit operator List<Vector2Int>(Vector2ListWrapper wrapper) => wrapper.v2IntList;
}