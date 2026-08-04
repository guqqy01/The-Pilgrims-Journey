using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
    public class SectionalAudioClipTimeline
    {
        private readonly AudioDialogueEditorWindow parentWindow;

        public readonly SectionalAudioClipDraggableArea sectionalAudioClipsDraggableArea;
        private SectionalAudioClipDraggableRect currentlyEditingSectionalAudioClipRect;

        private readonly TimeLineDraggableArea dialogueMixerGroupDraggableArea;
        private TimeLineDraggableRect currentlyEditingMixerGroupRect;

        private float mainTimeLineLength => parentWindow.mainTimeLineLength;

        private const float timelineHeight = 36f;
        private const float space = 5f;

        public delegate void ClearOtherDialougueMixerGroups();
        public ClearOtherDialougueMixerGroups clearOtherDialougueMixerGroups;

        private Rect rect;
        private readonly AudioDialogue audioDialogue;

        public bool Repaint;

        private Rect TimeLineRect(Rect rect) => new Rect(rect) { yMax = rect.yMin + timelineHeight };
        private Rect SectionalAudioClipsAreaRect(Rect rect) => new Rect(rect) { yMin = rect.yMin + timelineHeight};

        public SectionalAudioClipTimeline(AudioDialogueEditorWindow parentWindow, AudioDialogue audioDialogue)
        {
            this.parentWindow = parentWindow;
            this.audioDialogue = audioDialogue;

            sectionalAudioClipsDraggableArea = new SectionalAudioClipDraggableArea(mainTimeLineLength, true)
            {
                backgroundColorInner = (Color.grey * 0.2f).SetAlpha(1f),
                rightClickRectOptions = new Dictionary<string, GenericMenu.MenuFunction>()
                {
                    { "Edit", EditAudioClip},
                    { "Split", SplitAudioClip},
                    { "Remove", () => RemoveAudioClip(audioDialogue)}
                },
                onRightClickRect = SectionalAudioClipOnRightClickRect
            };

            //

            dialogueMixerGroupDraggableArea = new TimeLineDraggableArea(mainTimeLineLength, false, 0f)
            {
                defaultDraggableRectSizes = new Vector2(10f, 10f),
                defaultInnerColor = Color.white,
                defaultIgnoreOtherDraggableRects = true,
                backgroundColorInner = Color.clear,
                rightClickRectOptions = new Dictionary<string, GenericMenu.MenuFunction>()
                    {
                        { "Remove Mixer Group", () => RemoveDialogueItem(audioDialogue)},
                        { "Edit Mixer Group", EditDialogueItem},
                    },
                rightClickSpaceOptions = new Dictionary<string, GenericMenu.MenuFunction>()
                    {
                        { "Add Mixer Group", () => HandleAddDialogueItem(audioDialogue)},
                    },
                onRightClickRect = OnRightClickRect,
            };
        }

        public void ClearDialogueMixerGroups()
        {
            dialogueMixerGroupDraggableArea.Clear(2);
        }

        public void Draw(Rect rect)
        {
            if(this.rect != rect)
                this.rect = rect;

            Event e = Event.current;

            HandleAudioClipDraggableArea(e);

            DrawMixerGroupLines();
            TimelineDrawer.DrawTimeLine(rect, mainTimeLineLength, 0f);
            //DrawTimeLine(mainTimeLineLength, 0f);

            HandleDialogueItemsDraggableArea(e);

            AddDraggedAudioClip(e);

            Repaint = dialogueMixerGroupDraggableArea.DraggingRect || sectionalAudioClipsDraggableArea.DraggingRect;
        }

        private void DrawMixerGroupLines()
        {
            Handles.color = Color.blue;
            float width = 2f;
            float yMin = rect.yMin;
            float yMax = rect.yMax;

            foreach (DraggableRect draggableRect in dialogueMixerGroupDraggableArea.GetDraggableRects(2))
            {
                if (draggableRect is TimeLineDraggableRect)
                {
                    float xPos = draggableRect.rect.center.x;

                    Rect rectangle = new Rect() { xMin = xPos - width / 2f, xMax = xPos + width / 2f, yMin = yMin, yMax = yMax };

                    EditorGUI.DrawRect(rectangle, Color.cyan);
                }
            }
        }


        private void HandleDialogueItemsDraggableArea(Event e)
        {
            if (dialogueMixerGroupDraggableArea == null)
                return;

            dialogueMixerGroupDraggableArea.AreaRect = TimeLineRect(rect);
            dialogueMixerGroupDraggableArea.HandleDraggableRects(e, audioDialogue.audioDialogueMixerGroups);
        }

        private void OnRightClickRect(DraggableRect rect)
        {
            if (dialogueMixerGroupDraggableArea.RectRightClicked is TimeLineDraggableRect timeLineDraggableRect)
            {
                currentlyEditingMixerGroupRect = timeLineDraggableRect;
            }
        }

        private void HandleAddDialogueItem(AudioDialogue audioDialogue)
        {
            AudioDialogueMixerGroup newMixerGroup = new AudioDialogueMixerGroup();
            newMixerGroup.Time = Mathf.Lerp(0, mainTimeLineLength, Mathf.InverseLerp(0, TimeLineRect(rect).width, dialogueMixerGroupDraggableArea.PositionClicked.x));
            audioDialogue.audioDialogueMixerGroups.Add(newMixerGroup);
        }

        private void RemoveDialogueItem(AudioDialogue audioDialogue)
        {
            if (dialogueMixerGroupDraggableArea.RectRightClicked is TimeLineDraggableRect timeLineDraggableRect)
            {
                audioDialogue.audioDialogueMixerGroups.Remove((AudioDialogueMixerGroup)timeLineDraggableRect.ListOverTimeItem);

                dialogueMixerGroupDraggableArea.RemoveDraggableRect(audioDialogue.audioDialogueMixerGroups, timeLineDraggableRect);
                //dialogueMixerGroupZoomedDraggableArea.Clear(2);
                clearOtherDialougueMixerGroups?.Invoke();
            }
        }

        private void EditDialogueItem()
        {
            float timeValue = currentlyEditingMixerGroupRect.ListOverTimeItem.StartTime;
            float width = 200f;
            float xPos = timeValue / mainTimeLineLength * rect.width;
            Rect rectangle = new Rect() { yMin = TimeLineRect(rect).yMax, height = EditorGUIUtility.singleLineHeight, xMin = Mathf.Clamp(xPos - width / 2f - space, space, TimeLineRect(rect).xMax - width - 3f * space), width = width + 2f * space };
            UnityEditor.PopupWindow.Show(rectangle, new DialogueMixerGroupEditorWindowDropDown((AudioDialogueMixerGroup)currentlyEditingMixerGroupRect.ListOverTimeItem, parentWindow));
        }

        public float TimeValueOnMainTimeLineFromPosition(float xPos)
        {
            return mainTimeLineLength * (xPos / rect.width);
        }

        public float PositionOnMainTimeLineFromTime(float time)
        {
            return rect.width * (time / mainTimeLineLength);
        }

        private void HandleAudioClipDraggableArea(Event e)
        {
            if (sectionalAudioClipsDraggableArea == null)
                return;

            sectionalAudioClipsDraggableArea.AreaRect = SectionalAudioClipsAreaRect(rect);

            sectionalAudioClipsDraggableArea.HandleDraggableRects(e, audioDialogue.sectionalAudioClips);
        }

        private void SectionalAudioClipOnRightClickRect(DraggableRect rect)
        {
            if (sectionalAudioClipsDraggableArea.RectRightClicked is SectionalAudioClipDraggableRect timeLineDraggableRect)
            {
                currentlyEditingSectionalAudioClipRect = timeLineDraggableRect;
            }
        }

        private void AddDraggedAudioClip(Event e)
        {
            EventType eventType = e.type;
            if (eventType == EventType.DragUpdated ||
                eventType == EventType.DragPerform)
            {
                // Show a copy icon on the drag
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is AudioClip audioClip)
                        {
                            AddAudioClip(audioDialogue, audioClip);
                        }
                        else
                        {
                            Debug.LogWarning("Dragged object needs to be an AudioClip");
                        }
                    }
                }

                e.Use();
            }
        }

        private void AddAudioClip(AudioDialogue audioDialogue, AudioClip audioClip)
        {
            if (audioDialogue == null)
                return;

            Debug.Log($"Adding audioClip: {audioClip.name}");
            Debug.Log($"Fix time parameter below");
            audioDialogue.AddSectionAudioClip(audioClip, 0, 0, 1);
        }



        private void EditAudioClip()
        {
            PopupWindow.Show(currentlyEditingSectionalAudioClipRect.rect, new SectionalAudioClipEditDropDownWindow(currentlyEditingSectionalAudioClipRect.sectionalAudioClipViewer, parentWindow));
        }

        private void SplitAudioClip()
        {
            SectionalAudioClip[] sectionalAudioClips = currentlyEditingSectionalAudioClipRect.sectionalAudioClipViewer.sectionalAudioClip.Split(0.5f);

            RemoveAudioClip(audioDialogue);
            audioDialogue.sectionalAudioClips.AddRange(sectionalAudioClips);
        }

        private void RemoveAudioClip(AudioDialogue audioDialogue)
        {
            if (sectionalAudioClipsDraggableArea.RectRightClicked is TimeLineDraggableRect timeLineDraggableRect)
            {
                audioDialogue.sectionalAudioClips.Remove((SectionalAudioClip)timeLineDraggableRect.ListOverTimeItem);

                sectionalAudioClipsDraggableArea.RemoveDraggableRect(audioDialogue.sectionalAudioClips, timeLineDraggableRect);
            }
        }
    }
}

