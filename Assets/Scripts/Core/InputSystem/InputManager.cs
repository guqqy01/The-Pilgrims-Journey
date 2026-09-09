using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum InputContext
{
    Player,
    UI,
    UI_HeroStats,
    UI_TilePlayer,
    UI_Shop,
    Main_Menu,
    Dialogue,
    Paused,
}

/// <summary>
/// Implement this on anything that pushes an InputContext and cares about
/// when it gains/loses input focus, as opposed to just whether its map is enabled.
/// All methods are optional - use default interface methods so listeners only
/// override what they actually need.
/// </summary>
public interface IInputContextListener
{
    /// <summary>Called when a higher-priority exclusive context is pushed on top of this one.</summary>
    void OnContextPaused() { }

    /// <summary>Called when the context(s) above this one have been popped and this one is visible again.</summary>
    void OnContextResumed() { }

    /// <summary>Called when this specific context entry is popped for good.</summary>
    void OnContextClosed() { }
}

/// <summary>
/// Opaque handle returned by PushContext. Pass it back to PopContext to remove
/// exactly the entry you pushed - never pops "whatever is on top" by accident.
/// </summary>
public readonly struct InputContextHandle
{
    internal readonly int Id;
    internal InputContextHandle(int id) => Id = id;

    public static readonly InputContextHandle Invalid = new InputContextHandle(-1);
    public bool IsValid => Id >= 0;
}

public class InputManager : Singleton<InputManager>
{
    public InputSystemActions Actions { get; private set; }

    [Header("Priority (higher = takes precedence when exclusive)")]
    [SerializeField]
    private ContextPriorityEntry[] priorityOverrides;

    [Serializable]
    private struct ContextPriorityEntry
    {
        public InputContext Context;
        public int Priority;
    }

    private class ContextEntry
    {
        public int Id;
        public InputContext Context;
        public int Priority;
        public bool Exclusive;
        public IInputContextListener Listener;
        public bool CurrentlyEnabled;
    }

    private readonly List<ContextEntry> _activeEntries = new();
    private int _nextId = 0;

    private Dictionary<InputContext, InputActionMap> _contextMaps;
    private Dictionary<InputContext, int> _defaultPriority;

    // ---------------------------------------------------------------------
    // Setup
    // ---------------------------------------------------------------------

    private void Awake()
    {
        Actions = new InputSystemActions();
        base.InitiateFromRootAtAwake();

        _contextMaps = new Dictionary<InputContext, InputActionMap>
        {
            { InputContext.Player,       Actions.Player.Get() },
            { InputContext.UI,           Actions.UI.Get() },
            { InputContext.UI_HeroStats, Actions.UI_HeroStats.Get() },
            { InputContext.UI_TilePlayer,Actions.UI_TilePlayer.Get() },
            { InputContext.UI_Shop,      Actions.UI_Shop.Get() },
            /*{ InputContext.Main_Menu,    Actions.Main_Menu.Get() },
            { InputContext.Dialogue,     Actions.Dialogue.Get() },
            { InputContext.Paused,       Actions.Paused.Get() },*/
        };

        _defaultPriority = new Dictionary<InputContext, int>
        {
            { InputContext.Player,        0 },
            { InputContext.UI,            0 },
            { InputContext.UI_HeroStats,  5 },
            { InputContext.UI_TilePlayer, 5 },
            { InputContext.UI_Shop,       5 },
            { InputContext.Dialogue,      10 },
            { InputContext.Main_Menu,     15 },
            { InputContext.Paused,        20 },
        };

        // allow designer overrides from the Inspector without touching code
        if (priorityOverrides != null)
        {
            foreach (var entry in priorityOverrides)
                _defaultPriority[entry.Context] = entry.Priority;
        }
    }

    private void OnEnable()
    {
        Actions?.asset.Enable();
        PushContext(InputContext.Player, exclusive: true);
    }

