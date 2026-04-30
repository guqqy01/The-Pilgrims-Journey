using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class DraggableArea<T> : DraggableArea where T : IDraggableRectCompatible
{
    private readonly List<T> _list;

    private readonly Dictionary<DraggableRect, T> _draggableRectAndItem = new Dictionary<DraggableRect, T>();

    public Vector2 rectAreaRelativeSize = Vector2.one;

    public bool limitXSizeToRect = true;
    public bool limitYSizeToRect = true;

    // Default size for new items, in relative space (same units as rectAreaRelativeSize)
    public Vector2 defaultItemRelativeSize = new Vector2(0.3f, 0.3f);

    // Minimum size for resize, in relative space
    public Vector2 minItemRelativeSize = new Vector2(0.15f, 0.15f);

    // Converts minItemRelativeSize to pixel size for use by the base class resize logic
    public override Vector2 MinDraggableRectSize =>
        new Vector2(
            AreaRect.width * (minItemRelativeSize.x / rectAreaRelativeSize.x),
            AreaRect.height * (minItemRelativeSize.y / rectAreaRelativeSize.y)
        );

    public DraggableArea(List<T> compatibleList)
    {
        _list = compatibleList;
        protectedOnDraggingRect += SyncDraggedItemToRect;

        rightClickSpaceOptions.Add("Add Item", () =>
        {
            T newItem = (T)System.Activator.CreateInstance(typeof(T));
            _list.Add(newItem);
            HandleAmountOfDraggableRects();
            SetRectValues();
        });

        rightClickRectOptions.Add("Remove Item", () =>
        {
            if (RectRightClicked == null)
                return;
            if (_draggableRectAndItem.TryGetValue(RectRightClicked, out T item))
            {
                _list.Remove(item);
                HandleAmountOfDraggableRects();
                SetRectValues();
            }
        });
    }

    public override void HandleDraggableRects(Event e)
    {
        HandleAmountOfDraggableRects();
        SetRectValues();
        base.HandleDraggableRects(e);
    }

    protected override void HandleResize(Event e)
    {
        base.HandleResize(e);

        if(SelectedRect != null)
            SyncDraggedItemToRect(SelectedRect);
    }

    private Rect ClampRectToArea(Rect rect)
    {
        if (limitXSizeToRect)
            rect.width = Mathf.Min(rect.width, AreaRect.width);
        if (limitYSizeToRect)
            rect.height = Mathf.Min(rect.height, AreaRect.height);
        if (limitXPositionToRect)
            rect.x = Mathf.Clamp(rect.x, AreaRect.xMin, AreaRect.xMax - rect.width);
        if (limitYPositionToRect)
            rect.y = Mathf.Clamp(rect.y, AreaRect.yMin, AreaRect.yMax - rect.height);

        return rect;
    }

    protected void SyncDraggedItemToRect(DraggableRect draggedRect)
    {
        if (!_draggableRectAndItem.TryGetValue(draggedRect, out T item))
            return;

        item.SetPosition(GetItemRelativePosition(draggedRect.rect.position));
        item.SetSize(GetItemRelativeSize(draggedRect.rect.size));
    }

    private Vector2 GetItemRelativePosition(Vector2 rectPosition)
    {
        float xT = Mathf.InverseLerp(AreaRect.xMin, AreaRect.xMax, rectPosition.x);
        float yT = Mathf.InverseLerp(AreaRect.yMin, AreaRect.yMax, rectPosition.y);
        float itemRelativeXPosition = Mathf.Lerp(0, rectAreaRelativeSize.x, xT);
        float itemRelativeYPosition = Mathf.Lerp(0, rectAreaRelativeSize.y, yT);
        return new Vector2(itemRelativeXPosition, itemRelativeYPosition);
    }

    private Vector2 GetRectPositionFromItemRelativePosition(Vector2 itemRelativePosition)
    {
        float xT = Mathf.InverseLerp(0, rectAreaRelativeSize.x, itemRelativePosition.x);
        float yT = Mathf.InverseLerp(0, rectAreaRelativeSize.y, itemRelativePosition.y);
        float rectXPosition = Mathf.Lerp(AreaRect.xMin, AreaRect.xMax, xT);
        float rectYPosition = Mathf.Lerp(AreaRect.yMin, AreaRect.yMax, yT);
        return new Vector2(rectXPosition, rectYPosition);
    }

    private Vector2 GetItemRelativeSize(Vector2 rectSize)
    {
        float xT = Mathf.InverseLerp(0, AreaRect.size.x, rectSize.x);
        float yT = Mathf.InverseLerp(0, AreaRect.size.y, rectSize.y);

        float itemRelativeXSize = Mathf.Lerp(0, rectAreaRelativeSize.x, xT);
        float itemRelativeYSize = Mathf.Lerp(0, rectAreaRelativeSize.y, yT);
        
        return new Vector2(itemRelativeXSize, itemRelativeYSize);
    }

    private Vector2 GetRectSizeFromItemRelativeSize(Vector2 itemRelativeSize)
    {
        float xT = Mathf.InverseLerp(0, rectAreaRelativeSize.x, itemRelativeSize.x);
        float yT = Mathf.InverseLerp(0, rectAreaRelativeSize.y, itemRelativeSize.y);

        float rectXSize = Mathf.Lerp(0, AreaRect.size.x, xT);
        float rectYSize = Mathf.Lerp(0, AreaRect.size.y, yT);

        return new Vector2(rectXSize, rectYSize);
    }

    private void SetRectValues()
    {
        foreach (DraggableRect draggableRect in _draggableRects)
        {
            if (!_draggableRectAndItem.TryGetValue(draggableRect, out T item))
                continue;

            Rect rect = item.GetRect();
            rect.size = GetRectSizeFromItemRelativeSize(rect.size);
            rect.position = GetRectPositionFromItemRelativePosition(rect.position);

            draggableRect.rect = ClampRectToArea(rect);

            item.SetPosition(GetItemRelativePosition(draggableRect.rect.position));
            item.SetSize(GetItemRelativeSize(draggableRect.rect.size));
        }
    }

    protected void HandleAmountOfDraggableRects()
    {
        //Remove draggable rects

        List<DraggableRect> draggableRectsToRemove = new List<DraggableRect>();

        foreach(DraggableRect draggableRect in _draggableRects)
        {
            if (!_draggableRectAndItem.TryGetValue(draggableRect, out T item) || !_list.Contains(item))
                draggableRectsToRemove.Add(draggableRect);
        }
        
        for (int i = 0; i < draggableRectsToRemove.Count; i++)
        {
            if (draggableRectsToRemove[i] == SelectedRect)
            {
                SelectedRect = null;
                GUI.changed = true;
            }

            _draggableRects.Remove(draggableRectsToRemove[i]);
            _draggableRectAndItem.Remove(draggableRectsToRemove[i]);
        }

        //Add draggable rects

        if (_list == null)
            return;

        for (int i = 0; i < _list.Count; i++)
        {
            T item = _list[i];

            if (!_draggableRectAndItem.ContainsValue(item))
            {
                // Give the item a default size in relative space before reading its rect
                if (item.GetSize() == Vector2.zero)
                    item.SetSize(defaultItemRelativeSize);

                DraggableRect draggableRect = new DraggableRect(item.GetRect(), DefaultConstrainX, DefaultConstrainY, defaultInnerColor, defaultOuterColor, defaultIgnoreOtherDraggableRects);
                Add(0, draggableRect);
                _draggableRectAndItem.Add(draggableRect, item);
            }
        }
    }
}



