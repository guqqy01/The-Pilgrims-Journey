using UnityEngine;
using System.Collections.Generic;
namespace TPJ.UI
{
    public class PointHolderUI : MonoBehaviour
    {
        [SerializeField]
        private CardPointUI pointUIPrefab;
        private readonly List<CardPointUI> _pointUIs = new List<CardPointUI>();
        public void CreatePointUIs(int maxPoints, int currentPoints)
        {
            _pointUIs.DestroyAndClearList();

            int max = maxPoints;
            int current = currentPoints;

            int amountOfMaxWhole = Mathf.FloorToInt(max / 2f);
            int amountOfCurrentWhole = Mathf.FloorToInt(current / 2f);
            bool hasHalfMax = max.Odd();
            bool hasHalfCurrent = current.Odd();

            for (int i = 0; i < amountOfCurrentWhole; i++)
            {
                CardPointUI pointUI = Instantiate(pointUIPrefab, transform);
                pointUI.SetFilled(PointUIType.FullFull);
                _pointUIs.Add(pointUI);
            }

            if (hasHalfCurrent)
            {
                if (amountOfMaxWhole == amountOfCurrentWhole)
                {
                    CardPointUI pointUI = Instantiate(pointUIPrefab, transform);
                    pointUI.SetFilled(PointUIType.HalfFull);
                    _pointUIs.Add(pointUI);
                }
                else
                {
                    CardPointUI pointUI = Instantiate(pointUIPrefab, transform);
                    pointUI.SetFilled(PointUIType.FullHalf);
                    _pointUIs.Add(pointUI);
                }
            }

            int modifier = hasHalfCurrent ? -1 : 0;

            for (int i = 0; i < amountOfMaxWhole - amountOfCurrentWhole + modifier; i++)
            {
                CardPointUI pointUI = Instantiate(pointUIPrefab, transform);
                pointUI.SetFilled(PointUIType.FullEmpty);
                _pointUIs.Add(pointUI);
            }

            if (hasHalfMax && amountOfMaxWhole != amountOfCurrentWhole)
            {
                CardPointUI pointUI = Instantiate(pointUIPrefab, transform);
                pointUI.SetFilled(PointUIType.HalfEmpty);
                _pointUIs.Add(pointUI);
            }
        }
    }
}
