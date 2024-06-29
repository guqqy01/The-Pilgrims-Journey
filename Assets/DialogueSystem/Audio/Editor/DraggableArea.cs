using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using static UnityEditor.Rendering.FilterWindow;
using Codice.Client.BaseCommands;
using UnityEditor.XR;
using System.Linq;
using static UnityEditor.Experimental.GraphView.GraphView;
using static DraggableArea;

public class DraggableArea
{
    public Rect areaRect;

    protected readonly List<DraggableRect> draggableRects = new List<DraggableRect>();
    protected readonly SortedDictionary<int, List<DraggableRect>> layerAndDrawOrders = new SortedDictionary<int, List<DraggableRect>>();

    public Color backgroundColorInner = Color.grey;
    public Color backgroundColorOuter = Color.black;

    public Vector2 defaultDraggableRectSizes = new Vector2(10f, 10f);
    public Color defaultInnerColor = Color.grey;
    public Color defaultOuterColor = Color.black;
    public bool defaultIgnoreOtherDraggableRects;
    public bool fillOtherAxis;

    public bool DraggingRect { private set; get; }
    public DraggableRect currentlyDraggingRect;

    public List<Rect> avaliableRectsForDraggedRectToMoveWithin = new List<Rect>();

    public delegate void BeforeDraw(Rect rect);
    public BeforeDraw beforeDraw;

    public delegate void AfterDraw(Rect rect);
    public AfterDraw afterDraw;

    public delegate void BeforeDrag();
    public BeforeDrag beforeDrag;
    public delegate void AfterDrag();
    public AfterDrag afterDrag;

    protected delegate void ProtectedOnDraggingRect();
    protected ProtectedOnDraggingRect protectedOnDraggingRect;

    public Dictionary<string, GenericMenu.MenuFunction> rightClickSpaceOptions = new Dictionary<string, GenericMenu.MenuFunction>();
    public Dictionary<string, GenericMenu.MenuFunction> rightClickRectOptions = new Dictionary<string, GenericMenu.MenuFunction>();

    public delegate void OnRightClickRect(DraggableRect rectClicked);
    public OnRightClickRect onRightClickRect;

    public bool rightClickedItem;
    public Vector2 positionClicked;
    public DraggableRect rectRightClicked;

    public bool limitXPositionToRect = true;
    public bool limitYPositionToRect = true;

    public DraggableArea() { }

    public DraggableRect this[int i]
    {
        get
        {
            return draggableRects[i];
        }
    }

    public List<DraggableRect> GetDraggableRects()
    {
        return draggableRects;
    }

    public List<DraggableRect> GetDraggableRects(int layer)
    {
        if(layerAndDrawOrders.ContainsKey(layer))
            return layerAndDrawOrders[layer];

        return new List<DraggableRect>();
    }

    public virtual void HandleDraggableRects(Event e)
    {
        HandleDrawRectList();

        HandleBeforeDrag();

        HandleEndDrag(e);
        HandleStartDrag(e);
        HandleDrag(e);

        HandleAfterDrag();

        HandleRightClick(e);

        DrawBackground();

        beforeDraw?.Invoke(areaRect);

        if (DraggingRect)
        {
            protectedOnDraggingRect?.Invoke();
            GUI.changed = true;
        }

        DrawRects(e);

        afterDraw?.Invoke(areaRect);
    }

    private void HandleBeforeDrag()
    {
        beforeDrag?.Invoke();

        foreach(DraggableRect rect in draggableRects)
        {
            rect.BeforeDrag();
        }
    }

    private void HandleAfterDrag()
    {
        afterDrag?.Invoke();

        foreach (DraggableRect rect in draggableRects)
        {
            rect.AfterDrag();
        }
    }

    private void DrawBackground()
    {
        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(areaRect, backgroundColorInner, backgroundColorOuter);
    }

    private void HandleRightClick(Event e)
    {
        if (e.type != EventType.MouseDown || e.button != 1)
            return;

        rightClickedItem = false;

        positionClicked = e.mousePosition;

        foreach(var layerAndDrawOrder in layerAndDrawOrders)
        {
            foreach (DraggableRect rect in layerAndDrawOrder.Value)
            {
                if (rect.rect.Contains(e.mousePosition))
                {
                    rightClickedItem = true;
                    rectRightClicked = rect;

                    onRightClickRect?.Invoke(rect);

                    GenericMenu menu = new GenericMenu();
                    foreach (KeyValuePair<string, GenericMenu.MenuFunction> kvp in rightClickRectOptions)
                    {
                        menu.AddItem(new GUIContent(kvp.Key), false, kvp.Value);
                    }

                    if (menu.GetItemCount() > 0)
                    {
                        menu.ShowAsContext();
                        e.Use();
                    }
                    break;
                }
            }
        }
        
        if (!rightClickedItem && areaRect.Contains(e.mousePosition))
        {
            GenericMenu menu = new GenericMenu();
            foreach (KeyValuePair<string, GenericMenu.MenuFunction> kvp in rightClickSpaceOptions)
            {
                menu.AddItem(new GUIContent(kvp.Key), false, kvp.Value);
            }

            if(menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
                e.Use();
            }
        }
    }

