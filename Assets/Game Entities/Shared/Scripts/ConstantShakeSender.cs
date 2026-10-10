using Helper.Wait;
using Heros;
using UnityEngine;
/// <summary>
/// Drives a continuous ScreenShakeManager trauma source while a building is
/// emerging from the ground, scaled by distance to the player/camera: closer
/// = more shake, and it holds steady (doesn't decay) as long as the distance
/// stays constant, because it's re-set every frame rather than added once.
///
/// Usage:
/// - Drop this on the building.
/// - Assign `distanceTarget` (usually your hero/player transform - see note
///   below on camera vs. player).
/// - Call StartEmerging() when the rise animation begins, StopEmerging()
///   when it finishes. If you forget to call StopEmerging(), disabling or
///   destroying this object will clean it up too.
/// </summary>
public class ConstantShakeSender : MonoBehaviour
{
    [Header("Distance Target")]
    [Tooltip("What distance is measured to. Usually the player/hero - " +
             "if you'd rather base it on how far the CAMERA is from the " +
             "building instead (matters more if the camera can be dragged " +
             "away from the hero), assign the camera rig here instead.")]
    private Transform distanceTarget;

    [Header("Distance Falloff")]
    [Tooltip("At this distance (or closer), shake is at its strongest (maxTrauma).")]
    [SerializeField] private float minDistance = 2f;
    [Tooltip("At this distance (or farther), shake contributes nothing.")]
    [SerializeField] private float maxDistance = 15f;
    [Tooltip("Trauma contributed when the target is at minDistance or closer.")]
    [SerializeField, Range(0f, 1f)] private float maxTrauma = 0.6f;
    [Tooltip("Shapes the falloff between maxDistance (0) and minDistance (1). Default eases in as you get close rather than scaling linearly.")]
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool _isShaking;

    public void StartShaking()
    {
        _isShaking = true;
    }

    public void StopShaking()
    {
        _isShaking = false;
        if (ScreenShakeManager.Instance != null)
            ScreenShakeManager.Instance.ClearSustainedTrauma(this);
    }

    private void Start()
    {
        HeroManager.WhenReady(SetTarget);
    }

    private void SetTarget(HeroManager manager)
    {
        distanceTarget =    manager.ActiveHero != null ?
                            manager.ActiveHero.transform : 
                            null;
    }

    private void Update()
    {
        if (!_isShaking || distanceTarget == null || ScreenShakeManager.Instance == null)
            return;

        float distance = Vector3.Distance(transform.position, distanceTarget.position);

        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
        t = Mathf.Clamp01(t);

        float trauma = falloffCurve.Evaluate(t) * maxTrauma;

        ScreenShakeManager.Instance.SetSustainedTrauma(this, trauma);
    }

    private void OnDisable()
    {
        if (ScreenShakeManager.Instance != null)
            ScreenShakeManager.Instance.ClearSustainedTrauma(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
#endif
}