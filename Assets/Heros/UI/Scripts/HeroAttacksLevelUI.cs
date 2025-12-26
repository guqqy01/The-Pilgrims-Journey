using Heros;
using System.Collections.Generic;
using UnityEngine;

public class HeroAttacksLevelUI : MonoBehaviour
{
    [SerializeField]
    private HeroAttackItemUI heroAttackItemPrefab;

    [SerializeField]
    private Transform heroAttacksHolderTransform;

    private Dictionary<HeroAttack, HeroAttackItemUI> heroAttackItemUIs = new Dictionary<HeroAttack, HeroAttackItemUI>();

    private void Awake()
    {
        //HeroAttacksManager.Instance.OnAddedHeroAttack += CreateHeroAttackItemUI;
        //HeroAttacksManager.Instance.OnHeroAttackLevelChange += UpdateHeroAttackItemUI;
    }

    public void CreateHeroAttackItemUI(HeroAttack heroAttack)
    {
        if(heroAttackItemUIs.ContainsKey(heroAttack))
        {
            if(heroAttackItemUIs[heroAttack] != null)
            {
                UpdateHeroAttackItemUI(heroAttack);
                return;
            }
            else
            {
                heroAttackItemUIs.Remove(heroAttack);
            }
        }

        HeroAttackItemUI heroAttackItemUI = Instantiate(heroAttackItemPrefab, heroAttacksHolderTransform);
        heroAttackItemUI.Setup(heroAttack);


        heroAttackItemUIs.Add(heroAttack, heroAttackItemUI);
    }

    public void UpdateHeroAttackItemUI(HeroAttack heroAttack)
    {
        //heroAttackItemUIs[heroAttack].UpdateUI(heroAttack);
    }
}
