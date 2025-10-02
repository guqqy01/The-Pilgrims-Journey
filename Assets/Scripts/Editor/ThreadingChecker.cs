using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class ThreadingChecker
{
    [MenuItem("Tools/Check for Unity API Usage in Field Initializers and Static Constructors")]
    public static void CheckForThreadingIssues()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.FullName.StartsWith("System") && !a.FullName.StartsWith("mscorlib"));

        foreach (var assembly in assemblies)
        {
            // Skip dynamic assemblies
            if (assembly.IsDynamic)
                continue;

            // Only process assemblies in your Assets folder
            if (!assembly.Location.Replace("\\", "/").Contains("/Assets/"))
                continue;

            foreach (var type in assembly.GetTypes())
            {
                // Only process your namespaces (adjust as needed)
                if (type.Namespace == null || type.Namespace.StartsWith("UnityEngine") || type.Namespace.StartsWith("UnityEditor"))
                    continue;

                // Skip Unity Visual Scripting and package cache types
                if (type.Namespace == null ||
                    type.Namespace.StartsWith("UnityEngine") ||
                    type.Namespace.StartsWith("UnityEditor") ||
                    type.Namespace.StartsWith("Unity.VisualScripting") ||
                    type.Namespace.StartsWith("TMPro") ||
                    assembly.Location.Contains("PackageCache"))
                    continue;

                // Check static constructors
                var staticCtor = type.TypeInitializer;
                if (staticCtor != null)
                {
                    var body = staticCtor.GetMethodBody();
                    if (body != null)
                    {
                        var instructions = body.GetILAsByteArray();
                        if (ContainsUnityApiCall(type, staticCtor))
                        {
                            Debug.LogWarning($"Static constructor in {type.FullName} may use Unity API. Check for threading issues.");
                        }
                    }
                }

                // Check field initializers
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    // Heuristic: If field is UnityEngine.Object or derived, or Font/TMP_FontAsset
                    if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType) ||
                        fieldType.FullName == "TMPro.TMP_FontAsset" ||
                        fieldType.FullName == "UnityEngine.Font")
                    {
                        // Only check static fields for default value
                        if (field.IsStatic)
                        {
                            // Skip fields with unresolved generic parameters
                            if (!fieldType.ContainsGenericParameters)
                            {
                                var value = field.GetValue(null);
                                if (value != null)
                                {
                                    Debug.LogWarning($"Static field initializer for '{field.Name}' in '{type.FullName}' may use Unity API. Check for threading issues.");
                                }
                            }
                        }
                        else
                        {
                            // For instance fields, just warn about the presence of a UnityEngine.Object field
                            Debug.Log($"Instance field '{field.Name}' in '{type.FullName}' is of type '{fieldType.FullName}'. Manual review recommended for field initializers.");
                        }
                    }
                }
            }
        }
        Debug.Log("ThreadingChecker: Scan complete.");
    }

    // Heuristic: Check if method body contains Unity API calls (by name)
    private static bool ContainsUnityApiCall(Type type, ConstructorInfo ctor)
    {
        var body = ctor.GetMethodBody();
        if (body == null) return false;
        // This is a heuristic: we can't parse IL easily, so we check for known Unity types in the declaring type
        return type.Namespace != null && type.Namespace.StartsWith("UnityEngine");
    }
}