    private void HandleDrawRectList()
    {
        List<DraggableRect> toRemove = new List<DraggableRect>();
        foreach (var drawOrder in layerAndDrawOrders.Values.ToList())
        {
            foreach (var rect in drawOrder)
            {
                if (!draggableRects.Contains(rect))
                    toRemove.Add(rect);
            }

            foreach (var rect in toRemove)
            {
                drawOrder.Remove(rect);
            }
        }

        foreach (DraggableRect draggableRect in draggableRects)
        {
            bool foundRectInDrawOrder = false;

            foreach (var drawOrder in layerAndDrawOrders.Values.ToList())
            {
                if (drawOrder.Contains(draggableRect))
                {
                    foundRectInDrawOrder = true;
                    break;
                }
            }

            if (!foundRectInDrawOrder)
            {
                if (layerAndDrawOrders.ContainsKey(0))
                {
                    layerAndDrawOrders[0].Add(draggableRect);
                }
                else
                {
                    layerAndDrawOrders.Add(0, new List<DraggableRect>() { draggableRect });
                }
            }
        }
    }

    public void Clear(int layer)
    {
        if (!layerAndDrawOrders.ContainsKey(layer))
            return;

        foreach(DraggableRect draggableRect in layerAndDrawOrders[layer])
        {
            draggableRects.Remove(draggableRect);
        }
    }

    public void Add(int layer, DraggableRect draggableRect)
    {
        draggableRects.Add(draggableRect);
        if (layerAndDrawOrders.ContainsKey(layer))
        {
            layerAndDrawOrders[layer].Add(draggableRect);
        }
        else
        {
            layerAndDrawOrders.Add(layer, new List<DraggableRect>() { draggableRect});
        }
    }

    public void Remove(DraggableRect draggableRect)
    {
        draggableRects.Remove(draggableRect);
    }

    private void DrawRects(Event e)
    {
        foreach (var layerAndDrawOrder in layerAndDrawOrders)
        {
            foreach (DraggableRect draggableRect in layerAndDrawOrder.Value)
            {
                if (draggableRect == null)
                    continue;

                draggableRect.DrawRect(e);
            }
        }
    }

    private void HandleStartDrag(Event e)
    {
        if (e.type != EventType.MouseDown || e.button != 0)
            return;

        List<List<DraggableRect>> drawOrders = layerAndDrawOrders.Values.ToList();
        drawOrders.Reverse();

        for (int i = 0; i < drawOrders.Count; i++)
        {
            List<DraggableRect> drawOrder = drawOrders[i];

            drawOrder.Reverse();

            for (int j = 0; j < drawOrder.Count; j++)
            {
                if (drawOrder[j].HandleStartDrag(e))
                {
                    DraggingRect = true;
                    currentlyDraggingRect = drawOrder[j];
                    currentlyDraggingRect.dragging = true;
                    e.Use();
                    break;
                }
            }

            drawOrder.Reverse();

            if (DraggingRect)
            {
                drawOrder.Remove(currentlyDraggingRect);
                drawOrder.Add(currentlyDraggingRect);
                break;
            }
        }

        drawOrders.Reverse();

        if (DraggingRect)
            avaliableRectsForDraggedRectToMoveWithin = FindAvaliableRectsForDraggedRectToMoveWithin();
    }

    private void HandleEndDrag(Event e)
    {
        if (DraggingRect && ((e.type == EventType.MouseUp && e.button == 0) || (e.type == EventType.MouseEnterWindow)))
        {
            currentlyDraggingRect.dragging = false;
            DraggingRect = false;
        }
    }

    private void HandleDrag(Event e)
    {
        if (!DraggingRect)
            return;

        GetCurrentlyDraggingClosestPosition(e);
    }

