using UnityEngine;
using System.Collections.Generic;
using System.Collections;
namespace Attacks.Spot
{
    public class LightningAttack : BaseRandomMovementSpotAttackSpawn<RandomMovementSpotAttackStats>
    {
        [SerializeField]
        private Animator ani;
        [SerializeField]
        private ConstantShakeSender shaker;

        [SerializeField]
        private float fadeTime = 0.2f;

        protected override void Awake()
        {
            base.Awake();
            shaker.StartShaking();
        }

        private void OnDisable()
        {
            shaker.StopShaking();
        }

        protected override void OnDestroy()
        {
            shaker.StopShaking();
            base.OnDestroy();
        }

        public override void DestroyAttack()
        {
            ani.SetBool("Finished", true);

            spawnAudioSources.ForEach(x => FadePlayingAudio(x));
        }

        private IEnumerator FadePlayingAudio(ManagedAudioSource managedAudioSource)
        {
            if (managedAudioSource.IsPlaying)
            {
                float time = 0f;
                
                while (time < fadeTime)
                {
                    yield return null;
                    time += Time.deltaTime;

                    float multiplier = (fadeTime - time) / fadeTime;

                    managedAudioSource.ApplyVolume(multiplier);
                }
            }
        }
        public void DestroyLightning()
        {
            shaker.StopShaking();
            base.DestroyAttack();
        }
    }
}