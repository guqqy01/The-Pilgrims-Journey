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

        protected void Awake()
        {
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
        }

        public void DestroyLightning()
        {
            shaker.StopShaking();
            base.DestroyAttack();
        }
    }
}