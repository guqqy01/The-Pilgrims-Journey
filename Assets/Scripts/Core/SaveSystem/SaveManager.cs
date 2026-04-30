using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    // Paths built with Path.Combine to be cross-platform.
    private static readonly string gameData = Path.Combine(Application.persistentDataPath, "GameData");
    private static readonly string saveFolder = Path.Combine(gameData, "SaveFolder");
    private static readonly string fileExtension = ".json";

    // Simple lock for thread-safety (prevents concurrent writes).
    private static readonly object fileLock = new object();

    // Encryption password (if set, encryption/decryption is used).
    private static string encryptionPassword = null;
    public static bool EncryptionEnabled => !string.IsNullOrEmpty(encryptionPassword);

    public static void EnableEncryption(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        encryptionPassword = password;
    }

    public static void DisableEncryption() => encryptionPassword = null;

    // True if save folder exists (and can contain saves).
    public static bool SaveFolderExists => Directory.Exists(saveFolder);

    // True if a specific profile file exists.
    public static bool FileExists(string profileName)
        => File.Exists(GetFilePath(profileName));

    // Delete all saves (original DeleteSaveFile had ambiguous behavior).
    public static void DeleteAllSaves()
    {
        lock (fileLock)
        {
            try
            {
                if (Directory.Exists(saveFolder))
                    Directory.Delete(saveFolder, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save folder '{saveFolder}': {ex}");
            }
        }
    }

    // Delete a specific profile file if it exists.
    public static void IfFileExistsDelete(string profileName)
    {
        lock (fileLock)
        {
            var path = GetFilePath(profileName);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save file '{path}': {ex}");
            }
        }
    }

    // Try to load a save; returns false on any error (safe for callers).
    public static bool TryLoad<T>(string profileName, out SaveProfile<T> outSaveProfile) where T : SaveProfileData
    {
        outSaveProfile = null;
        var path = GetFilePath(profileName);

        if (!File.Exists(path))
            return false;

        try
        {
            string fileContents;
            // Keep file read under lock to avoid concurrent write/read races.
            lock (fileLock)
            {
                fileContents = File.ReadAllText(path);
            }

            if (EncryptionEnabled)
            {
                try
                {
                    fileContents = AesEncryption.DecryptStringFromBase64(fileContents, encryptionPassword);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Decryption failed for '{path}': {ex}");
                    return false;
                }
            }

            outSaveProfile = JsonUtility.FromJson<SaveProfile<T>>(fileContents);
            return outSaveProfile != null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load save '{path}': {ex}");
            outSaveProfile = null;
            return false;
        }
    }

    // Try saving. Returns true on success. If overwrite is false and file exists, returns false.
    public static bool TrySave<T>(SaveProfile<T> save, bool overwrite = false) where T : SaveProfileData
    {
        if (save == null)
            throw new ArgumentNullException(nameof(save));

        var path = GetFilePath(save.profileName);

        var jsonString = JsonUtility.ToJson(save, true);
        string outputText = jsonString;

        if (EncryptionEnabled)
        {
            try
            {
                outputText = AesEncryption.EncryptStringToBase64(jsonString, encryptionPassword);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Encryption failed for profile '{save.profileName}': {ex}");
                return false;
            }
        }

        lock (fileLock)
        {
            if (!overwrite && File.Exists(path))
            {
                Debug.LogWarning($"Save profile '{save.profileName}' already exists. Use overwrite to replace it.");
                return false;
            }

            try
            {
                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);

                var tempPath = path + ".tmp";

                // Write to temp file first, then atomically replace.
                File.WriteAllText(tempPath, outputText);

                if (File.Exists(path))
                {
                    // Use File.Replace on .NET Framework to replace atomically when possible.
                    try
                    {
                        File.Replace(tempPath, path, null);
                    }
                    catch
                    {
                        // If Replace fails for any reason, fallback to overwrite via Delete+Move.
                        if (File.Exists(path))
                            File.Delete(path);
                        File.Move(tempPath, path);
                    }
                }
                else
                {
                    File.Move(tempPath, path);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save profile '{save.profileName}' to '{path}': {ex}");
                return false;
            }
        }
    }

    // Convenience wrapper to force overwrite.
    public static bool SaveOver<T>(SaveProfile<T> save) where T : SaveProfileData
        => TrySave(save, overwrite: true);

    // Helpers
    private static string GetFilePath(string profileName)
        => Path.Combine(saveFolder, SanitizeProfileName(profileName) + fileExtension);

    // Removes path components to avoid path traversal and invalid characters.
    private static string SanitizeProfileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Profile name cannot be null or whitespace.", nameof(name));

        // Keep only file name portion and remove any invalid filename chars.
        var fileName = Path.GetFileName(name);
        foreach (var c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c.ToString(), string.Empty);

        return fileName;
    }
}
