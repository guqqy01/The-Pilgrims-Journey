using System.Collections.Generic;
namespace GameManagerCalls
{
    // Central registry that components can optionally register with for deterministic discovery.
    public static class InterfaceRegistry<T> where T : IGameManagerCallInterface
    {
        private static readonly List<T> registered = new List<T>();

        public static void Register(T item)
        {
            if (item == null) return;
            if (registered.Contains(item)) return;
            registered.Add(item);
        }

        public static void Unregister(T item)
        {
            if (item == null) return;
            registered.Remove(item);
        }

        public static IReadOnlyList<T> GetAll() => registered.AsReadOnly();

        public static bool HasAny => registered.Count > 0;

        public static void Clear() => registered.Clear();
    }
}

