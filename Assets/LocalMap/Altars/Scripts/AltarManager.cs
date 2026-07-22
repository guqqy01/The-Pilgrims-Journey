using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameManagerCalls;

public class AltarManager : Singleton<AltarManager>, IOnAfterCreateLocalMap
{
    [SerializeField]
    private WeightedChanceList<AltarSO> altarSOs;
    private List<Altar> _altars;
    [SerializeField]
    private AltarRewardPoolSO altarRewardPool;
    public AltarRewardPoolSO AltarRewardPool => altarRewardPool;

    public void AfterCreateLocalMap()
    {
        Altar[] altars = Resources.FindObjectsOfTypeAll<Altar>();
        _altars = altars.ToList();

        foreach(Altar altar in _altars)
        {
            if (altar == null)
                continue;

            altar.Setup(altarSOs.GetRandom(), AltarRewardPool.relicRewards.GetRandom());
        }
    }
}
