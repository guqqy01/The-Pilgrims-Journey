using Monsters;
using UnityEngine;

namespace Monsters
{
    [CreateAssetMenu]
    public class MonsterSO : ScriptableObject
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

        [Header("Stats")]
        public float movementSpeed;

        public int health;
        public int shield;
        public float shieldCooldown;
        public float shieldRegeneration;
    }
}


