using UnityEngine;
using GameManagerCalls;
namespace Heros
{
    public class HeroManager : Singleton<HeroManager>, IOnAfterCreateLocalMap
    {
        public Hero hero;


    }

}