public class DraggableArea
{
    public Rect AreaRect;

    protected readonly List<DraggableRect> _draggableRects = new List<DraggableRect>();
    protected readonly SortedDictionary<int, List<DraggableRect>> layerAndDrawOrders = new SortedDictionary<int, List<DraggableRect>>();

    public Color backgroundColorInner = Color.grey;
    public Color backgroundColorOuter = Color.black;

    public Vector2 defaultDraggableRectSizes = new Vector2(10f, 10f);
    public Color defaultInnerColor = Color.grey;
    public Color defaultOuterColor = Color.black;
    public bool defaultIgnoreOtherDraggableRects;
    public bool fillOtherAxis;

    public bool DefaultConstrainX = false;
    public bool DefaultConstrainY = false;

    public bool DraggingRect { private set; get; }
    public DraggableRect CurrentlyDraggingRect { private set; get; }

    private List<Rect> _avaliableRectsForDraggedRectToMoveWithin = new List<Rect>();

    public delegate void BeforeDraw(Rect rect);
    public BeforeDraw beforeDraw;

    public delegate void AfterDraw(Rect rect);
    public AfterDraw afterDraw;

    public delegate void BeforeDrag();
    public BeforeDrag beforeDrag;
    public delegate void AfterDrag();
    public AfterDrag afterDrag;

    protected delegate void ProtectedOnDraggingRect(DraggableRect draggingRect);
    protected ProtectedOnDraggingRect protectedOnDraggingRect;

