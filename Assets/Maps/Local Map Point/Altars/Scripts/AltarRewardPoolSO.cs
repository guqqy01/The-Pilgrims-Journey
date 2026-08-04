using UnityEngine;
using System.Collections.Generic;
using Relics;

[CreateAssetMenu(menuName = "Altar/Reward Pool")]
public class AltarRewardPoolSO : ScriptableObject
{
    public List<RelicSO> relicRewards;
}
