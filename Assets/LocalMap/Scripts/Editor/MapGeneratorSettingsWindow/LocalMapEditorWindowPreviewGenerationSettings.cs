using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace LocalMap.Editor
{
    [Serializable]
    public class LocalMapEditorWindowPreviewGenerationSettings : IEditorMenuToolbarTab
    {
        public string Name => "Preview Generation";

        [SerializeField]
        private Direction _startDirection;
        public Direction StartWall => _startDirection;

        [SerializeField]
        private FlagedDirection _toDirections;
        public List<Direction> ToDirections => _toDirections.ConvertToListOfDirections();

        [SerializeField]
        private FlagedDirection _otherFromDirections;
        public List<Direction> OtherFromDirections => _otherFromDirections.ConvertToListOfDirections();

        public int? NullableSeed
        {
            get
            {
                if (_useRandomSeed)
                {
                    return null;
                }
                return _seed;
            }
        }

        [SerializeField]
        private int _seed;
        [SerializeField]
        private bool _useRandomSeed = true;

        public void DrawTab(Rect rect)
        {
            Rect startWallRect = new Rect(rect) {yMin = rect.yMin + 2f, height = EditorGUIUtility.singleLineHeight };
            Rect toDirectionsRect = new Rect(rect) { yMin = startWallRect.yMax + 2f,  height = EditorGUIUtility.singleLineHeight };
            Rect otherFromDirectionsRect = new Rect(rect) { yMin = toDirectionsRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };
            Rect useRandomSeedRect = new Rect(rect) { yMin = otherFromDirectionsRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };
            Rect seedRect = new Rect(rect) { yMin = useRandomSeedRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

            _startDirection = (Direction)EditorGUI.EnumPopup(startWallRect, new GUIContent("Start Wall"), _startDirection);

            _toDirections = (FlagedDirection)EditorGUI.EnumFlagsField(toDirectionsRect, new GUIContent("To Directions"), _toDirections);

            _otherFromDirections = (FlagedDirection)EditorGUI.EnumFlagsField(otherFromDirectionsRect, new GUIContent("Other From Directions"), _otherFromDirections);

            _useRandomSeed = EditorGUI.Toggle(useRandomSeedRect, new GUIContent("Use a random seed for genneration"), _useRandomSeed);

            if (!_useRandomSeed)
            {
                _seed = EditorGUI.IntField(seedRect, new GUIContent("Seed"), _seed);
            }
        }

        public float GetHeight(Rect rect)
        {
            float height = EditorGUIUtility.singleLineHeight * (_useRandomSeed ? 4f : 5f) + 5f * 2f;

            return MathF.Max(rect.height, height);
        }
    }
}
