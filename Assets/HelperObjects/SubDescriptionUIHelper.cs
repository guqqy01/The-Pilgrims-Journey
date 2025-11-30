using AYellowpaper.SerializedCollections;
using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "SubDescriptionUIHelper", menuName = "Helper/Sub Description UI Helper")]
public class SubDescriptionUIHelper : ScriptableObject
{
    [SerializeField]
    private SerializedDictionary<TypeReference, SubDescriptionUI> TypeReferenceAndItsSubDescriptionUI;

    private Dictionary<Type, SubDescriptionUI> runtimeLookup;

    private void OnEnable()
    {
        Setup();
    }

    private void Setup()
    {
        runtimeLookup = TypeReferenceAndItsSubDescriptionUI
            .Where(x => x.Key.Type != null) // defensive: skip nulls
            .ToDictionary(x => x.Key.Type, x => x.Value);
    }

    public SubDescriptionUI GetSubDescriptionUIFromType(Type type)
    {
        if (runtimeLookup == null)
            Setup();

        runtimeLookup.TryGetValue(type, out var ui);
        return ui;
    }

}
