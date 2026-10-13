using UnityEngine;
namespace GameManagerCalls
{
    /// <summary>
    /// One-liner entry points for every IGameManagerCallInterface.
    /// Example: GameCalls.Save(); GameCalls.GameEnd("You win");
    /// Each method forwards to InterfaceCaller, so exceptions in one handler
    /// are logged and don't stop the others.
    /// </summary>
    public static class GameCalls
    {
        // Level flow
        public static void PlayLocalLevel() =>
            InterfaceCaller.CallInterfaces<IOnPlayLocalLevel>(i => i.OnPlayLocalLevel());

        public static void LevelCompleted() =>
            InterfaceCaller.CallInterfaces<IOnCompletedLevel>(i => i.OnLevelCompleted());

        public static void CompleteLocalMapLevel() =>
            InterfaceCaller.CallInterfaces<IOnCompleteLocalMapLevel>(i => i.OnCompleteLocalMapLevel());

        public static void UnitPassed(int totalPassed, int requiredToPass) =>
            InterfaceCaller.CallInterfaces<IOnUnitPassed>(i => i.OnUnitPassed(totalPassed, requiredToPass));

        public static void GameEnd(string message) =>
            InterfaceCaller.CallInterfaces<IOnGameEnd>(i => i.OnGameEnd(message));

        // Map creation
        public static void BeforeCreateLocalMap() =>
            InterfaceCaller.CallInterfaces<IOnBeforeCreateLocalMap>(i => i.OnBeforeCreateLocalMap());

        public static void CreateLocalMap() =>
            InterfaceCaller.CallInterfaces<IOnCreateLocalMap>(i => i.OnCreateLocalMap());

        public static void AfterCreateLocalMap() =>
            InterfaceCaller.CallInterfaces<IOnAfterCreateLocalMap>(i => i.AfterCreateLocalMap());

        public static void CreateCurrentMapPoint()
        {
            InterfaceCaller.CallInterfaces<IOnCreateCurrentMapPoint>(i => i.OnCreateCurrentMapPoint());
        }

        public static void GenerateNewRandomEvent() =>
            InterfaceCaller.CallInterfaces<IOnGenerateNewRandomEvent>(i => i.OnGenerateNewRandomEvent());

        // Game lifecycle
        public static void SetupNewGame() =>
            InterfaceCaller.CallInterfaces<IOnSetupNewGame>(i => i.OnSetupNewGame());

        public static void Clear() =>
            InterfaceCaller.CallInterfaces<IOnClear>(i => i.Clear());

        // Persistence
        public static void Load() =>
            InterfaceCaller.CallInterfaces<IOnLoad>(i => i.Load());

        public static void LoadAfterMapPointSceneLoad() =>
            InterfaceCaller.CallInterfaces<IOnLoadAfterMapPointSceneLoad>(i => i.LoadAfterMapPointSceneLoad());

        public static void Save() =>
            InterfaceCaller.CallInterfaces<IOnSave>(i => i.Save());

        public static void Delete() =>
            InterfaceCaller.CallInterfaces<IOnDelete>(i => i.Delete());
    }
}

