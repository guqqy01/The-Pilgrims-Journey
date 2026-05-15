using Equipment;
using GlobalMap;
using System;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Tiles
{

    public class TileUIManager : Singleton<TileUIManager>
    {
        private InputManager inputManager;

        [SerializeField]
        public Canvas canvas;
        public static Canvas Canvas => Instance.canvas;

        private TileRotationHelper rotationHelper;
        [SerializeField]
        private TileRotationHelper rotationHelperPrefab;

        private TileUIBase draggingTile;

        [SerializeField]
        private List<TileSO> tilesNotToShowHelper = new List<TileSO>();

        private void Start()
        {
            inputManager = InputManager.Instance;
        }

        protected void Awake()
        {
            TileUIEventBus.OnDragTile += DraggingTile;
        }

        protected override void CleanUp()
        {
            TileUIEventBus.OnDragTile -= DraggingTile;
        }

        private void OnDestroy()
        {
            TileUIEventBus.Clear();
        }

        private void Update()
        {
            if(draggingTile != null)
                rotationHelper.Follow(draggingTile);
        }


        private void DraggingTile(TileUIBase tileUI)
        {
            if (tileUI != null && tilesNotToShowHelper.Contains(tileUI.Tile.TileSO))
            {
                tileUI = null;
            }

            if(rotationHelper == null && tileUI != null)
            {
                rotationHelper = Instantiate(rotationHelperPrefab, canvas.transform);
            }

            draggingTile = tileUI;

            if (rotationHelper == null)
                return;

            if (tileUI == null)
            {
                rotationHelper.gameObject.SetActive(false);
                return;
            }

            rotationHelper.gameObject.SetActive(true);
        }
    }
}


