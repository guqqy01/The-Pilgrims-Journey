using UnityEngine;
using UnityEditor;
using Map;
using UnityEngine.UIElements;

[CustomEditor(typeof(GlobalMapGenerationSettings))]
public class GlobalMapGenerationSettingsEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        // Draw all the serializable fields first
        root.Add(new IMGUIContainer(() => DrawDefaultInspector()));

        var settings = (GlobalMapGenerationSettings)target;
        for (int i = 0; i < settings.PathSettings.Count; i++)
        {
            var s = settings.PathSettings[i];

            // Label for clarity
            var lbl = new Label($"Element {i} Direction Tendency:");
            lbl.style.marginTop = 10;
            root.Add(lbl);

            // Add our curve‐rasterizing view
            var view = new CurveTextureView(
                new[]
                {
                    s.directionTendency.curveMin,
                    s.directionTendency.curveMax
                },
                new[] { Color.red, Color.green }
            );
            view.style.marginBottom = 10;
            root.Add(view);
        }

        return root;
    }
}
