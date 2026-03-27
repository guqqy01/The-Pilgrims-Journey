using UnityEngine;
using UnityEditor;

public class VerticalSeparator
{
    private float _topRectHeight;

    private float minHeight = 100f;

    private float mouseClickHeight = 5f;

    private bool dragging;

    public Rect topRect;

    public Rect bottomRect;



    public VerticalSeparator(Rect wholeRect)
    {
        _topRectHeight = wholeRect.height / 2f;

        DoSeperator(wholeRect);
    }

    public bool DoSeperator(Rect wholeRect)
    {
        bool repaint = false;

        Event e = Event.current;

        Rect mouseClickRect = new Rect(wholeRect) { yMin = wholeRect.yMin + _topRectHeight - mouseClickHeight / 2f, height = mouseClickHeight };

        EditorGUIUtility.AddCursorRect(mouseClickRect, MouseCursor.ResizeVertical);

        if (mouseClickRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                dragging = true;
            }
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            dragging = false;
        }

        if (dragging)
        {
            _topRectHeight = e.mousePosition.y - wholeRect.yMin;
            repaint = true;
        }

        _topRectHeight = Mathf.Clamp(_topRectHeight, minHeight, wholeRect.height - minHeight);

        topRect = new Rect(wholeRect) { height = _topRectHeight};
        bottomRect = new Rect(wholeRect) { yMin = topRect.yMax };

        Handles.BeginGUI();

        Color handlesColor = Handles.color;
        Handles.color = Color.black;
        Vector2 left = new Vector2(wholeRect.xMin, wholeRect.yMin + _topRectHeight);
        Vector2 right = new Vector2(wholeRect.xMax, wholeRect.yMin + _topRectHeight);

        Handles.DrawLine(left, right);
        Handles.color = handlesColor;
        Handles.EndGUI();

        return repaint;
    }
}