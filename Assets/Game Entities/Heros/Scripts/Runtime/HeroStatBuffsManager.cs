namespace Heros
{
    public class HeroStatBuffsManager : StatBuffsManager, IToBeInitiatedFromRoot
    {
        private static readonly SingletonHelper<HeroStatBuffsManager> _singleton = new();
        public static HeroStatBuffsManager Instance => _singleton.Instance;
        public static bool HasInstance => _singleton.HasInstance;

        public void InitiateFromRootAtAwake()
        {
            if (!_singleton.TryClaim(this, () => Destroy(gameObject)))
                return;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _singleton.Release(this);
        }
    }
}


