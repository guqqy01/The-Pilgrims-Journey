using System.IO;
using UnityEditor;
using UnityEngine;

public class GetSpritesFromFolders : MonoBehaviour
{
    [MenuItem("Tools/Collect PNGs Into Sprites Folder")]
    private static void CollectPNGs()
    {
        // Ask user to select a folder
        string selectedPath = EditorUtility.OpenFolderPanel("Select Root Folder", "Assets", "");
        if (string.IsNullOrEmpty(selectedPath))
            return;

        // Ensure path is inside Assets
        if (!selectedPath.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder inside the Assets directory.", "OK");
            return;
        }

        // Create "Sprites" folder inside the selected folder
        string spritesFolder = Path.Combine(selectedPath, "Sprites");
        if (!Directory.Exists(spritesFolder))
            Directory.CreateDirectory(spritesFolder);

        // Find all PNGs recursively
        string[] pngFiles = Directory.GetFiles(selectedPath, "*.png", SearchOption.AllDirectories);

        int movedCount = 0;
        foreach (string filePath in pngFiles)
        {
            // Skip files already in Sprites folder
            if (filePath.StartsWith(spritesFolder))
                continue;

            // Get the parent folder name
            string parentFolder = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;

            // New file name = parent folder name + ".png"
            string newFileName = parentFolder + ".png";
            string destPath = Path.Combine(spritesFolder, newFileName);

            // Handle duplicates by appending a counter
            int counter = 1;
            while (File.Exists(destPath))
            {
                destPath = Path.Combine(spritesFolder, $"{parentFolder}_{counter}.png");
                counter++;
            }

            File.Move(filePath, destPath); // Move instead of copy
            movedCount++;
        }

        // Refresh AssetDatabase so Unity sees new files
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Moved and renamed {movedCount} PNG(s) into {spritesFolder}", "OK");
    }


}
