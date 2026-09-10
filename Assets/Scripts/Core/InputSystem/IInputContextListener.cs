
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
