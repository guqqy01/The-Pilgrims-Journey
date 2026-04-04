using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DialogueSystem
{
    public class SectionalAudioClipEditorViewer
    {
        private Color backgroundColorInner = (Color.grey * 0.5f).SetAlpha(1f);
        private Color backgroundColorOuter = Color.black;

        private readonly SectionalAudioClip sectionalAudioClip;
        private readonly AudioClipWaveData waveData;

        public Action onClose;

        private TimeLineDraggableArea timelineArea;

        private Rect areaRect;
        Rect timelineRect => new Rect(areaRect) { height = EditorGUIUtility.singleLineHeight * 2 };
        Rect viewerRect => new Rect(areaRect) { yMin = timelineRect.yMax };


        private readonly List<IDraggableRectTimeItem> timeStartAndEnd;

        public SectionalAudioClipEditorViewer(SectionalAudioClip editingSectionalAudioClip, AudioClipWaveData audioClipWaveData, Action OnClose)
        {
            sectionalAudioClip = editingSectionalAudioClip;
            waveData = audioClipWaveData;
            onClose = OnClose;

            timelineArea = new TimeLineDraggableArea(editingSectionalAudioClip.audioClip.length, false, 0f)
            {
                defaultDraggableRectSizes = new Vector2(10f, 15f),
                defaultInnerColor = Color.white,
                defaultIgnoreOtherDraggableRects = true,
                backgroundColorInner = (Color.grey * 0.5f).SetAlpha(1f),
                beforeDraw = DrawZoomedMixerGroupLines,
                limitXPositionToRect = false,
                
            };

            timelineArea.DrawTimeline = true;

            timeStartAndEnd = new List<IDraggableRectTimeItem>() 
            { 
                new DraggableTimeItemFloat(editingSectionalAudioClip.MinTime, DraggableRectAxis.Horizontal, IDraggableRectTimeItem.TimePosition.Center), 
                new DraggableTimeItemFloat(editingSectionalAudioClip.MaxTime, DraggableRectAxis.Horizontal, IDraggableRectTimeItem.TimePosition.Center) 
            };
        }

        public void Draw(Event e, Rect rect)
        {
            if(areaRect != rect)
                areaRect = rect;

            Handles.color = Color.white;

            Handles.DrawSolidRectangleWithOutline(timelineRect, backgroundColorInner, backgroundColorOuter);

            SectionalAudioClipDrawer.DrawAudioClip(viewerRect, waveData[(int)WaveDataLOD.High], DrawMethod.Fade, sectionalAudioClip.MinValue, sectionalAudioClip.MaxValue);

            HandleDialogueItemsZoomedDraggableArea(e);

            if (GUI.Button(new Rect(rect) { yMin = rect.yMax - 20 }, "Exit"))
            {
                onClose?.Invoke();
            }
        }


        private void HandleDialogueItemsZoomedDraggableArea(Event e)
        {
            if (timelineArea == null)
                return;

            timelineArea.timeScale = sectionalAudioClip.audioClip.length;
            timelineArea.timeStart = 0f;

            timelineArea.AreaRect = timelineRect;

            timelineArea.HandleDraggableRects(e, timeStartAndEnd);

            List<TimeLineDraggableRect> draggableRects = timelineArea.TimelineDraggableRects;

            draggableRects.Sort();

            sectionalAudioClip.MinTime = draggableRects[0].Time;
            sectionalAudioClip.MaxTime = draggableRects[1].Time;

        }

        private void DrawZoomedMixerGroupLines(Rect rect)
        {
            Handles.color = Color.blue;
            float width = 2f;

            float yMin = areaRect.yMin;
            float yMax = areaRect.yMax;

            foreach (DraggableRect draggableRect in timelineArea.GetDraggableRects(2))
            {
                float xPos = draggableRect.rect.center.x;

                Rect rectangle = new Rect() { xMin = xPos - width / 2f, xMax = xPos + width / 2f, yMin = yMin, yMax = yMax };

                EditorGUI.DrawRect(rectangle, Color.cyan);
            }
        }
    }

    
}