    private List<Rect> FindAvaliableRectsForDraggedRectToMoveWithin()
    {
        List<Rect> otherRects = new List<Rect>();

        List<float> verticalLines = new List<float>();
        List<float> horizontalLines = new List<float>();

        if (!currentlyDraggingRect.ignoreOtherDraggableRects)
        {
            for (int i = 0; i < draggableRects.Count; i++)
            {
                if (draggableRects[i] == currentlyDraggingRect || draggableRects[i].ignoreOtherDraggableRects)
                    continue;

                Rect rect = draggableRects[i].rect;
                if (!currentlyDraggingRect.constrainedX)
                {
                    verticalLines.Add(rect.xMin);
                    verticalLines.Add(rect.xMax);
                }

                if (!currentlyDraggingRect.constrainedY)
                {
                    horizontalLines.Add(rect.yMin);
                    horizontalLines.Add(rect.yMax);
                }

                otherRects.Add(rect);
            }
        }
        
        List<Rect> avaliableRects = new List<Rect>();

        verticalLines.Sort();
        horizontalLines.Sort();

        for (int xMinIndex = -1; xMinIndex < verticalLines.Count; xMinIndex++)
        {
            for (int xMaxIndex = xMinIndex + 1; xMaxIndex < verticalLines.Count + 1; xMaxIndex++)
            {
                for (int yMinIndex = -1; yMinIndex < horizontalLines.Count; yMinIndex++)
                {
                    for (int yMaxIndex = yMinIndex + 1; yMaxIndex < horizontalLines.Count + 1; yMaxIndex++)
                    {
                        float xMin = (xMinIndex != -1) ?
                            verticalLines[xMinIndex] :
                            limitXPositionToRect ? areaRect.xMin : float.MinValue;

                        float xMax = (xMaxIndex != verticalLines.Count) ? 
                            verticalLines[xMaxIndex] :
                            limitXPositionToRect ? areaRect.xMax : float.MaxValue;

                        float yMin = (yMinIndex != -1) ? 
                            horizontalLines[yMinIndex] : 
                            limitYPositionToRect ? areaRect.yMin : float.MinValue;

                        float yMax = (yMaxIndex != horizontalLines.Count) ? 
                            horizontalLines[yMaxIndex] :
                            limitYPositionToRect ? areaRect.yMax : float.MaxValue;


                        if (currentlyDraggingRect.constrainedX)
                        {
                            xMax = currentlyDraggingRect.rect.xMax;
                            xMin = currentlyDraggingRect.rect.xMin;
                        }
                        if (currentlyDraggingRect.constrainedY)
                        {
                            yMax = currentlyDraggingRect.rect.yMax;
                            yMin = currentlyDraggingRect.rect.yMin;
                        }

                        Rect candidateRect = new Rect() { xMin = xMin, xMax = xMax, yMin = yMin, yMax = yMax };

                        bool intersects = false;

                        foreach (Rect rect in otherRects)
                        {
                            if (rect.Overlaps(candidateRect))
                            {
                                intersects = true;
                                break;
                            }
                        }

                        if (!intersects && candidateRect.width != 0f && candidateRect.height != 0f && !avaliableRects.Contains(candidateRect))
                        {
                            avaliableRects.Add(candidateRect);
                        }
                    }
                }
            }
        }

       

        List<Rect> finalAvaliableRects = new List<Rect>();

        foreach (Rect rect in avaliableRects)
        {
            if (rect.width + 0.0001f < currentlyDraggingRect.rect.width || rect.height + 0.0001f < currentlyDraggingRect.rect.height)
                continue;

            foreach (Rect containCheckRect in avaliableRects)
            {
                if(rect == containCheckRect)
                    continue;

                if (containCheckRect.ContainsWholeRect(rect))
                {
                    goto Next;
                }
            }
            finalAvaliableRects.Add(rect);

        Next:;
        }

        return finalAvaliableRects;
    }

    private void GetCurrentlyDraggingClosestPosition(Event e)
    {
        Vector2 preferedPosition = e.mousePosition + currentlyDraggingRect.dragOffset;

        float closestDistance = float.MaxValue;
        Vector2 closestPosition = currentlyDraggingRect.rect.center;

        foreach (Rect avaliableRect in avaliableRectsForDraggedRectToMoveWithin)
        {
            float closestX = Mathf.Clamp(preferedPosition.x, avaliableRect.xMin + currentlyDraggingRect.rect.width / 2f, avaliableRect.xMax - currentlyDraggingRect.rect.width / 2f);
            float closestY = Mathf.Clamp(preferedPosition.y, avaliableRect.yMin + currentlyDraggingRect.rect.height / 2f, avaliableRect.yMax - currentlyDraggingRect.rect.height / 2f);
            Vector2 position = new Vector2(closestX, closestY);
            float distance = Vector2.Distance(preferedPosition, position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = position;

                if (closestDistance == 0f)
                    break;
            }
        }
        currentlyDraggingRect.rect.center = closestPosition;
    }
}
