using UnityEngine;
using System.Collections.Generic;
using System;

public class Tab : MonoBehaviour
{
    public List<GameObject> subTabs;

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Open(GameObject subTab)
    {
        gameObject.SetActive(true);

        foreach(GameObject sTab in subTabs)
        {
            sTab.SetActive(sTab == subTab);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}

[Serializable]
public class TabReference
{
    public Tab tab;
    public int subTabIndex = -1;

    public void Open()
    {
        tab.gameObject.SetActive(true);

        if (subTabIndex >= 0 && tab.subTabs.Count > 0)
        {
            tab.Open(tab.subTabs[subTabIndex]);
        }
    }

    public void Close()
    {
        tab.Close();
    }
}
