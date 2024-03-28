using UnityEngine;
using Units;

public class HealthPotionSpecial : MonoBehaviour, EquipmentSpecial
{
    private Health health;
    private Unit unit;

    private float healthPercentageToStartRecovery;

    private bool started;

    private float timeToNextHeal = 0;

    private float recoveryRate;

    private int amountOfHealthToRecover;

    

    private void Awake()
    {
        unit = GetComponent<Unit>();
        health = GetComponent<Health>();
        health.onHealthChange += OnHealthChange;
    }

    public void Init(params object[] parameters)
    {
        healthPercentageToStartRecovery = (float)parameters[0];
        amountOfHealthToRecover = Mathf.CeilToInt(health.MaxHealth * (float)parameters[1]);
        recoveryRate = (float)parameters[2] / amountOfHealthToRecover; ;
    }

    private void OnHealthChange(int currentHealth)
    {
        if ((float)currentHealth / health.MaxHealth < healthPercentageToStartRecovery)
            started = true;
    }

    private void Update()
    {
        if (!started)
            return;

        if (timeToNextHeal > Time.time)
            return;

        if (!unit.healthPotionParticleSystem.isPlaying)
            unit.healthPotionParticleSystem.Play();

        timeToNextHeal = Time.time + recoveryRate;

        health.ModifyHealth(1);

        amountOfHealthToRecover--;

        if (amountOfHealthToRecover <= 0)
        {
            unit.healthPotionParticleSystem.Stop();
            Destroy(this);
        }
    }
}


public interface EquipmentSpecial
{
    public void Init(params object[] parameters);
}