using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ManagedAudioSource))]
public class AudioResidule : MonoBehaviour
{
    private ManagedAudioSource _audioSource;
    private Transform _target;
    private float _fadeTime;
    private Coroutine _lifecycleCoroutine;
    private Action<AudioResidule> _onFinished;

    public bool IsActive => _lifecycleCoroutine != null;

    private void Awake()
    {
        _audioSource = GetComponent<ManagedAudioSource>();
    }

    public void Play(AudioResiduleSettings settings, Transform target, float startTime, Action<AudioResidule> onFinished)
    {
        _target = target;
        _fadeTime = settings.fadeTime;
        _onFinished = onFinished;

        transform.position = target != null ? target.position : transform.position;

        _audioSource.audioClipSettings = settings.clip;
        _audioSource.Play(startTime);
        if (_lifecycleCoroutine != null)
            StopCoroutine(_lifecycleCoroutine);
        _lifecycleCoroutine = StartCoroutine(Lifecycle());
    }

    /// <summary>Cuts the residule short immediately — e.g. a scene unload or a "mute all" call.</summary>
    public void StopImmediately()
    {
        if (_lifecycleCoroutine != null)
        {
            StopCoroutine(_lifecycleCoroutine);
            _lifecycleCoroutine = null;
        }
        Finish();
    }

    private IEnumerator Lifecycle()
    {
        while (true)
        {
            // Target destroyed underneath us — fade out in place and finish.
            if (_target == null)
            {
                yield return AudioManager.Instance.FadeOutAudioOverTime(_audioSource.Source, _fadeTime);
                Finish();
                yield break;
            }

            transform.position = _target.position;

            // Clip finished naturally, target still alive — just clean up.
            if (!_audioSource.IsPlaying)
            {
                Finish();
                yield break;
            }

            yield return null;
        }
    }

    private void Finish()
    {
        _audioSource.Stop();
        _lifecycleCoroutine = null;
        _target = null;

        var callback = _onFinished;
        _onFinished = null;
        callback?.Invoke(this);
    }
}
