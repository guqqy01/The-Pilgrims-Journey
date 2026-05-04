using AYellowpaper.SerializedCollections;
using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class SubDescriptionUIHelper : Singleton<SubDescriptionUIHelper>
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

    public bool TryGetSubDescriptionUIFromType(Type type, out SubDescriptionUI subDescriptionUI)
    {
        if (runtimeLookup == null)
            Setup();

        return runtimeLookup.TryGetValue(type, out subDescriptionUI);
    }

}
