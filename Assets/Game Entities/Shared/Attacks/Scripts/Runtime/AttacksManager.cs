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

        [SerializeField]
        private StatContainer stats;

        public event Action<BaseAttack> OnAddAttackEvent;
        public event Action<BaseAttack> OnRemoveAttackEvent;
        public event Action<BaseAttack> OnAttackLevelChangeEvent;

        [SerializeField]
        private LayerMask hitLayerMask;

        private readonly List<BaseAttackSO> lateAttacks = new List<BaseAttackSO>();

        protected virtual void OnDestroy()
        {
            OnAddAttackEvent = null;
            OnRemoveAttackEvent = null;
            OnAttackLevelChangeEvent = null;
        }

        protected virtual void Awake()
        {
            if (stats == null)
                stats = GetComponent<StatContainer>();

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

            if(stats == null)
            {
                WorkingAddAttack(attackSO, null);
                Debug.Log("Stats are null, ensure setup");
                return;
            }

            if (stats.IsSetup)
            {
                WorkingAddAttack(attackSO, stats.GetBuff);
            }
            else
            {
                stats.OnSetup += AddAllLateAttacks;
                lateAttacks.Add(attackSO);
            }
        }

        private void WorkingAddAttack(BaseAttackSO attackSO, Func<StatTypes, Buff> entityBuffCallback)
        {
            var attack = (BaseAttack)GetGameObjectToHaveAttack().AddComponent(attackSO.AttackType);

            attack.Setup(attackSO, entityBuffCallback, hitLayerMask);

            attacks.Add(attackSO, attack);

            OnAddAttackEvent?.Invoke(attack);
        }

        private void AddAllLateAttacks()
        {
            foreach(var attack in lateAttacks)
            {
                AddAttack(attack);
            }

            lateAttacks.Clear();
            stats.OnSetup -= AddAllLateAttacks;
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
            if (attacks.TryGetValue(attackSO, out BaseAttack baseAttack))
            {
                OnRemoveAttackEvent?.Invoke(baseAttack);

                Destroy(baseAttack);

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

                OnAttackLevelChangeEvent?.Invoke(attack);
            }
        }
    }
}

