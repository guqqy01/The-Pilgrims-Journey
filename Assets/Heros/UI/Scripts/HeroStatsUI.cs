using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Heros;
using LevelingSystem;
using Helper.Wait;
using Equipment;
public class HeroStatsUI : MonoBehaviour
{
    [Header("Defence Stats")]
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI maxShieldText;
    [SerializeField] private TextMeshProUGUI ShieldRegenerationText;
    [SerializeField] private TextMeshProUGUI ShieldCooldownText;
    [Header("Utility Stats")]
    [SerializeField] private TextMeshProUGUI MovementSpeedText;
    [SerializeField] private TextMeshProUGUI LuckText;
    [SerializeField] private TextMeshProUGUI ExperienceText;
    [Header("Attack Stats")]
    [SerializeField] private TextMeshProUGUI DamageText;
    [SerializeField] private TextMeshProUGUI AttackSpeedText;
    [SerializeField] private TextMeshProUGUI NumberOfProjectilesText;
    [SerializeField] private TextMeshProUGUI CriticalChanceText;
    [SerializeField] private TextMeshProUGUI CriticalDamageText;
    [SerializeField] private TextMeshProUGUI SizeText;


    private HeroStatContainer heroStats;
    public HeroStatContainer HeroStatContainer
    {
        get
        {
            if(heroStats == null && HeroManager.Instance != null)
                heroStats = HeroManager.Instance.Stats;

            return heroStats;
        }
    }

    private void Awake()
    {
        WaitForHelper.WaitForConditionThenPerformAction(() => HeroStatContainer != null, SubscribeToStatChange);
    }

    private void SubscribeToStatChange()
    {
        HeroStatContainer.statChanged += UpdateHeroStatsUI;
        UpdateHeroStatsUI();
    }

    public void UpdateHeroStatsUI()
    {
        if (HeroStatContainer == null)
            return;

        maxHealthText.text = "Max Health: " + ((int)HeroStatContainer.Get(HeroStats.Health)).ToString();
        maxShieldText.text = "Max Shield: " + ((int)HeroStatContainer.Get(HeroStats.Shield)).ToString();
        ShieldRegenerationText.text = "Shield Regeneration: " + ((float)HeroStatContainer.Get(HeroStats.ShieldRegeneration)).ToString();
        ShieldCooldownText.text = "Shield Cooldown: " + ((float)HeroStatContainer.Get(HeroStats.ShieldCooldown)).ToString();

        MovementSpeedText.text = "Movement Speed: " + ((float)HeroStatContainer.Get(HeroStats.MovementSpeed)).ToString();

        DamageText.text = "Damage Buff: " + ((int)HeroStatContainer.Get(HeroStats.Damage)).ToString();
        AttackSpeedText.text = "Attack Speed: " + ((float)HeroStatContainer.Get(HeroStats.AttackSpeed)).ToString();
        NumberOfProjectilesText.text = "Number of Projectiles: " + ((int)HeroStatContainer.Get(HeroStats.NumberOfProjectiles)).ToString();
        CriticalChanceText.text = "Critical Chance: " + ((float)HeroStatContainer.Get(HeroStats.CriticalChance)).ToString() + "%";
        CriticalDamageText.text = "Critical Damage: " + ((float)HeroStatContainer.Get(HeroStats.CriticalDamageMultiplier)).ToString() + "%";
        SizeText.text = "Attack Size: " + ((float)HeroStatContainer.Get(HeroStats.AttackSize)).ToString();
    }
}
