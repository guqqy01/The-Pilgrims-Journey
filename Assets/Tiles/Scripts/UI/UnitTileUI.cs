using Equipment;
using System.Collections.Generic;
using System.Linq;
using Units;
using UnityEngine;
using UnityEngine.UI;

namespace Tiles
{
    public class TilePlayerTileUI : TileUI<ITilePlayerTileItem>
    {

    }

    public class UnitTileUI : TilePlayerTileUI, IDescriptable
    {
        
        [SerializeField]
        private RectTransform unitUITransform;
        private Image unitGraphics;

        private UnitTile _unitTile;
        public UnitTile UnitTile 
        { 
            get 
            { 
                if(_unitTile == null)
                {
                    if(Tile is UnitTile unitTile)
                    {
                        _unitTile = unitTile;
                    }
                    else
                    {
                        return null;
                    }
                }
                return _unitTile; 
            } 
        }

        public EquipmentSO[] Equipment => UnitTile.Equipment;

        public UnitSO UnitSO
        {
            get
            {
                if (Tile.Item is not UnitSO unitSO)
                    return null;

                return unitSO;
            }
        }

        private void Awake()
        {
            unitGraphics = unitUITransform.GetComponent<Image>();
        }

        public void OnUnitSpawn()
        {
            UnitTile.OnUnitSpawn();
        }

        public override void Rotate(int angle)
        {
            base.Rotate(angle);
            unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public override void RotateTo(float angle)
        {
            base.RotateTo(angle);
            unitUITransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public override void BaseInit(Tile<ITilePlayerTileItem> tile)
        {
            if (tile is not UnitTile unitTile)
                return;

            base.BaseInit(unitTile);

            unitGraphics = unitUITransform.GetComponent<Image>();
            unitGraphics.sprite = unitTile.UnitSO.sprite;
            tileShapeTextureBuilder.Build(tile.Shape);
            unitGraphics.rectTransform.anchoredPosition = GetCenteredAnchoredPositionPositionInShape(tile.Shape);
        }

        

        #region Stats

        public float SpawnRateValue => UnitTile.SpawnRate;
        public float MovementValue => UnitTile.Movement;
        public int HealthValue => UnitTile.Health;
        public int ShieldValue => UnitTile.Shield;
        public float ShieldRegenerationValue => UnitTile.ShieldRegen;
        public float ShieldCooldownValue => UnitTile.ShieldCooldown;

        public string Title => UnitSO.Title;

        public string MainDescription => UnitSO.MainDescription;

        public Sprite Sprite => UnitSO.Sprite;

        public List<ISubDescriptionData> GetSubDescriptionData => UnitSO.GetSubDescriptionData;

        #endregion
    }
 }

