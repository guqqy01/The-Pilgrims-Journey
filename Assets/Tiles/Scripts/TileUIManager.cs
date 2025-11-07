using Equipment;
using GlobalMap;
using System;
using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.UIElements;
using static Tiles.Tile;

namespace Tiles
{

    public class TileUIManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
    {
        public static TileUIManager Instance { private set; get; }

        private InputManager inputManager;

        [SerializeField]
        public Canvas canvas;
        public static Canvas Canvas => Instance.canvas;

        private TileRotationHelper rotationHelper;
        [SerializeField]
        private TileRotationHelper rotationHelperPrefab;

        private Tile draggingTile;

        [SerializeField]
        private List<TileSO> tilesNotToShowHelper = new List<TileSO>();

        private void Start()
        {
            inputManager = InputManager.Instance;
        }

        public void InitiateSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }


        protected void Awake()
        {
            Tile.onDragTile += DraggingTile;

        }

        protected void OnDestroy()
        {
            Tile.onDragTile -= DraggingTile;
        }

        private void Update()
        {
            if(draggingTile != null)
                rotationHelper.Follow(draggingTile);
        }


        private void DraggingTile(Tile tile)
        {
            if (tile != null && tilesNotToShowHelper.Contains(tile.TileSO))
            {
                tile = null;
            }

            if(rotationHelper == null && tile != null)
            {
                rotationHelper = Instantiate(rotationHelperPrefab, canvas.transform);
            }

            draggingTile = tile;

            if (rotationHelper == null)
                return;

            if (tile == null)
            {
                rotationHelper.gameObject.SetActive(false);
                return;
            }

            rotationHelper.gameObject.SetActive(true);
        }
    }
}


