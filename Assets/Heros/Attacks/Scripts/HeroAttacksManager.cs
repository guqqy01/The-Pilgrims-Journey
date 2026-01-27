using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.Collections;
using UnityEditor.SceneManagement;

namespace Heros.Attacks
{
    public class HeroAttacksManager : Singleton<HeroAttacksManager>
    {
        private Hero ActiveHero => HeroManager.Instance.ActiveHero;

        [SerializeField]
        private Dictionary<BaseHeroAttackSO, HeroAttack> attacks = new Dictionary<BaseHeroAttackSO, HeroAttack>();

        public delegate void OnAddHeroAttack(HeroAttack heroAttack);
        public OnAddHeroAttack onAddHeroAttack;

        public delegate void OnRemoveHeroAttack(HeroAttack heroAttack);
        public OnAddHeroAttack onRemoveHeroAttack;

        public delegate void OnHeroAttackLevelChange(HeroAttack heroAttack);
        public OnHeroAttackLevelChange onHeroAttackLevelChange;
        
        private void Update()
        {
            if(attacks.Count != 0)
                return;
        }

        private void OnDestroy()
        {
            onAddHeroAttack = null;
            onRemoveHeroAttack = null;
            onHeroAttackLevelChange = null;
        }

        public bool HasHeroAttack(BaseHeroAttackSO heroAttackSO)
        {
            return attacks.ContainsKey(heroAttackSO);
        }

        public void AddHeroAttack(BaseHeroAttackSO heroAttackSO)
        {
            if (attacks.ContainsKey(heroAttackSO))
                return;

            if (!typeof(HeroAttack).IsAssignableFrom(heroAttackSO.HeroAttackType))
            {
                Debug.LogError($"Type {heroAttackSO.HeroAttackType} is not a HeroAttack!");
                return;
            }

            var attack = (HeroAttack)ActiveHero.gameObject.AddComponent(heroAttackSO.HeroAttackType);
            attack.Setup(HeroManager.Instance.HeroSO, heroAttackSO);

            attacks.Add(heroAttackSO, attack);

            onAddHeroAttack?.Invoke(attack);
        }

        

        public void RemoveHeroAttack(BaseHeroAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out HeroAttack heroAttack))
            {
                onRemoveHeroAttack?.Invoke(heroAttack);

                Destroy(heroAttack);

                attacks.Remove(heroAttackSO);
            }
        }
        
        public int GetHeroAttackLevel(BaseHeroAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out HeroAttack heroAttack))
            {
                return heroAttack.Level;
            }
            return 0;
        }

        public void LevelUpOrAddHeroAttack(BaseHeroAttackSO heroAttackSO)
        {
            int heroAttackLevel = GetHeroAttackLevel(heroAttackSO);

            if (heroAttackLevel == 0)
            {
                AddHeroAttack(heroAttackSO);
            }
            else
            {
                SetHeroAttackLevel(heroAttackSO, heroAttackLevel + 1);
            }
        }

        public void SetHeroAttackLevel(BaseHeroAttackSO heroAttackSO, int level)
        {
            if (attacks.TryGetValue(heroAttackSO, out HeroAttack heroAttack))
            {
                heroAttack.SetHeroAttackLevel(level);

                onHeroAttackLevelChange?.Invoke(heroAttack);
            }
        }

        private void ClearAllAttacks()
        {
            foreach (var attack in attacks.Values)
            {
                onRemoveHeroAttack?.Invoke(attack);
                Destroy(attack);
            }
            attacks.Clear();
        }

    }

}