    public Dictionary<string, GenericMenu.MenuFunction> rightClickSpaceOptions = new Dictionary<string, GenericMenu.MenuFunction>();
    public Dictionary<string, GenericMenu.MenuFunction> rightClickRectOptions = new Dictionary<string, GenericMenu.MenuFunction>();

    public delegate void OnRightClickRect(DraggableRect rectClicked);
    public OnRightClickRect onRightClickRect;

    private bool _rightClickedItem;
    public Vector2 PositionClicked { private set; get; }
    public DraggableRect RectRightClicked { private set; get; }

    public bool limitXPositionToRect = true;
    public bool limitYPositionToRect = true;

    // New fields
    public DraggableRect SelectedRect { get; protected set; }
    public virtual Vector2 MinDraggableRectSize { get; set; } = new Vector2(5f, 5f);
    private const float HandleSize = 6f;

    private bool _resizing;
    private ResizeHandle _activeHandle;
    private List<Rect> _availableRectsForResize = new List<Rect>();

    private enum ResizeEdge { Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight }

    private struct ResizeHandle
    {
        public Rect rect;
        public ResizeEdge edge;
    }

    public DraggableArea() { }

    public DraggableRect this[int i]
    {
        get
        {
            return _draggableRects[i];
        }
    }

    public List<DraggableRect> GetDraggableRects()
    {
        return _draggableRects;
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

        HandleSelection(e);

        HandleBeforeDrag();

        HandleEndResize(e);
        HandleEndDrag(e);

        HandleStartResize(e);

        HandleStartDrag(e);
        HandleDrag(e);
        HandleResize(e);

        HandleAfterDrag();
        HandleRightClick(e);
        DrawBackground();
        beforeDraw?.Invoke(AreaRect);
        if (DraggingRect) 
        { 
            protectedOnDraggingRect?.Invoke(CurrentlyDraggingRect); 
            GUI.changed = true; 
        }
        DrawRects(e);

        afterDraw?.Invoke(AreaRect);
        DrawResizeHandles();
    }

    private void HandleSelection(Event e)
    {
        if (e.type != EventType.MouseDown || e.button != 0)
            return;

        List<List<DraggableRect>> drawOrders = layerAndDrawOrders.Values.ToList();
        drawOrders.Reverse();

        foreach (List<DraggableRect> drawOrder in drawOrders)
        {
            List<DraggableRect> reversed = Enumerable.Reverse(drawOrder).ToList();
            foreach (DraggableRect draggableRect in reversed)
            {
                if (draggableRect.rect.Contains(e.mousePosition))
                {
                    SelectedRect = draggableRect;
                    GUI.changed = true;
                    return;
                }
            }
        }

        if (SelectedRect != null)
        {
            foreach (ResizeHandle handle in GetResizeHandles(SelectedRect))
            {
                if (handle.rect.Contains(e.mousePosition))
                    return; // Let HandleStartResize deal with it
            }
        }

        if (AreaRect.Contains(e.mousePosition))
        {
            SelectedRect = null;
            GUI.changed = true;
        }
    }

