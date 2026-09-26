using Attacks;
using Equipment;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
namespace Monsters
{
    [CreateAssetMenu]
    public class MonsterSO : BuffableAttackEntitySO, IDescriptable, ITileable, IEquipmentHolder, IAttackHolder
    {
        public new string name;
        [TextArea()]
        public string summary;
        [TextArea()]
        public string description;

        public Sprite sprite;

        public Monster prefab;

        public Vector2Int size = Vector2Int.one;

        public MonsterMovementType movementType = MonsterMovementType.Stationary;

        public int powerLevel = 1;


        [Header("Difficulty Scaling")]
        public float movementDifficityScaling = 1f;

        public float healthDifficultyScaling = 1.05f;
        public float shieldDifficultyScaling = 1.05f;
        public float shieldCooldownDifficultyScaling = 1f;
        public float shieldRegenerationDifficultyScaling = 1f;

        public string Title => name;

        public string MainDescription => description;

        public Sprite Sprite => sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => new List<ISubDescriptionData>();

        public string ShortDescription => description;

        [SerializeField]
        private int maxEquipmentCapacity = 1;
        public int MaxEquipmentCapacity => maxEquipmentCapacity;

        private int maxAttackCapacity = 1;
        public int MaxAttackCapacity => maxAttackCapacity;
    }
}


