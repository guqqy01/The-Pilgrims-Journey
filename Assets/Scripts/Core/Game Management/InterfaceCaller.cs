using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameManagerCalls
{
    internal static class InterfaceCaller
    {
        // Call interfaces safely. Exceptions in one handler won't prevent others from running.
        public static void CallInterfaces<T>(Action<T> action) where T : IGameManagerCallInterface
        {
            if (action == null) return;

            List<T> interfaces = InterfaceFinder.FindInterfaces<T>();

            if (interfaces == null || interfaces.Count == 0)
            {
                // No listeners found - that's fine. Avoid spamming logs in normal flow.
                return;
            }

            foreach (T interfaceInstance in interfaces)
            {
                if (interfaceInstance is UnityEngine.Object obj && obj == null) continue;
                try
                {
                    action(interfaceInstance);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.StackTrace);
                    Debug.LogError($"Exception while calling interface {typeof(T).Name} on {interfaceInstance}: {ex}");
                }
            }
        }
    }
}