    private List<ResizeHandle> GetResizeHandles(DraggableRect draggableRect)
    {
        Rect r = draggableRect.rect;
        float h = HandleSize;
        float hh = h / 2f;

        return new List<ResizeHandle>
        {
            new ResizeHandle { edge = ResizeEdge.Left,        rect = new Rect(r.xMin - hh, r.center.y - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.Right,       rect = new Rect(r.xMax - hh, r.center.y - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.Top,         rect = new Rect(r.center.x - hh, r.yMin - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.Bottom,      rect = new Rect(r.center.x - hh, r.yMax - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.TopLeft,     rect = new Rect(r.xMin - hh, r.yMin - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.TopRight,    rect = new Rect(r.xMax - hh, r.yMin - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.BottomLeft,  rect = new Rect(r.xMin - hh, r.yMax - hh, h, h) },
            new ResizeHandle { edge = ResizeEdge.BottomRight, rect = new Rect(r.xMax - hh, r.yMax - hh, h, h) },
        };
    }
    private void HandleStartResize(Event e)
    {
        if (SelectedRect == null || _resizing || DraggingRect)
            return;
        if (e.type != EventType.MouseDown || e.button != 0)
            return;

        foreach (ResizeHandle handle in GetResizeHandles(SelectedRect))
        {
            if (!handle.rect.Contains(e.mousePosition))
                continue;

            _resizing = true;
            _activeHandle = handle;
            _availableRectsForResize = FindAvailableRectsForResize(SelectedRect, handle.edge);
            e.Use();
            return;
        }
    }

    private void HandleEndResize(Event e)
    {
        if (!_resizing) return;
        if ((e.type == EventType.MouseUp && e.button == 0) || e.type == EventType.MouseLeaveWindow)
        {
            _resizing = false;
        }
    }

    protected virtual void HandleResize(Event e)
    {
        if (!_resizing || SelectedRect == null)
            return;

        Vector2 mouse = e.mousePosition;
        Rect r = SelectedRect.rect;

        // Which boundaries does this edge move?
        bool movesLeft = _activeHandle.edge is ResizeEdge.Left or ResizeEdge.TopLeft or ResizeEdge.BottomLeft;
        bool movesRight = _activeHandle.edge is ResizeEdge.Right or ResizeEdge.TopRight or ResizeEdge.BottomRight;
        bool movesTop = _activeHandle.edge is ResizeEdge.Top or ResizeEdge.TopLeft or ResizeEdge.TopRight;
        bool movesBottom = _activeHandle.edge is ResizeEdge.Bottom or ResizeEdge.BottomLeft or ResizeEdge.BottomRight;

        float newXMin = movesLeft ? mouse.x : r.xMin;
        float newXMax = movesRight ? mouse.x : r.xMax;
        float newYMin = movesTop ? mouse.y : r.yMin;
        float newYMax = movesBottom ? mouse.y : r.yMax;

        // Clamp moved edges to the best available rect, same principle as dragging
        foreach (Rect available in _availableRectsForResize)
        {
            if (movesLeft) newXMin = Mathf.Clamp(newXMin, available.xMin, r.xMax - MinDraggableRectSize.x);
            if (movesRight) newXMax = Mathf.Clamp(newXMax, r.xMin + MinDraggableRectSize.x, available.xMax);
            if (movesTop) newYMin = Mathf.Clamp(newYMin, available.yMin, r.yMax - MinDraggableRectSize.y);
            if (movesBottom) newYMax = Mathf.Clamp(newYMax, r.yMin + MinDraggableRectSize.y, available.yMax);
        }

        SelectedRect.rect = Rect.MinMaxRect(newXMin, newYMin, newXMax, newYMax);
        GUI.changed = true;
    }

    private List<Rect> FindAvailableRectsForResize(DraggableRect resizingRect, ResizeEdge edge)
    {
        // Reuse the same spatial subdivision as dragging.
        // The available rects already represent all collision-free zones,
        // so we just filter to ones that overlap the fixed edges of the rect
        // (the edges that aren't moving stay put, so the available rect must contain them).

        if (resizingRect.IgnoreOtherDraggableRects)
        {
            // Just return the area rect as the single available space
            return new List<Rect> { AreaRect };
        }

        // Build other rects the same way as dragging
        List<Rect> otherRects = _draggableRects
            .Where(r => r != resizingRect && !r.IgnoreOtherDraggableRects)
            .Select(r => r.rect)
            .ToList();

        List<float> verticalLines = new List<float> { AreaRect.xMin, AreaRect.xMax };
        List<float> horizontalLines = new List<float> { AreaRect.yMin, AreaRect.yMax };

        foreach (Rect r in otherRects)
        {
            verticalLines.Add(r.xMin);
            verticalLines.Add(r.xMax);
            horizontalLines.Add(r.yMin);
            horizontalLines.Add(r.yMax);
        }

        verticalLines = verticalLines.Distinct().OrderBy(v => v).ToList();
        horizontalLines = horizontalLines.Distinct().OrderBy(v => v).ToList();

        List<Rect> available = new List<Rect>();

        for (int xi = 0; xi < verticalLines.Count - 1; xi++)
        {
            for (int yi = 0; yi < horizontalLines.Count - 1; yi++)
            {
                Rect candidate = Rect.MinMaxRect(
                    verticalLines[xi], horizontalLines[yi],
                    verticalLines[xi + 1], horizontalLines[yi + 1]);

                bool blocked = otherRects.Any(r => r.Overlaps(candidate));
                if (!blocked)
                    available.Add(candidate);
            }
        }

        return available;
    }

    private void DrawResizeHandles()
    {
        if (SelectedRect == null) return;

        foreach (ResizeHandle handle in GetResizeHandles(SelectedRect))
        {
            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(handle.rect, Color.white, Color.black);
        }
    }

    private void HandleBeforeDrag()
    {
        beforeDrag?.Invoke();

        foreach(DraggableRect rect in _draggableRects)
        {
            rect.BeforeDrag();
        }
    }

    private void HandleAfterDrag()
    {
        afterDrag?.Invoke();

        foreach (DraggableRect rect in _draggableRects)
        {
            rect.AfterDrag();
        }
    }

    private void DrawBackground()
    {
        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(AreaRect, backgroundColorInner, backgroundColorOuter);
    }

    private void HandleRightClick(Event e)
    {
        if (e.type != EventType.MouseDown || e.button != 1)
            return;

        _rightClickedItem = false;

        PositionClicked = e.mousePosition;

        foreach(var layerAndDrawOrder in layerAndDrawOrders)
        {
            foreach (DraggableRect rect in layerAndDrawOrder.Value)
            {
                if (rect.rect.Contains(e.mousePosition))
                {
                    _rightClickedItem = true;
                    RectRightClicked = rect;

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
        
        if (!_rightClickedItem && AreaRect.Contains(e.mousePosition))
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
                if (!_draggableRects.Contains(rect))
                    toRemove.Add(rect);
            }

            foreach (var rect in toRemove)
            {
                drawOrder.Remove(rect);
            }
        }

        foreach (DraggableRect draggableRect in _draggableRects)
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
            _draggableRects.Remove(draggableRect);
        }
    }

    public void Add(int layer, DraggableRect draggableRect)
    {
        _draggableRects.Add(draggableRect);
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
        _draggableRects.Remove(draggableRect);
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
                    CurrentlyDraggingRect = drawOrder[j];
                    e.Use();
                    break;
                }
            }

            drawOrder.Reverse();

            if (DraggingRect)
            {
                drawOrder.Remove(CurrentlyDraggingRect);
                drawOrder.Add(CurrentlyDraggingRect);
                break;
            }
        }

        drawOrders.Reverse();

        if (DraggingRect)
            _avaliableRectsForDraggedRectToMoveWithin = FindAvaliableRectsForDraggedRectToMoveWithin();
    }

    private void HandleEndDrag(Event e)
    {
        if (DraggingRect && ((e.type == EventType.MouseUp && e.button == 0) || (e.type == EventType.MouseEnterWindow)))
        {
            CurrentlyDraggingRect.StopDragging();
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

        if (!CurrentlyDraggingRect.IgnoreOtherDraggableRects)
        {
            for (int i = 0; i < _draggableRects.Count; i++)
            {
                if (_draggableRects[i] == CurrentlyDraggingRect || _draggableRects[i].IgnoreOtherDraggableRects)
                    continue;

                Rect rect = _draggableRects[i].rect;
                if (!CurrentlyDraggingRect.ConstrainedX)
                {
                    verticalLines.Add(rect.xMin);
                    verticalLines.Add(rect.xMax);
                }

                if (!CurrentlyDraggingRect.ConstrainedY)
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
                            limitXPositionToRect ? AreaRect.xMin : float.MinValue;

                        float xMax = (xMaxIndex != verticalLines.Count) ? 
                            verticalLines[xMaxIndex] :
                            limitXPositionToRect ? AreaRect.xMax : float.MaxValue;

                        float yMin = (yMinIndex != -1) ? 
                            horizontalLines[yMinIndex] : 
                            limitYPositionToRect ? AreaRect.yMin : float.MinValue;

                        float yMax = (yMaxIndex != horizontalLines.Count) ? 
                            horizontalLines[yMaxIndex] :
                            limitYPositionToRect ? AreaRect.yMax : float.MaxValue;


                        if (CurrentlyDraggingRect.ConstrainedX)
                        {
                            xMax = CurrentlyDraggingRect.rect.xMax;
                            xMin = CurrentlyDraggingRect.rect.xMin;
                        }
                        if (CurrentlyDraggingRect.ConstrainedY)
                        {
                            yMax = CurrentlyDraggingRect.rect.yMax;
                            yMin = CurrentlyDraggingRect.rect.yMin;
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
            if (rect.width + 0.0001f < CurrentlyDraggingRect.rect.width || rect.height + 0.0001f < CurrentlyDraggingRect.rect.height)
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
        Vector2 preferedPosition = e.mousePosition + CurrentlyDraggingRect.DragOffset;

        float closestDistance = float.MaxValue;
        Vector2 closestPosition = CurrentlyDraggingRect.rect.center;

        foreach (Rect avaliableRect in _avaliableRectsForDraggedRectToMoveWithin)
        {
            float closestX = Mathf.Clamp(preferedPosition.x, avaliableRect.xMin + CurrentlyDraggingRect.rect.width / 2f, avaliableRect.xMax - CurrentlyDraggingRect.rect.width / 2f);
            float closestY = Mathf.Clamp(preferedPosition.y, avaliableRect.yMin + CurrentlyDraggingRect.rect.height / 2f, avaliableRect.yMax - CurrentlyDraggingRect.rect.height / 2f);
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
        CurrentlyDraggingRect.rect.center = closestPosition;
    }
}
