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
    private HeroManager HeroManager => HeroManager.Instance;

    private void Awake()
    {
        WaitForHelper.WaitForObjectThenPerformAction(() => HeroManager.Instance, SubscribeToHeroStatsSetup);
        WaitForHelper.WaitForConditionThenPerformAction(() => heroStats != null, SubscribeToStatChange);
    }

    private void SubscribeToHeroStatsSetup()
    {
        if (heroStats != null)
            heroStats.statChanged -= UpdateHeroStatsUI;

        HeroManager.onStatsSetup -= HeroStatsSetup;
        HeroManager.onStatsSetup += HeroStatsSetup;
        HeroStatsSetup(HeroManager.Stats);

    }

    private void HeroStatsSetup(HeroStatContainer heroStatContainer)
    {
        heroStats = heroStatContainer;
    }

    private void SubscribeToStatChange()
    {
        heroStats.statChanged -= UpdateHeroStatsUI;
        heroStats.statChanged += UpdateHeroStatsUI;
        UpdateHeroStatsUI();
    }

    public void UpdateHeroStatsUI()
    {
        if (heroStats == null)
            return;

        maxHealthText.text = "Max Health: " + ((int)heroStats.Get(HeroStats.Health)).ToString();
        maxShieldText.text = "Max Shield: " + ((int)heroStats.Get(HeroStats.Shield)).ToString();
        ShieldRegenerationText.text = "Shield Regeneration: " + ((float)heroStats.Get(HeroStats.ShieldRegeneration)).ToString();
        ShieldCooldownText.text = "Shield Cooldown: " + ((float)heroStats.Get(HeroStats.ShieldCooldown)).ToString();

        MovementSpeedText.text = "Movement Speed: " + ((float)heroStats.Get(HeroStats.MovementSpeed)).ToString();

        DamageText.text = "Damage Buff: " + ((int)heroStats.Get(HeroStats.Damage)).ToString();
        AttackSpeedText.text = "Attack Speed: " + ((float)heroStats.Get(HeroStats.AttackSpeed)).ToString();
        NumberOfProjectilesText.text = "Number of Projectiles: " + ((int)heroStats.Get(HeroStats.NumberOfAttacks)).ToString();
        CriticalChanceText.text = "Critical Chance: " + ((float)heroStats.Get(HeroStats.CriticalChance)).ToString() + "%";
        CriticalDamageText.text = "Critical Damage: " + ((float)heroStats.Get(HeroStats.CriticalDamageMultiplier)).ToString() + "%";
        SizeText.text = "Attack Size: " + ((float)heroStats.Get(HeroStats.AttackSize)).ToString();
    }

    private void OnDestroy()
    {
        if(heroStats != null)
        {
            heroStats.statChanged -= UpdateHeroStatsUI;
        }

        if(HeroManager != null)
        {
            HeroManager.onStatsSetup -= HeroStatsSetup;
        }
    }
}
