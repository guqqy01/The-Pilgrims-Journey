using UnityEngine;

namespace Monsters
{
    public class MonsterAnimationHelper : MonoBehaviour
    {
        public delegate void Helper();
        public Helper onAttack;
        public Helper onAttackFinish;

        public void OnAnimAttack()
        {
            onAttack?.Invoke();
        }

        public void OnAnimAttackFinish()
        {
            onAttackFinish?.Invoke();
        }
    }

}
