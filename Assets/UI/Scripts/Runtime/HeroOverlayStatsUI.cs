using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LevelingSystem;
public class HeroOverlayStatsUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI levelText;

    private void Awake()
    {
        ExperienceAndLevelingManager.WhenReady(levelManager => levelManager.OnLevelChanged += OnLevelUp);
    }

    private void OnDestroy()
    {
        if(ExperienceAndLevelingManager.Instance != null)
            ExperienceAndLevelingManager.Instance.OnLevelChanged -= OnLevelUp;
    }

    private void OnLevelUp(int level)
    {
        levelText.text = $"Level: {level}";
    }
}
