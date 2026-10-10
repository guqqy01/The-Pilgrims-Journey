using Spawning;
using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    [CreateAssetMenu(menuName = "Spawning/Quest Settings")]
    public class QuestSettings : ScriptableObject
    {
        [Tooltip("How many quests a level offers (different templates, picked by weight).")]
        public ScalingValue questCount = new ScalingValue(2f, 0.1f);

        [AbstractSelector]
        [SerializeReference]
        public List<QuestTemplate> templates = new List<QuestTemplate>();
    }
}