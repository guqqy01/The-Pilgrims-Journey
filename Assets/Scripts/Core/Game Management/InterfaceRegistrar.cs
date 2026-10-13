using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;
namespace GameManagerCalls
{
    public static class InterfaceRegistrar
    {
        private struct Entry
        {
            public Action<object> Register;
            public Action<object> Unregister;
        }

        private static readonly Dictionary<Type, Entry[]> cache = new Dictionary<Type, Entry[]>();

        private static readonly MethodInfo registerMethod =
            typeof(InterfaceRegistrar).GetMethod(nameof(RegisterTyped), BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo unregisterMethod =
            typeof(InterfaceRegistrar).GetMethod(nameof(UnregisterTyped), BindingFlags.NonPublic | BindingFlags.Static);

        [Preserve]
        private static void RegisterTyped<T>(object target) where T : IGameManagerCallInterface
            => InterfaceRegistry<T>.Register((T)target);

        [Preserve]
        private static void UnregisterTyped<T>(object target) where T : IGameManagerCallInterface
            => InterfaceRegistry<T>.Unregister((T)target);

        private static Entry[] GetEntries(Type type)
        {
            if (cache.TryGetValue(type, out var entries))
                return entries;

            var list = new List<Entry>();
            foreach (Type iface in type.GetInterfaces())
            {
                if (iface == typeof(IGameManagerCallInterface)) continue;
                if (!typeof(IGameManagerCallInterface).IsAssignableFrom(iface)) continue;

                list.Add(new Entry
                {
                    Register = (Action<object>)Delegate.CreateDelegate(
                        typeof(Action<object>), registerMethod.MakeGenericMethod(iface)),
                    Unregister = (Action<object>)Delegate.CreateDelegate(
                        typeof(Action<object>), unregisterMethod.MakeGenericMethod(iface)),
                });
            }

            entries = list.ToArray();
            cache[type] = entries;
            return entries;
        }

        public static void Register(object target)
        {
            if (target == null) return;
            foreach (var entry in GetEntries(target.GetType()))
                entry.Register(target);
        }

        public static void Unregister(object target)
        {
            if (target == null) return;
            foreach (var entry in GetEntries(target.GetType()))
                entry.Unregister(target);
        }
    }
}

