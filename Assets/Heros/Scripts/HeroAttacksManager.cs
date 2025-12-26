using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.Collections;
using UnityEditor.SceneManagement;

namespace Heros
{
    public class HeroAttacksManager : Singleton<HeroAttacksManager>
    {
        private Hero ActiveHero => HeroManager.Instance.ActiveHero;

        [SerializeField]
        private Dictionary<HeroAttackSO, HeroAttack> attacks = new Dictionary<HeroAttackSO, HeroAttack>();

        [SerializeField]
        private List<HeroAttackTest> tests = new List<HeroAttackTest>();

        public delegate void OnAddHeroAttack(HeroAttack heroAttack);
        public OnAddHeroAttack onAddHeroAttack;

        public delegate void OnRemoveHeroAttack(HeroAttack heroAttack);
        public OnAddHeroAttack onRemoveHeroAttack;

        public delegate void OnHeroAttackLevelChange(HeroAttack heroAttack);
        public OnHeroAttackLevelChange onHeroAttackLevelChange;

        private void Awake()
        {
            
        }

        private void Update()
        {
            foreach(HeroAttackTest heroAttackTest in tests)
            {
                if (Input.GetKeyDown(heroAttackTest.KeyCode))
                {
                    ClearAllAttacks();

                    AddHeroAttack(heroAttackTest.heroAttackSO);
                }
            }
        }

        public bool HasHeroAttack(HeroAttackSO heroAttackSO)
        {
            return attacks.ContainsKey(heroAttackSO);
        }

        public void AddHeroAttack(HeroAttackSO heroAttackSO)
        {
            if (attacks.ContainsKey(heroAttackSO))
                return;

            if (!typeof(HeroAttack).IsAssignableFrom(heroAttackSO.HeroAttackType))
            {
                Debug.LogError($"Type {heroAttackSO.HeroAttackType} is not a HeroAttack!");
                return;
            }

            var attack = (HeroAttack)ActiveHero.gameObject.AddComponent(heroAttackSO.HeroAttackType);
            attack.Setup(HeroManager.Instance.heroSO, heroAttackSO);

            attacks.Add(heroAttackSO, attack);

            onAddHeroAttack?.Invoke(attack);
        }

        public void RemoveHeroAttack(HeroAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out HeroAttack heroAttack))
            {
                onRemoveHeroAttack?.Invoke(heroAttack);

                Destroy(heroAttack);

                attacks.Remove(heroAttackSO);
            }
        }
        
        public int GetHeroAttackLevel(HeroAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out HeroAttack heroAttack))
            {
                return heroAttack.Level;
            }
            return 0;
        }

        public void LevelUpOrAddHeroAttack(HeroAttackSO heroAttackSO)
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

        public void SetHeroAttackLevel(HeroAttackSO heroAttackSO, int level)
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

    [Serializable]
    internal class HeroAttackTest
    {
        public HeroAttackSO heroAttackSO;
        public KeyCode KeyCode;
    }
}

