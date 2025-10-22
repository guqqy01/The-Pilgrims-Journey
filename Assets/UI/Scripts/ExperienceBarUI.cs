using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
public class ExperienceBarUI : MonoBehaviour
{
    [SerializeField]
    private Image experienceBarFill;

    private void OnEnable()
    {
        ExperienceAndLevelingManager.Instance.onUpdateUI -= OnUpdateExperienceBar;
        ExperienceAndLevelingManager.Instance.onUpdateUI += OnUpdateExperienceBar;
        OnUpdateExperienceBar();
    }

    private void OnDisable()
    {
        if(ExperienceAndLevelingManager.Instance != null)
            ExperienceAndLevelingManager.Instance.onUpdateUI -= OnUpdateExperienceBar;
    }

    public void OnUpdateExperienceBar()
    {
        ExperienceAndLevelingManager manager = ExperienceAndLevelingManager.Instance;

        float fillAmount = Mathf.Clamp01((float)manager.CurrentExperience / (float)manager.ExperienceNeededToLevel);

        experienceBarFill.fillAmount = fillAmount;
    }
}
