using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class EquipmentSO : ScriptableObject
{
    public Sprite sprite;

    public List<EquipmentIntBuff> IntBuffs;
    public List<EquipmentFloatBuff> FloatBuffs;
}
