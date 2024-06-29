using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string saveFolder = Application.persistentDataPath + "/GameData";

    public static bool FileExists(string profileName)
    {
        return File.Exists($"{saveFolder}/{profileName}");
    }

    public static void Delete(string profileName)
    {
        if (!FileExists(profileName))
            throw new System.Exception($"Save Profile {profileName} does not exist.");

        File.Delete($"{saveFolder}/{profileName}");
    }

    public static void IfFileExistsDelete(string profileName)
    {
        if (FileExists(profileName))
        {
            File.Delete($"{saveFolder}/{profileName}");
        }
    }

    public static SaveProfile<T> Load<T>(string profileName) where T : SaveProfileData
    {
        if(!FileExists(profileName))
            throw new System.Exception($"Save Profile {profileName} does not exist.");

        var fileContents = File.ReadAllText($"{saveFolder}/{profileName}");

        //decrypt

        return JsonUtility.FromJson<SaveProfile<T>>(fileContents);
    }

    public static bool TryLoad<T>(string profileName, out SaveProfile<T> outSaveProfile) where T : SaveProfileData
    {
        outSaveProfile = null;

        if (!FileExists(profileName))
            return false;

        var fileContents = File.ReadAllText($"{saveFolder}/{profileName}");
        //decrypt
        outSaveProfile = JsonUtility.FromJson<SaveProfile<T>>(fileContents);

        return true;
    }

    public static void Save<T>(SaveProfile<T> save) where T : SaveProfileData
    {
        if (FileExists(save.profileName))
            throw new System.Exception($"Save Profile {save.profileName} already exists!");

        var jsonString = JsonUtility.ToJson(save, true);

        //encrypt

        if(!Directory.Exists(saveFolder)) //create / GameData if it's not already there.
            Directory.CreateDirectory(saveFolder);

        File.WriteAllText($"{saveFolder}/{save.profileName}", jsonString);
    }

    public static void SaveOver<T>(SaveProfile<T> save) where T : SaveProfileData
    {
        IfFileExistsDelete($"{saveFolder}/{save.profileName}");

        var jsonString = JsonUtility.ToJson(save, true);

        //encrypt

        if (!Directory.Exists(saveFolder)) //create / GameData if it's not already there.
            Directory.CreateDirectory(saveFolder);

        File.WriteAllText($"{saveFolder}/{save.profileName}", jsonString);
    }
}
