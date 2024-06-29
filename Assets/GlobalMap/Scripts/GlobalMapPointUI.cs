using System;
using UnityEngine;

namespace Map
{
    public class GlobalMapPointUI : MonoBehaviour
    {
        public GlobalMapPoint Point { private set; get; }
        private GlobalMapUI globalMapUI;

        [SerializeField]
        private GlobalMapPointText normalGO;
        [SerializeField]
        private GlobalMapPointText randomGO;
        [SerializeField]
        private GlobalMapPointText shopGO;
        [SerializeField]
        private GlobalMapPointText relicGO;

        public Color Color
        {
            set
            {
                normalGO.Color = value;
                randomGO.Color = value;
                shopGO.Color = value;
                relicGO.Color = value;
            }
        }

        public void SetMapPointText(GlobalMapPointText pointText)
        {
            normalGO.gameObject.SetActive(pointText == normalGO);
            randomGO.gameObject.SetActive(pointText == randomGO);
            shopGO.gameObject.SetActive(pointText == shopGO);
            relicGO.gameObject.SetActive(pointText == relicGO);
        }

        public void UpdateUI(GlobalMapPoint point, GlobalMapUI globalMapUI)
        {
            this.Point = point;
            this.globalMapUI = globalMapUI;

            switch (point.Type)
            {
                case GlobalMapPointType.Normal:
                    SetMapPointText(normalGO);
                    normalGO.transform.localScale = new Vector3(globalMapUI.mapPointSize, globalMapUI.mapPointSize, globalMapUI.mapPointSize);
                    break;
                case GlobalMapPointType.Random:
                    SetMapPointText(randomGO);
                    randomGO.transform.localScale = new Vector3(globalMapUI.mapPointSize, globalMapUI.mapPointSize, globalMapUI.mapPointSize);
                    break;
                case GlobalMapPointType.Shop:
                    SetMapPointText(shopGO);
                    shopGO.transform.localScale = new Vector3(globalMapUI.mapPointSize, globalMapUI.mapPointSize, globalMapUI.mapPointSize);
                    break;
                case GlobalMapPointType.Relic:
                    SetMapPointText(relicGO);
                    relicGO.transform.localScale = new Vector3(globalMapUI.mapPointSize, globalMapUI.mapPointSize, globalMapUI.mapPointSize);
                    break;
            }

            Color = globalMapUI.typeAndSettings[point.Type].Color;
        }


        public void PlayReachPointAnimation()
        {

        }
    }

    [Serializable]
    public class GlobalMapPointUISettings
    {
        public char Character;
        public Vector2 Offset;
        public Color Color;
    }

}
