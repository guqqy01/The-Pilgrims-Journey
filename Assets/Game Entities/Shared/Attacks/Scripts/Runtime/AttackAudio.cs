using System.Collections.Generic;
using UnityEngine;

namespace Attacks
{
    public class AttackAudio : MonoBehaviour
    {
        [SerializeField]
        private AttackSpawnBase attack;

        [SerializeField]
        private List<AudioResiduleSettings> spawnAudioSources;

        [SerializeField]
        private List<AudioResiduleSettings> hitAudioSources;

        private void Awake()
        {
            if (attack != null)
            {
                attack.OnSpawn += HandleAudioOnSpawn;
                attack.OnHit += HandleAudioOnHit;
            }
            HandleAudioOnSpawn();
        }

        private void OnDestroy()
        {
            if (attack != null)
            {
                attack.OnSpawn -= HandleAudioOnSpawn;
                attack.OnHit -= HandleAudioOnHit;
            }
            // No takeover/rescue logic needed here — spawn and hit audio are already
            // played on independent AudioResidule objects, so destroying this object
            // never cuts anything off.
        }

        private void HandleAudioOnSpawn()
        {
            if (spawnAudioSources == null || AudioResiduleManager.Instance == null)
                return;

            for (int i = 0; i < spawnAudioSources.Count; i++)
            {
                var settings = spawnAudioSources[i];
                if (settings?.clip?.clip == null) continue;
                AudioResiduleManager.Instance.CreateResidule(settings, transform);
            }
        }

        private void HandleAudioOnHit(OnHitInfo hitInfo)
        {
            if (hitAudioSources == null || hitAudioSources.Count == 0 || AudioResiduleManager.Instance == null)
                return;

            AudioResiduleSettings toPlay = null;
            for (int i = 0; i < hitAudioSources.Count; i++)
            {
                if (hitAudioSources[i]?.clip?.clip != null)
                {
                    toPlay = hitAudioSources[i];
                    break;
                }
            }

            if (toPlay != null)
                AudioResiduleManager.Instance.CreateResidule(toPlay, transform);
        }
    }
}
