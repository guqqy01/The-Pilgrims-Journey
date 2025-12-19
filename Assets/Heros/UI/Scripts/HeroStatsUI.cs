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


    private HeroStatBuffsManager heroStatsManager;
    public HeroStatBuffsManager HeroStatsManager
    {
        get
        {
            if(heroStatsManager == null)
                heroStatsManager = HeroStatBuffsManager.Instance;

            return heroStatsManager;
        }
    }

    private HeroEquipmentManager heroEquipmentManager;
    public HeroEquipmentManager HeroEquipmentManager
    {
        get
        {
            if(heroEquipmentManager == null)
                heroEquipmentManager = HeroEquipmentManager.Instance;

            return heroEquipmentManager;
        }
    }

    private OverallStatsManager overallBuffManager;
    public OverallStatsManager OverallBuffManager
    {
        get
        {
            if(overallBuffManager == null)
                overallBuffManager = OverallStatsManager.Instance;

            return overallBuffManager;
        }
    }

    private void Awake()
    {
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => HeroStatBuffsManager.Instance, SubscribeToHeroStatsManager);
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => OverallStatsManager.Instance, SubscribeToOverallStatsManager);
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => HeroEquipmentManager.Instance, SubscribeToHeroEquipmentManager);
    }

    private void SubscribeToHeroEquipmentManager()
    {
        HeroEquipmentManager.Inventory.onEquipmentChanged += UpdateHeroStatsFromEquipmentChange;
        UpdateHeroStatsUI();
    }

    private void SubscribeToHeroStatsManager()
    {
        HeroStatsManager.buffsChanged += UpdateHeroStatsUI;
        UpdateHeroStatsUI();
    }

    private void SubscribeToOverallStatsManager()
    {
        OverallBuffManager.buffsChanged += UpdateOverallStatsUI;
        UpdateOverallStatsUI();
    }

    private void OnDestroy()
    {
        if(HeroStatsManager != null)
        {
            HeroStatsManager.buffsChanged -= UpdateHeroStatsUI;
        }

        if(OverallBuffManager != null)
        {
            OverallBuffManager.buffsChanged -= UpdateOverallStatsUI;
        }

        if(HeroEquipmentManager != null)
        {
            HeroEquipmentManager.Inventory.onEquipmentChanged -= UpdateHeroStatsFromEquipmentChange;
        }
    }

    public void UpdateAllStatsUI()
    {
        UpdateHeroStatsUI();
        UpdateOverallStatsUI();
    }

    private void UpdateHeroStatsFromEquipmentChange(EquipmentContainer container, EquipmentSO from, EquipmentSO to)
    {
        UpdateAllStatsUI();
    }

    public void UpdateHeroStatsUI()
    {
        if (HeroStatsManager == null)
            return;

        maxHealthText.text = "Max Health: " + ((int)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.Health)).ToString();
        maxShieldText.text = "Max Shield: " + ((int)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.Shield)).ToString();
        ShieldRegenerationText.text = "Shield Regeneration: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.ShieldRegeneration)).ToString();
        ShieldCooldownText.text = "Shield Cooldown: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.ShieldCooldown)).ToString();

        MovementSpeedText.text = "Movement Speed: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.MovementSpeed)).ToString();

        DamageText.text = "Damage Buff: " + ((int)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.Damage)).ToString();
        AttackSpeedText.text = "Attack Speed: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.AttackSpeed)).ToString();
        NumberOfProjectilesText.text = "Number of Projectiles: " + ((int)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.NumberOfProjectiles)).ToString();
        CriticalChanceText.text = "Critical Chance: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.CriticalChance)).ToString() + "%";
        CriticalDamageText.text = "Critical Damage: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.CriticalDamageMultiplier)).ToString() + "%";
        SizeText.text = "Attack Size: " + ((float)HeroStatBase.GetBuffableStat(HeroManager.Instance.heroSO, HeroStats.AttackSize)).ToString();
    }

    public void UpdateOverallStatsUI()
    {
        if (OverallBuffManager == null)
            return;

        LuckText.text = "Luck: " + ((float)OverallBuffManager.Luck).ToString();
        ExperienceText.text = "Experience Gain: " + ((float)OverallBuffManager.ExperienceGain).ToString();
    }
}
