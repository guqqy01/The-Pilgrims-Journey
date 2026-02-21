using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Info
{
    public static IDescriptable CurrentlySelected;

    public delegate void OnInfoSelected(IDescriptable info);
    public static OnInfoSelected onInfoSelected { get; set; }

    public static void Select(IDescriptable info)
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
