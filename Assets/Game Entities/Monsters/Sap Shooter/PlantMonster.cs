using System.Collections;
using UnityEngine;
using Attacks.Projectiles;
using Attacks;
namespace Monsters
{
    public class PlantMonster : Monster
    {
        [SerializeField]
        private AttacksManager attacksManager;

        protected override void ShockEffect(float time)
        {
            attacksManager.DelayAllAttacks(time);
        }
    }
}