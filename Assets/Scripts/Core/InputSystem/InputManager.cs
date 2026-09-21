using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    public InputSystemActions Actions { get; private set; }

    #region Action Shortcuts
    public static InputSystemActions.UIActions UI
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if(Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.UI;
        }
    }

    

    public static InputSystemActions.UI_HeroStatsActions UI_HeroStats
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if (Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.UI_HeroStats;
        }
    }

    public static InputSystemActions.PlayerActions Player
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if (Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.Player;
        }
    }
    public static InputSystemActions.Camera_MovementActions CameraMovement    
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if (Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.Camera_Movement;
        }
    }
    public static InputSystemActions.Spell_CastingActions Spell_Casting
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if (Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.Spell_Casting;
        }
    }

    public static InputSystemActions.Spell_SelectionActions Spell_Selection 
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if (Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.Spell_Selection;
        }
    }

    public static InputSystemActions.DialogueActions Dialogue
    {
        get
        {
            if (Instance == null)
                throw new NullReferenceException("Instance not set");

            if (Instance.Actions == null)
                throw new NullReferenceException("Instance not set");

            return Instance.Actions.Dialogue;
        }
    }

    #endregion

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

    protected override void Awake()
    {
        Actions = new InputSystemActions();
        base.Awake();

        _contextMaps = new Dictionary<InputContext, InputActionMap>
        {
            { InputContext.Player,       Actions.Player.Get() },
            { InputContext.UI,           Actions.UI.Get() },
            { InputContext.UI_HeroStats, Actions.UI_HeroStats.Get() },
            { InputContext.UI_TilePlayer,Actions.UI_TilePlayer.Get() },
            { InputContext.UI_Shop,      Actions.UI_Shop.Get() },
            { InputContext.Dialogue,     Actions.Dialogue.Get() },
            { InputContext.Spell_Selection,     Actions.Spell_Selection.Get() },
            { InputContext.Spell_Casting,     Actions.Spell_Casting.Get() },
            /*{ InputContext.Main_Menu,    Actions.Main_Menu.Get() },
            { InputContext.Paused,       Actions.Paused.Get() },*/
        };

        _defaultPriority = new Dictionary<InputContext, int>
        {
            { InputContext.Player,          0 },
            { InputContext.UI,              0 },
            { InputContext.Spell_Selection, 0 },
            { InputContext.Spell_Casting,   0 },
            { InputContext.UI_HeroStats,    5 },
            { InputContext.UI_TilePlayer,   5 },
            { InputContext.UI_Shop,         5 },
            { InputContext.Dialogue,        10 },
            { InputContext.Main_Menu,       15 },
            { InputContext.Paused,          20 },
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
        PushContext(InputContext.Player);
        PushContext(InputContext.Spell_Selection);
        PushContext(InputContext.UI);
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
    public InputContextHandle PushContext(InputContext context, bool exclusive = false, IInputContextListener listener = null)
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
            bool shouldBeEnabled = topExclusive == null || e == topExclusive;

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

}