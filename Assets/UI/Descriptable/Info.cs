using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Info
{
    public static IDescriptableUI CurrentlySelected;

    public delegate void OnInfoSelected(IDescriptableUI info);
    public static OnInfoSelected onInfoSelected { get; set; }

    public static void Select(IDescriptableUI info)
    {
        CurrentlySelected?.Deselect();

        CurrentlySelected = info;

        CurrentlySelected.Select();

        onInfoSelected?.Invoke(CurrentlySelected);
    }

    public static void Dispose()
    {
        onInfoSelected = null;
    }
}
