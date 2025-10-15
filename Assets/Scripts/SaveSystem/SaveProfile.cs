using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SaveProfile<T> where T : SaveProfileData
{
    public string profileName;
    public T saveData;

    private SaveProfile() { }

    public SaveProfile(string name, T saveData)
    {
        this.profileName = name;
        this.saveData = saveData;
    }
}

[Serializable]
public abstract record SaveProfileData { }


public interface ISaveLoad
{
    public string SaveFileName { get; }

    public virtual void Load() { }
    public void Save();
}