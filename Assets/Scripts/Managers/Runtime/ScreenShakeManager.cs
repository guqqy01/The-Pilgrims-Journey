using System.Collections.Generic;
using UnityEngine;

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

    private float _noiseSeedX;
    private float _noiseSeedY;
    private float _noiseSeedRot;

    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;

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

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp(trauma + Mathf.Abs(amount), 0f, maxTrauma);
    }

    public void ShakeOnce(float amount)
    {
        trauma = Mathf.Clamp(Mathf.Max(trauma, Mathf.Abs(amount)), 0f, maxTrauma);
    }

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
    public void SetSustainedTrauma(object source, float amount)
    {
        _sustainedSources[source] = new SustainedSource
        {
            amount = Mathf.Clamp01(amount),
            lastUpdateTime = useUnscaledTime ? Time.unscaledTime : Time.time
        };
    }

    public void ClearSustainedTrauma(object source)
    {
        if (_sustainedSources.TryGetValue(source, out var s))
        {
            _sustainedSources.Remove(source);
            trauma = Mathf.Clamp(Mathf.Max(trauma, s.amount), 0f, maxTrauma);
        }
    }
    public float CurrentTrauma => _effectiveTrauma;
}