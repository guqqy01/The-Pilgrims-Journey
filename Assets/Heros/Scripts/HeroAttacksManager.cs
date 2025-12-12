using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.Collections;
using UnityEditor.SceneManagement;

namespace Heros
{
    public class HeroAttacksManager : MonoBehaviour
    {
        [SerializeField]
        private Dictionary<HeroAttackSO, HeroAttack> attacks = new Dictionary<HeroAttackSO, HeroAttack>();

        [SerializeField]
        private List<HeroAttackTest> tests = new List<HeroAttackTest>();

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

        public void AddHeroAttack(HeroAttackSO heroAttackSO)
        {
            if (attacks.ContainsKey(heroAttackSO))
                return;

            if (!typeof(HeroAttack).IsAssignableFrom(heroAttackSO.HeroAttackType))
            {
                Debug.LogError($"Type {heroAttackSO.HeroAttackType} is not a HeroAttack!");
                return;
            }

            var attack = (HeroAttack)gameObject.AddComponent(heroAttackSO.HeroAttackType);
            attack.Setup(HeroManager.Instance.heroSO, heroAttackSO);

            attacks.Add(heroAttackSO, attack);
        }

        public void RemoveHeroAttack(HeroAttackSO heroAttackSO)
        {
            if (attacks.TryGetValue(heroAttackSO, out HeroAttack heroAttack))
            {
                Destroy(heroAttack);

                attacks.Remove(heroAttackSO);
            }
        }

        private void ClearAllAttacks()
        {
            foreach (var attack in attacks.Values)
            {
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

