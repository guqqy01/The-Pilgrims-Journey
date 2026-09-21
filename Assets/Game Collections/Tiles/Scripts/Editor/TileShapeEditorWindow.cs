using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileShapeEditorWindow : EditorWindow
{
    Vector2 drag;
    float tileSeperation = 60f;
    float innerTileSizePercent = 0.95f;
    List<Vector2Int> listV2IntProp;
    SerializedProperty tilePicker;
    public static void Init(SerializedProperty v2IntProp)
    {
        TileShapeEditorWindow window = (TileShapeEditorWindow)EditorWindow.GetWindow(typeof(TileShapeEditorWindow));
        window.tilePicker = v2IntProp.Copy();
        v2IntProp.Next(true);
        if (v2IntProp.isArray)
        {
            v2IntProp.Next(true); // skip generic field
            v2IntProp.Next(true); // advance to array size field

            // Get the array size
            int arrayLength = v2IntProp.intValue;

            v2IntProp.Next(true); // advance to first array index

            // Write values to list
            List<Vector2Int> values = new List<Vector2Int>(arrayLength);
            int lastIndex = arrayLength - 1;
            for (int i = 0; i < arrayLength; i++)
            {
                values.Add(v2IntProp.vector2IntValue); // copy the value to the list
                if (i < lastIndex) v2IntProp.Next(false); // advance without drilling into children
            }

            window.listV2IntProp = values;
        }
        window.drag = window.position.center;
        window.Show();
    }

    public override void SaveChanges()
    {
        base.SaveChanges();

        SerializedProperty copy = tilePicker.Copy();

        copy.Next(true);

        if (copy.isArray)
        {
            copy.Next(true);
            copy.Next(true);

            copy.intValue = listV2IntProp.Count;
            copy.Next(true);
            int lastIndex = listV2IntProp.Count - 1;
            for (int i = 0; i < listV2IntProp.Count; i++)
            {
                copy.vector2IntValue = listV2IntProp[i];
                if (i < lastIndex) copy.Next(false);
            }
            tilePicker.serializedObject.ApplyModifiedProperties();
        }
    }


    void OnGUI()
    {
        Event e = Event.current;
        HandleDrag(e);
        Handles.BeginGUI();
        DrawTiles(e);
        Handles.EndGUI();
    }

    private void HandleDrag(Event e)
    {
        if (e.type == EventType.MouseDrag && e.button == 1)
        {
            drag += e.delta;
        }
    }

    private void DrawTiles(Event e)
    {
        Vector2Int min = PositionToV2Int(new Vector2(0, - position.height) + new Vector2(-drag.x, drag.y)) ;
        Vector2Int max = PositionToV2Int(new Vector2(position.width , 0) + new Vector2(-drag.x, drag.y));

        for (int i = min.x - 2; i < max.x + 2; i++)
        {
            for (int j = min.y - 2; j < max.y + 2; j++)
            {
                DrawTile(new Vector2Int(i, j), drag, e);
            }
        }
    }


    private Vector2Int PositionToV2Int(Vector2 point)
    {
        int x = Mathf.RoundToInt(point.x / (2f * tileSeperation));
        int y = Mathf.RoundToInt(point.y / (2f * tileSeperation));
        return new Vector2Int(x, y);
    }

    private void DrawTile(Vector2Int tilePosition, Vector2 offset, Event e)
    {
        float xPos = 2f * tileSeperation * tilePosition.x + offset.x;
        float yPos = -2f * tileSeperation * tilePosition.y + offset.y;

        float innerHexSize = tileSeperation * innerTileSizePercent;

        if (tilePosition == new Vector2Int())
        {
            Vector2[] centerHexRefPoints = GetPointsOnTile(new Vector2(xPos, yPos), tileSeperation);
            Handles.color = Color.blue;
            Handles.DrawAAConvexPolygon(centerHexRefPoints[0], centerHexRefPoints[1], centerHexRefPoints[2], centerHexRefPoints[3]);
        }

        Vector2[] points = GetPointsOnTile(new Vector2(xPos, yPos), innerHexSize);

        if (IsPointInTile(Event.current.mousePosition, points))
        {
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                if (listV2IntProp.Contains(tilePosition))
                {
                    listV2IntProp.Remove(tilePosition);
                }
                else
                {
                    listV2IntProp.Add(tilePosition);
                }
                hasUnsavedChanges = true;
                SaveChanges();
            }

            Handles.color = listV2IntProp.Contains(tilePosition) ? Color.green : Color.cyan;
        }
        else
        {
            Handles.color = listV2IntProp.Contains(tilePosition) ? Color.green : Color.white;
        }

        Handles.DrawAAConvexPolygon(points[0], points[1], points[2], points[3]);
        Handles.Label(new Vector3(xPos, yPos), tilePosition.ToString(), EditorStyles.centeredGreyMiniLabel);
        Repaint();
    }

    private Vector2[] GetPointsOnTile(Vector2 pos, float size)
    {
        Vector2 p1 = new Vector2(pos.x - size, pos.y - size);
        Vector2 p2 = new Vector2(pos.x - size, pos.y + size);
        Vector2 p3 = new Vector2(pos.x + size, pos.y + size);
        Vector2 p4 = new Vector2(pos.x + size, pos.y - size);

        return new Vector2[4] { p1, p2, p3, p4};
    }

    private bool IsPointInTile(Vector2 point, Vector2[] tilePoints)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < tilePoints.Length; i++)
        {
            if (tilePoints[i].x > maxX)
                maxX = tilePoints[i].x;

            if (tilePoints[i].y > maxY)
                maxY = tilePoints[i].y;

            if (tilePoints[i].x < minX)
                minX = tilePoints[i].x;

            if (tilePoints[i].y < minY)
                minY = tilePoints[i].y;
        }

        return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
    }
}
