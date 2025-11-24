using UnityEngine;

namespace Heros
{
    [CreateAssetMenu()]
    public class HeroSO : ScriptableObject
    {
        [TextArea()]
        public string Description;

        public Hero prefab;


    }

}
