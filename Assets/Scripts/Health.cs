using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth = 5;

    public delegate void OnDie();
    public OnDie onDie;

    public delegate void OnHealthChange(int currentHealth);
    public OnHealthChange onHealthChange;

    private void Start()
    {
        currentHealth = maxHealth;
        onHealthChange?.Invoke(currentHealth);
    }

    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.RoundToInt(Mathf.Clamp(amount + currentHealth, 0, maxHealth));

        onHealthChange?.Invoke(currentHealth);

        if (currentHealth == 0)
        {
            onDie?.Invoke();
            Destroy(gameObject);
        }
            
    }
}

