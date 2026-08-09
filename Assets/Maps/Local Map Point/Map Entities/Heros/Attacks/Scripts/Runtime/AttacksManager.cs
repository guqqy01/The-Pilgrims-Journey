using System;
using System.Collections.Generic;
using UnityEngine;
namespace Attacks
{
    public class AttacksManager : MonoBehaviour
    {
        protected readonly Dictionary<BaseAttackSO, BaseAttack> attacks = new Dictionary<BaseAttackSO, BaseAttack>();

        public event Action<BaseAttack> onAddAttackEvent;
        public event Action<BaseAttack> onRemoveAttackEvent;
        public event Action<BaseAttack> onAttackLevelChangeEvent;

        [SerializeField]
        private LayerMask hitLayerMask;

        private BuffableEntitySO _buffableEntitySO;
        private bool _isSetup = false;

        protected virtual void OnDestroy()
        {
            onAddAttackEvent = null;
            onRemoveAttackEvent = null;
            onAttackLevelChangeEvent = null;
        }

        public void Setup(BuffableEntitySO buffableEntitySO)
        {
            _buffableEntitySO = buffableEntitySO;

            _isSetup = true;
        }

        public bool HasAttack(BaseAttackSO attackSO)
        {
            return attacks.ContainsKey(attackSO);
        }

        public void AddAttack(BaseAttackSO attackSO)
        {
            if (!_isSetup)
            {
                Debug.Log($"AttacksManager.AddAttack: delaying AddAttack for {attackSO.name} until Setup is called");
                return;
            }

            if (attacks.ContainsKey(attackSO))
                return;

            if (!typeof(BaseAttack).IsAssignableFrom(attackSO.AttackType))
            {
                Debug.LogError($"Type {attackSO.AttackType} is not an Attack!");
                return;
            }

            var attack = (BaseAttack)gameObject.AddComponent(attackSO.AttackType);

            attack.Setup(_buffableEntitySO, attackSO, hitLayerMask);

            attacks.Add(attackSO, attack);

            onAddAttackEvent?.Invoke(attack);
        }

        public void RemoveAttack(BaseAttackSO attackSO)
        {
            if (attacks.TryGetValue(attackSO, out BaseAttack heroAttack))
            {
                onRemoveAttackEvent?.Invoke(heroAttack);

                Destroy(heroAttack);

                attacks.Remove(attackSO);
            }
        }

        public int GetAttackLevel(BaseAttackSO attackSO)
        {
            if (attacks.TryGetValue(attackSO, out BaseAttack heroAttack))
            {
                return heroAttack.Level;
            }
            return 0;
        }

        public void LevelUpOrAddAttack(BaseAttackSO attackSO)
        {
            int attackLevel = GetAttackLevel(attackSO);

            if (attackLevel == 0)
            {
                AddAttack(attackSO);
            }
            else
            {
                SetAttackLevel(attackSO, attackLevel + 1);
            }
        }

        public void SetAttackLevel(BaseAttackSO attackSO, int level)
        {
            if (!attacks.ContainsKey(attackSO))
            {
                AddAttack(attackSO);
            }

            if (attacks.TryGetValue(attackSO, out BaseAttack attack))
            {
                attack.SetAttackLevel(level);

                onAttackLevelChangeEvent?.Invoke(attack);
            }
        }
    }
}

