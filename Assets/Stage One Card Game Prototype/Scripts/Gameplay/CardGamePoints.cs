using System;
using UnityEngine;

namespace TPJ.Gameplay
{
    public class CardGamePoints : MonoBehaviour
    {
        [SerializeField]
        private DeckHolder owner;

        private int _currentPoints;
        [SerializeField]
        private int maxPoints = 14;
        public int CurrentPoints => _currentPoints;
        public int MaxPoints => maxPoints;
        public event Action OnPointValueChange;

        public readonly BuffableStat PointGenerationPerRound = new BuffableStat(2);

        private void Awake() => CardGameEventsBus.OnBeginTurn += GenerateFromNewRound;
        public bool HasEnoughPoints(int value)
        {
            return _currentPoints >= value;
        }

        public void Reset()
        {
            _currentPoints = 0;
            OnPointValueChange?.Invoke();
        }

        public void GenerateFromNewRound(DeckHolder holder)
        {
            if (holder != owner)
                return;

            ModifyPoints(PointGenerationPerRound.Value);
        }

        public void ModifyPoints(int value)
        {
            _currentPoints = Mathf.Clamp(_currentPoints + value, 0, maxPoints);
            OnPointValueChange?.Invoke();
        }

        private void OnDestroy()
        {
            CardGameEventsBus.OnBeginTurn -= GenerateFromNewRound;
            OnPointValueChange = null;
        }
    }
}