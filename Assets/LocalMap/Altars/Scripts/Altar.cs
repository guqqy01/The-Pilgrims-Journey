using Heros;
using System;
using UnityEngine;
using Relics;
using Equipment;
using Monsters;
using System.Collections.Generic;
using LocalMap;
using GlobalMap;
public class Altar : MonoBehaviour, IInteractable
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    public AltarSO AltarSO {  get; private set; }
    
    public int Cost { get; private set; }

    public event Action<bool> HeroRangeChange;

    public bool InteractedWith { private set; get; } = false;

    public RelicSO Reward { private set; get; }

    public event Action UpdateUI;

    public void Setup(AltarSO altarSO, RelicSO reward)
    {
        AltarSO = altarSO;
        spriteRenderer.sprite = altarSO.altarSprite;
        Cost = altarSO.costRange.RandomPoint();
        Reward = reward;
        UpdateUI?.Invoke();
    }

    public void RaiseHeroRangeChange(bool inRange)
    {
        if(!InteractedWith)
            HeroRangeChange?.Invoke(inRange);
    }

    public bool CanInteract()
    {
        if (InteractedWith)
            return false;

        Health heroHealth = HeroManager.Instance.ActiveHero.HealthAndShieldManager.Health;

        switch (AltarSO.AltarPayment)
        {
            case AltarPayment.Gold:
                return ResourceManager.Instance.Gold >= Cost;
            case AltarPayment.Health:
                return heroHealth.CurrentHealth >= Cost;
            case AltarPayment.HealthPercentage:
                return Mathf.FloorToInt(heroHealth.CurrentHealth * 100f / heroHealth.MaxHealth) >= Cost;
            case AltarPayment.Combat:
                return true;
        }

        return true;
    }

    private void TakePayment()
    {
        Health heroHealth = HeroManager.Instance.ActiveHero.HealthAndShieldManager.Health;
        Vector2 hitDirection = (transform.position - heroHealth.transform.position).normalized;
        switch (AltarSO.AltarPayment)
        {
            case AltarPayment.Gold:
                ResourceManager.Instance.ModifyGold(-Cost);
                break;
            case AltarPayment.Health:
                heroHealth.ModifyHealth(-Cost, hitDirection, 0f);
                break;
            case AltarPayment.HealthPercentage:
                int healthPaid = Mathf.FloorToInt(heroHealth.MaxHealth * Cost / 100f);
                heroHealth.ModifyHealth(-healthPaid, hitDirection, 0f);
                break;
            case AltarPayment.Combat:
                BeingCombatTrail();
                break;
        }
    }

    private void GiveReward()
    {
        switch (AltarSO.AltarPayment)
        {
            case AltarPayment.Combat:
                WaitForCombatFinished();
                return;
        }

        RelicManager.Instance.AddRelic(Reward);
    }

    private void BeingCombatTrail()
    {
        List<WaveMonsterSpawn> possibleSpawns = new List<WaveMonsterSpawn>();

        MonsterSpawnSettings spawnSettings = GlobalMapGenerator.Instance.CurrentMapPoint.GenerationSettings.MonsterSpawnSettings;

        foreach(MonsterSpawn monsterSpawn in spawnSettings.MonsterSpawns)
        {
            if (monsterSpawn is not WaveMonsterSpawn waveMonsterSpawn) continue;

            possibleSpawns.Add(waveMonsterSpawn);
        }

        MonsterSpawnManager.Instance.BeginSpawningWave(possibleSpawns.GetRandom(), 1, Cost);
    }

    private void WaitForCombatFinished()
    {
        RelicManager.Instance.AddRelic(Reward);
    }

    public bool Interact()
    {
        if(!CanInteract())
            return false;

        InteractedWith = true;

        TakePayment();
        GiveReward();

        spriteRenderer.sprite = AltarSO.brokenAltarSprite;
        UpdateUI?.Invoke();

        return true;
    }
}