    private void OnDisable()
    {
        Actions?.asset.Disable();
    }

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    /// <summary>
    /// Activates a context. If exclusive and higher priority than whatever is
    /// currently the top exclusive context, it takes over immediately and the
    /// previous top is paused (not disabled outright - it stays in the stack).
    /// If a higher-priority exclusive context is already active, this entry is
    /// recorded but stays inactive until it becomes the top priority naturally.
    /// Non-exclusive contexts are always enabled immediately, layered on top of
    /// whatever else is active, and never block or get blocked.
    /// </summary>
    public InputContextHandle PushContext(InputContext context, bool exclusive = true, IInputContextListener listener = null)
    {
        if (!_contextMaps.ContainsKey(context))
        {
            Debug.LogError($"InputManager: no action map registered for context '{context}'.");
            return InputContextHandle.Invalid;
        }

        var entry = new ContextEntry
        {
            Id = _nextId++,
            Context = context,
            Priority = _defaultPriority.TryGetValue(context, out int p) ? p : 0,
            Exclusive = exclusive,
            Listener = listener,
            CurrentlyEnabled = false,
        };

        _activeEntries.Add(entry);
        Recompute();
        return new InputContextHandle(entry.Id);
    }

    /// <summary>
    /// Removes exactly the entry associated with this handle - never pops
    /// "whatever is on top." Safe to call even if this entry is not currently
    /// the active/visible one (e.g. it was pending behind something higher priority).
    /// </summary>
    public void PopContext(InputContextHandle handle)
    {
        if (!handle.IsValid)
            return;

        int index = _activeEntries.FindIndex(e => e.Id == handle.Id);
        if (index < 0)
        {
            Debug.LogWarning($"InputManager: tried to pop a context handle that is not active (id {handle.Id}). " +
                              "This usually means it was already popped.");
            return;
        }

        var entry = _activeEntries[index];
        _activeEntries.RemoveAt(index);

        if (entry.CurrentlyEnabled)
            SetMapEnabled(entry.Context, false);

        entry.Listener?.OnContextClosed();

        Recompute();
    }

    /// <summary>True if this specific handle is still registered (pushed and not yet popped).</summary>
    public bool IsActive(InputContextHandle handle) =>
        handle.IsValid && _activeEntries.Exists(e => e.Id == handle.Id);

    /// <summary>True if this specific handle's context currently has its map enabled (i.e. is the visible/top one).</summary>
    public bool IsEnabled(InputContextHandle handle)
    {
        var entry = _activeEntries.Find(e => e.Id == handle.Id);
        return entry != null && entry.CurrentlyEnabled;
    }

    // ---------------------------------------------------------------------
    // Core resolution
    // ---------------------------------------------------------------------

    private void Recompute()
    {
        // Find the highest-priority exclusive entry - only it (plus every
        // non-exclusive entry) is allowed to be enabled right now.
        ContextEntry topExclusive = null;
        foreach (var e in _activeEntries)
        {
            if (e.Exclusive && (topExclusive == null || e.Priority > topExclusive.Priority))
                topExclusive = e;
        }

        foreach (var e in _activeEntries)
        {
            bool shouldBeEnabled = !e.Exclusive || e == topExclusive;

            if (shouldBeEnabled && !e.CurrentlyEnabled)
            {
                e.CurrentlyEnabled = true;
                SetMapEnabled(e.Context, true);
                e.Listener?.OnContextResumed();
            }
            else if (!shouldBeEnabled && e.CurrentlyEnabled)
            {
                e.CurrentlyEnabled = false;
                SetMapEnabled(e.Context, false);
                e.Listener?.OnContextPaused();
            }
        }
    }

    private void SetMapEnabled(InputContext context, bool enabled)
    {
        if (!_contextMaps.TryGetValue(context, out var map))
            return;

        if (enabled && !map.enabled) map.Enable();
        else if (!enabled && map.enabled) map.Disable();
    }

    // ---------------------------------------------------------------------
    // Misc existing utilities
    // ---------------------------------------------------------------------

    private Camera cam;
    public Camera Camera
    {
        get
        {
            if (cam == null)
                cam = Camera.main;
            return cam;
        }
    }

    public Vector2 MousePosition => Camera.ScreenToWorldPoint(Input.mousePosition);

    public bool MouseOverUI =>
        EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}