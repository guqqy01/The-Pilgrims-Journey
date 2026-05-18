using GameManagerCalls;
using GlobalMap;
using UnityEngine;

public class DifficultyLevelManager : Singleton<DifficultyLevelManager>, IOnSetupNewGame, IOnClear
{
    [SerializeField]
    private float levelDifficultyMultiplier = 1f;
    [HideInInspector]
    public BuffableStat LevelDifficultyMultiplier;

    [SerializeField]
    private float horizontalDistanceDifficultyMultiplier = 1f;
    [HideInInspector]
    public BuffableStat HorizontalDistanceDifficultyMultiplier;

    [SerializeField]
    private float overallDifficultyMultiplier = 1f;
    [HideInInspector]
    public BuffableStat OverallDifficultyMultiplier;

    public float DifficultyLevel
    {
        get
        {
            return (GameManager.Instance.Level * LevelDifficultyMultiplier.Value 
                + GlobalMapGenerator.Instance.CurrentPosition.x * HorizontalDistanceDifficultyMultiplier.Value) * OverallDifficultyMultiplier.Value;
        }
    }

    public void Clear()
    {
        OnSetupNewGame();
    }

    public override void InitiateFromRootAtAwake()
    {
        base.InitiateFromRootAtAwake();

        LevelDifficultyMultiplier = new BuffableStat(levelDifficultyMultiplier);
        HorizontalDistanceDifficultyMultiplier = new BuffableStat(horizontalDistanceDifficultyMultiplier);
        OverallDifficultyMultiplier = new BuffableStat(overallDifficultyMultiplier);

    }

    public void OnSetupNewGame()
    {
        LevelDifficultyMultiplier.ClearAllBuffs();
        HorizontalDistanceDifficultyMultiplier.ClearAllBuffs();
        OverallDifficultyMultiplier.ClearAllBuffs();
    }
}
