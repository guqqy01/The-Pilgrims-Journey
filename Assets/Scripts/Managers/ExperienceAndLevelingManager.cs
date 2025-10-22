using UnityEditor.Rendering;
using UnityEngine;

public class ExperienceAndLevelingManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static ExperienceAndLevelingManager Instance { private set; get; }

    public int Level { private set; get; } = 0;
    public float CurrentExperience { private set; get; }
    public float ExperienceNeededToLevel => Level * 10f + 10f;

    public delegate void OnUpdateUI();
    public OnUpdateUI onUpdateUI;

    public void InitiateSingleton()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Awake()
    {
        LocalMapLevelCompletedManager.onUnitPassed += OnUnitPassed;
    }

    private void OnDestroy()
    {
        LocalMapLevelCompletedManager.onUnitPassed -= OnUnitPassed;
    }

    public void LevelUp()
    {
        Level++;
        CurrentExperience = 0;
        LevelingUpRewardManager.Instance.SetupRelicsChoices();
    }

    private void OnUnitPassed(int x, int y)
    {
        CurrentExperience = CurrentExperience + OverallBuffManager.Instance.ExperienceGain;

        if (CurrentExperience >= ExperienceNeededToLevel)
        {
            LevelUp();
        }

        onUpdateUI?.Invoke();
    }
}
