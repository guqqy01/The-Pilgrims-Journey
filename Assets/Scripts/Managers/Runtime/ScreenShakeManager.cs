using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton screen shake manager for 2D games.
///
/// Uses a "trauma" model (as popularized by Squirrel Eiserloh's GDC talk):
/// - Trauma builds up from 0 to 1 and decays over time.
/// - Shake offset/rotation scale with trauma^2, so small shakes feel gentle
///   and big shakes feel violent, without needing separate curves per hit.
/// - Perlin noise drives the shake so it's smooth, not jittery random noise.
///
/// Setup:
/// 1. Put this script on an empty GameObject in your scene (e.g. "ScreenShakeManager").
/// 2. Create a child GameObject under your actual Camera called "CameraShakePivot"
///    (or just point cameraTransform at your Camera's transform directly) and
///    assign it in the inspector. Shake is applied as a local offset, so if you
///    point this at your camera's own transform, make sure nothing else is
///    driving that transform's position/rotation every frame (e.g. Cinemachine),
///    or the two will fight. See notes below for Cinemachine integration.
/// 3. Call ScreenShakeManager.Instance.AddTrauma(0.5f) or ShakeOnce(...) from
///    wherever things happen (impacts, explosions, hits taken, etc).
/// </summary>
[DisallowMultipleComponent]
public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    [Header("Target")]
    [Tooltip("Transform the shake is applied to. Usually a pivot parented to your camera, or the camera itself if nothing else drives its local transform.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Trauma")]
    [Tooltip("Current trauma level, 0-1. You can read this but should add to it via AddTrauma().")]
    [SerializeField, Range(0f, 1f)] private float trauma = 0f;
    [Tooltip("How fast trauma decays per second. Higher = shake settles faster.")]
    [SerializeField] private float traumaDecay = 1.5f;
    [Tooltip("Trauma is raised to this power before driving shake magnitude. 2 gives a nice non-linear falloff (small shakes are subtle, big shakes are punchy).")]
    [SerializeField] private float traumaExponent = 2f;

    [Header("Shake Amounts")]
    [Tooltip("Maximum positional offset (world units) at trauma = 1.")]
    [SerializeField] private float maxOffset = 0.6f;
    [Tooltip("Maximum rotation (degrees, Z axis) at trauma = 1.")]
    [SerializeField] private float maxRotation = 8f;
    [Tooltip("How fast the underlying Perlin noise is sampled. Higher = faster, jitterier shake.")]
    [SerializeField] private float frequency = 25f;

    [Header("Options")]
    [Tooltip("If true, uses unscaled delta time (shake still works while Time.timeScale = 0, e.g. pause/hit-stop).")]
    [SerializeField] private bool useUnscaledTime = false;
    [Tooltip("Clamp trauma so a single big AddTrauma call can't exceed this.")]
    [SerializeField] private float maxTrauma = 1f;
    [Tooltip("If a sustained trauma source (SetSustainedTrauma) hasn't been refreshed within this many seconds, it's dropped automatically - a safety net in case a source forgets to call ClearSustainedTrauma.")]
    [SerializeField] private float sustainedSourceTimeout = 0.3f;

    // Distinct noise offsets per axis so X/Y/rotation don't move in lockstep.
    private float _noiseSeedX;
    private float _noiseSeedY;
    private float _noiseSeedRot;

    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;

    // Ongoing, continuously-driven trauma sources (e.g. a building emerging
    // nearby), keyed by whatever object is driving them. Unlike the decaying
    // `trauma` field above (for one-off impacts), these hold steady as long
    // as the source keeps calling SetSustainedTrauma() every frame - which is
    // how you get "constant shake while standing at a constant distance".
    private struct SustainedSource
    {
        public float amount;
        public float lastUpdateTime;
    }
    private readonly Dictionary<object, SustainedSource> _sustainedSources = new Dictionary<object, SustainedSource>();
    private List<object> _expiredSourceKeysScratch;
    private float _effectiveTrauma;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _noiseSeedX = Random.Range(0f, 1000f);
        _noiseSeedY = Random.Range(0f, 1000f);
        _noiseSeedRot = Random.Range(0f, 1000f);

        if (cameraTransform != null)
        {
            _originalLocalPosition = cameraTransform.localPosition;
            _originalLocalRotation = cameraTransform.localRotation;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // Decay trauma over time, always clamped to [0, 1].
        trauma = Mathf.Clamp01(trauma - traumaDecay * dt);

        // Combine with any sustained sources (e.g. a nearby building
        // emerging), dropping ones that haven't checked in recently.
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        float sustainedMax = 0f;
        foreach (var kvp in _sustainedSources)
        {
            if (now - kvp.Value.lastUpdateTime > sustainedSourceTimeout)
            {
                (_expiredSourceKeysScratch ??= new List<object>()).Add(kvp.Key);
                continue;
            }
            sustainedMax = Mathf.Max(sustainedMax, kvp.Value.amount);
        }
        if (_expiredSourceKeysScratch != null)
        {
            foreach (var key in _expiredSourceKeysScratch)
                _sustainedSources.Remove(key);
            _expiredSourceKeysScratch.Clear();
        }

        _effectiveTrauma = Mathf.Max(trauma, sustainedMax);
        float shakeAmount = Mathf.Pow(_effectiveTrauma, traumaExponent);

        if (shakeAmount <= 0.0001f)
        {
            // Settle back exactly to rest to avoid any residual drift.
            cameraTransform.localPosition = _originalLocalPosition;
            cameraTransform.localRotation = _originalLocalRotation;
            return;
        }

        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) * frequency;

        float offsetX = (Mathf.PerlinNoise(_noiseSeedX, t) * 2f - 1f) * maxOffset * shakeAmount;
        float offsetY = (Mathf.PerlinNoise(_noiseSeedY, t) * 2f - 1f) * maxOffset * shakeAmount;
        float rotZ = (Mathf.PerlinNoise(_noiseSeedRot, t) * 2f - 1f) * maxRotation * shakeAmount;

        cameraTransform.localPosition = _originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
        cameraTransform.localRotation = _originalLocalRotation * Quaternion.Euler(0f, 0f, rotZ);
    }

    /// <summary>
    /// Add trauma (0-1). Multiple calls stack (clamped to maxTrauma), so
    /// rapid hits naturally escalate the shake instead of resetting it.
    /// </summary>
    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp(trauma + Mathf.Abs(amount), 0f, maxTrauma);
    }

    /// <summary>
    /// Convenience one-shot shake: sets trauma directly to the given amount
    /// if it's higher than current trauma (won't weaken an existing bigger shake).
    /// </summary>
    public void ShakeOnce(float amount)
    {
        trauma = Mathf.Clamp(Mathf.Max(trauma, Mathf.Abs(amount)), 0f, maxTrauma);
    }

    /// <summary>
    /// Immediately stops all shake and resets trauma to zero, including
    /// any active sustained sources.
    /// </summary>
    public void StopShake()
    {
        trauma = 0f;
        _sustainedSources.Clear();
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = _originalLocalPosition;
            cameraTransform.localRotation = _originalLocalRotation;
        }
    }

    /// <summary>
    /// Drive a continuous shake source (0-1), e.g. "how much trauma this
    /// building's emergence should currently contribute". Call this every
    /// frame the source is active - if it stops being called for longer
    /// than sustainedSourceTimeout, the source is dropped automatically.
    /// `source` is just a key (pass `this` from the calling component) so
    /// multiple sustained sources can be active at once; the strongest one
    /// wins each frame rather than stacking.
    /// </summary>
    public void SetSustainedTrauma(object source, float amount)
    {
        _sustainedSources[source] = new SustainedSource
        {
            amount = Mathf.Clamp01(amount),
            lastUpdateTime = useUnscaledTime ? Time.unscaledTime : Time.time
        };
    }

    /// <summary>
    /// Stop a sustained source immediately (e.g. the building has finished
    /// emerging). Its last trauma value is folded into the normal decaying
    /// pool so the shake eases out naturally instead of cutting off.
    /// </summary>
    public void ClearSustainedTrauma(object source)
    {
        if (_sustainedSources.TryGetValue(source, out var s))
        {
            _sustainedSources.Remove(source);
            trauma = Mathf.Clamp(Mathf.Max(trauma, s.amount), 0f, maxTrauma);
        }
    }

    /// <summary>
    /// Current effective trauma (0-1) after combining decaying impact
    /// trauma with any active sustained sources - this is what actually
    /// drives the shake magnitude this frame.
    /// </summary>
    public float CurrentTrauma => _effectiveTrauma;
}