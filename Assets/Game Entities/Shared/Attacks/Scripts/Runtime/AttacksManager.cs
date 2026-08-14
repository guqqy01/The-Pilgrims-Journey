using System;
using System.Collections.Generic;
using UnityEngine;


namespace Attacks
{
    public class AttacksManager : MonoBehaviour
    {
        protected readonly Dictionary<BaseAttackSO, BaseAttack> attacks = new Dictionary<BaseAttackSO, BaseAttack>();

        [SerializeField]
        private List<BaseAttackSO> beginningAttacks;

        public event Action<BaseAttack> onAddAttackEvent;
        public event Action<BaseAttack> onRemoveAttackEvent;
        public event Action<BaseAttack> onAttackLevelChangeEvent;

        [SerializeField]
        private LayerMask hitLayerMask;

        protected virtual void OnDestroy()
        {
            onAddAttackEvent = null;
            onRemoveAttackEvent = null;
            onAttackLevelChangeEvent = null;
        }

        protected virtual void Awake()
        {
            if (beginningAttacks == null)
                return;

            foreach (var attackSO in beginningAttacks)
            {
                if (attackSO == null)
                    continue;

                AddAttack(attackSO);
            }
        }

        public bool HasAttack(BaseAttackSO attackSO)
        {
            return attacks.ContainsKey(attackSO);
        }

        public void ActiveAttack(BaseAttackSO baseAttackSO, bool value)
        {
            attacks[baseAttackSO].Active = value;
        }

        public void AttackTriggeredExternally(BaseAttackSO baseAttackSO, bool value)
        {
            attacks[baseAttackSO].AttacksTriggeredExternally = value;
        }

        public void TriggerAttack(BaseAttackSO baseAttackSO)
        {
            attacks[baseAttackSO].ExternalTriggerAttack();
        }

        public void AddAttack(BaseAttackSO attackSO)
        {
            if (attacks.ContainsKey(attackSO))
                return;

            if (!typeof(BaseAttack).IsAssignableFrom(attackSO.AttackType))
            {
                Debug.LogError($"Type {attackSO.AttackType} is not an Attack!");
                return;
            }

            var attack = (BaseAttack)GetGameObjectToHaveAttack().AddComponent(attackSO.AttackType);

            attack.Setup(attackSO, hitLayerMask);

            attacks.Add(attackSO, attack);

            onAddAttackEvent?.Invoke(attack);
        }

        public void DelayAllAttacks(float time)
        {
            foreach(var kvp in attacks)
            {
                var attack = kvp.Value;
                attack.DelayAttack(time);
            }
        }

        protected virtual GameObject GetGameObjectToHaveAttack() { return gameObject; }

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

