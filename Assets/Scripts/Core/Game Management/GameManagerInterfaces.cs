namespace GameManagerCalls
{
    public interface IGameManagerCallInterface 
    {

    }

    public interface IOnPlayLocalLevel : IGameManagerCallInterface
    {
        public void OnPlayLocalLevel();
    }

    public interface IOnCompletedLevel : IGameManagerCallInterface
    {
        public void OnLevelCompleted();
    }

    public interface IOnGameEnd : IGameManagerCallInterface
    {
        public void OnGameEnd(string message);
    }

    public interface IOnBeforeCreateLocalMap : IGameManagerCallInterface
    {
        public void OnBeforeCreateLocalMap();
    }

    public interface IOnCreateLocalMap : IGameManagerCallInterface
    {
        public void OnCreateLocalMap();
    }

    public interface IOnAfterCreateLocalMap : IGameManagerCallInterface
    {
        public void AfterCreateLocalMap();
    }

    public interface IOnCreateCurrentMapPoint : IGameManagerCallInterface
    {
        public void OnCreateCurrentMapPoint();
    }

    public interface IOnGenerateNewRandomEvent : IGameManagerCallInterface
    {
        public void OnGenerateNewRandomEvent();
    }

    public interface IOnCompleteLocalMapLevel : IGameManagerCallInterface
    {
        public void OnCompleteLocalMapLevel();
    }

    public interface IOnClear : IGameManagerCallInterface
    {
        public void Clear();
    }

    public interface IOnSetupNewGame : IGameManagerCallInterface
    {
        public void OnSetupNewGame();
    }

    public interface IOnLoadAfterMapPointSceneLoad : IGameManagerCallInterface
    {
        public void LoadAfterMapPointSceneLoad();
    }

    public interface IOnLoad : IGameManagerCallInterface
    {
        public void Load();
    }

    public interface IOnSave : IGameManagerCallInterface
    {
        public void Save();
    }

    public interface IOnDelete : IGameManagerCallInterface
    {
        public void Delete();
    }

    public interface IOnUnitPassed : IGameManagerCallInterface
    {
        public void OnUnitPassed(int totalPassed, int requiredToPass);
    }
}

