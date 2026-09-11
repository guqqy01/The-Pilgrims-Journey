using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using LocalMap;

public class FootstepAudio : MonoBehaviour
{
    [SerializeField]
    private Transform movementTransform;
    [SerializeField]
    private List<AudioClipAndSettings> fallbackClips;
    [SerializeField]
    private SerializedDictionary<TileType, List<AudioClipAndSettings>> clips;

    [Tooltip("Actual distance (not squared) between footstep sounds.")]
    [SerializeField]
    private float distanceBetweenStepSounds;

    private Vector2 previousPosition;
    private float currentDistance;

    private void Awake()
    {
        // Seed with the real starting position so the first Update() doesn't
        // measure a spurious jump from (0,0) and fire an instant footstep.
        if (movementTransform != null)
            previousPosition = movementTransform.position;
    }

    private void Update()
    {
        if (movementTransform == null)
            return;

        Vector2 currentPosition = movementTransform.position;
        currentDistance += Vector2.Distance(currentPosition, previousPosition);
        previousPosition = currentPosition;

        if (currentDistance >= distanceBetweenStepSounds)
        {
            PlayStep();
            currentDistance -= distanceBetweenStepSounds;
        }
    }

    private void PlayStep()
    {
        AudioClipAndSettings clip = GetStepClip();
        if (clip == null)
            return;

        AudioManager.Instance.PlayClip(clip);
    }

    private AudioClipAndSettings GetStepClip()
    {
        TileType? tileType = LocalMapGenerator.Instance.GetTileTypeAtPosition(movementTransform.position);

        if (!tileType.HasValue)
        {
            return fallbackClips != null && fallbackClips.Count > 0 ? fallbackClips.GetRandom() : null;
        }

        foreach(var kvp in clips)
        {
            TileType tileT = kvp.Key;
            List<AudioClipAndSettings> clips = kvp.Value;

            if (tileT.HasFlag(tileType.Value) && clips != null && clips.Count > 0)
            {
                return clips.GetRandom();
            }
        }

        return fallbackClips != null && fallbackClips.Count > 0 ? fallbackClips.GetRandom() : null;
    }
}