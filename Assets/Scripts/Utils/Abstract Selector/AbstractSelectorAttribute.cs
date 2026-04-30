using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// Add to a field which has a [SerializedReference] attribute to allow the user to select between the derived types in the inspector
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class AbstractSelectorAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AbstractTitle : PropertyAttribute 
{
    public string Title { get; set; }
    public Type Type { get; set; }

    public AbstractTitle(string title)
    {
        Title = title; 
    }

    public AbstractTitle(string title, Type type)
    {
        Title = title;
        Type = type;
    }
}

