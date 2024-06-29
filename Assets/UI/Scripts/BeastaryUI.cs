using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Monsters
{

    public class BeastaryUI : MonoBehaviour
    {
        private BeastaryManager manager;

        public Image monsterImage;

        public Text monsterName;
        public Text monsterSummary;
        public Text monsterDescription;

        public Text encounteredText;
        public Text killedText;
        public Text dmgDealtText;

        private int index = 0;

        private void Start()
        {
            manager = BeastaryManager.Instance;

            manager.updateUI += () => UpdateMonsterUI(manager.GetBeasaryItemFromMonstersIndex(index));

            UpdateMonsterUI(manager.GetBeasaryItemFromMonstersIndex(index));
        }

        public void LeftButtonClick()
        {
            index = Mathf.Clamp(index - 1, 0, manager.beastaryItems.Count - 1);

            UpdateMonsterUI(manager.GetBeasaryItemFromMonstersIndex(index));
        }

        public void RightButtonClick()
        {
            index = Mathf.Clamp(index + 1, 0, manager.beastaryItems.Count - 1);

            UpdateMonsterUI(manager.GetBeasaryItemFromMonstersIndex(index));
        }

        public void UpdateMonsterUI(BeastaryItem item)
        {
            monsterImage.sprite = item.monster.sprite;
            switch (item.LockedState)
            {
                case LockedLevelState.Unknown:
                    monsterImage.color = Color.black;
                    monsterName.text = "???";
                    monsterSummary.text = "?????";
                    monsterDescription.text = "?????";
                    encounteredText.text = "";
                    killedText.text = "";
                    dmgDealtText.text = "";
                    break;
                case LockedLevelState.Locked:
                    monsterImage.color = Color.white;
                    monsterName.text = item.monster.name;
                    monsterSummary.text = item.monster.summary;
                    monsterDescription.text = "Encounter more of these monsters to learn more about them";
                    encounteredText.text = "Number Encountered: " + item.encountered.ToString();
                    killedText.text = "Pilgrims Killed: ??";
                    dmgDealtText.text = "Damage Dealt: ??";
                    break;
                case LockedLevelState.Level1:
                    monsterImage.color = Color.white;
                    monsterName.text = item.monster.name;
                    monsterSummary.text = item.monster.summary;
                    monsterDescription.text = item.monster.description;
                    encounteredText.text = "Number Encountered: " + item.encountered.ToString();
                    killedText.text = "Pilgrims Killed: ??";
                    dmgDealtText.text = "Damage Dealt: ??";
                    break;
                case LockedLevelState.Level2:
                    monsterImage.color = Color.white;
                    monsterName.text = item.monster.name;
                    monsterSummary.text = item.monster.summary;
                    monsterDescription.text = item.monster.description;
                    encounteredText.text = "Number Encountered: " + item.encountered.ToString();
                    killedText.text = "Pilgrims Killed: " + item.NumberKilledTotal.ToString();
                    dmgDealtText.text = "Damage Dealt: " + item.DamageDealtTotal.ToString();
                    break;
                case LockedLevelState.Level3:
                    monsterImage.color = Color.white;
                    monsterName.text = item.monster.name;
                    monsterSummary.text = item.monster.summary;
                    monsterDescription.text = item.monster.description;
                    encounteredText.text = "Number Encountered: " + item.encountered.ToString();
                    killedText.text = "Pilgrims Killed: " + item.NumberKilledTotal.ToString();
                    dmgDealtText.text = "Damage Dealt: " + item.DamageDealtTotal.ToString();
                    break;
            }


        }
    }

}