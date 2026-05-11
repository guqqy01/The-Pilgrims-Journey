using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using Equipment;

namespace Tiles
{
    public class UnitTileUI : TileUI<UnitSO>, IDescriptable
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
                return Tile.Item;
            }
            set
            {
                UnitTile.SetupNewUnitSO(value);

                if (unitGraphics == null)
                    unitGraphics = unitUITransform.GetComponent<Image>();

                unitGraphics.sprite = Tile.Item.sprite;
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

        public override void BaseInit(Tile<UnitSO> tile)
        {
            base.BaseInit(tile);
            unitGraphics = unitUITransform.GetComponent<Image>();
            unitGraphics.sprite = Tile.Item.sprite;
            tileShapeTextureBuilder.Build(tile.Shape);
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

