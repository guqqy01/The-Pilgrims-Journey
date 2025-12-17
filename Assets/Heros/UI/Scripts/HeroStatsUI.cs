using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Heros;
using LevelingSystem;
using Helper.Wait;
public class HeroStatsUI : MonoBehaviour
{
    private HeroStatsManager heroStatsManager;
    public HeroStatsManager HeroStatsManager
    {
        get
        {
            if(heroStatsManager == null)
                heroStatsManager = HeroStatsManager.Instance;

            return heroStatsManager;
        }
    }

    private OverallBuffManager overallBuffManager;
    public OverallBuffManager OverallBuffManager
    {
        get
        {
            if(overallBuffManager == null)
                overallBuffManager = OverallBuffManager.Instance;

            return overallBuffManager;
        }
    }

    private void Awake()
    {
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => HeroStatsManager.Instance, SubscribeToHeroStatsManager);
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => OverallBuffManager.Instance, SubscribeToOverallStatsManager);
    }

    private void SubscribeToHeroStatsManager()
    {

    }

    private void SubscribeToOverallStatsManager()
    {

    }

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

    public void UpdateAllStatsUI()
    {
        UpdateHeroStatsUI();
        UpdateOverallStatsUI();
    }

    public void UpdateHeroStatsUI()
    {
        if (HeroStatsManager == null)
            return;

        maxHealthText.text = "Max Health: " + ((int)HeroStatsManager.MaxHealth).ToString();
        maxShieldText.text = "Max Shield: " + ((int)HeroStatsManager.Shield).ToString();
        ShieldRegenerationText.text = "Shield Regeneration: " + ((float)HeroStatsManager. ShieldRegeneration).ToString();
        ShieldCooldownText.text = "Shield Cooldown: " + ((float)HeroStatsManager.ShieldCooldown).ToString();

        MovementSpeedText.text = "Movement Speed: " + ((float)HeroStatsManager.MovementSpeed).ToString();

        DamageText.text = "Damage Buff: " + ((int)HeroStatsManager.DamageBuff).ToString();
        AttackSpeedText.text = "Attack Speed: " + ((float)HeroStatsManager.AttackSpeed).ToString();
        NumberOfProjectilesText.text = "Number of Projectiles: " + ((int)HeroStatsManager.NumberOfProjectiles).ToString();
        CriticalChanceText.text = "Critical Chance: " + ((float)HeroStatsManager.CriticalChance).ToString() + "%";
        CriticalDamageText.text = "Critical Damage: " + ((float)HeroStatsManager.CriticalDamage).ToString() + "%";
        SizeText.text = "Attack Size: " + ((float)HeroStatsManager.AttackSize).ToString();
    }

    public void UpdateOverallStatsUI()
    {
        if (OverallBuffManager == null)
            return;

        LuckText.text = "Luck: " + ((float)OverallBuffManager.Luck).ToString();
        ExperienceText.text = "Luck: " + ((float)OverallBuffManager.ExperienceGain).ToString();
    }
}
