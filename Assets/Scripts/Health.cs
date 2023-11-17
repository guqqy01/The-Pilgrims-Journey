using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 5;
    public float currentHealth = 5;

    public delegate void OnDie();
    public OnDie onDie;

    public delegate void OnHealthChange();
    public OnHealthChange onHealthChange;

    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(amount + currentHealth, 0, maxHealth);

        onHealthChange?.Invoke();

        if (currentHealth == 0 && onDie != null)
            onDie.Invoke();
    }
}

