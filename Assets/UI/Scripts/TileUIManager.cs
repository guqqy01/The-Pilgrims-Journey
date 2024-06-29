using Equipment;
using Map;
using System;
using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tiles
{

    public class TileUIManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
    {
        public static TileUIManager Instance { private set; get; }

        private InputManager inputManager;

        [SerializeField]
        public Canvas canvas;
        public static Canvas Canvas => Instance.canvas;

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
    }
}


