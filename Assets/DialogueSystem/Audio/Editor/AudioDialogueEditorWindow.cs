using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;
using System;

namespace DialogueSystem
{
    public class AudioDialogueEditorWindow : EditorWindow
    {
        #region Fields

        [SerializeField]
        private Prompt prompt;

        [SerializeField]
        private int paragraphIndex = -1;

        private Paragraph paragraph;

        [SerializeField]
        private AudioDialogue audioDialogue;

        private SectionalAudioClipTimeline sectionalAudioClipTimeLine;

        private const float space = 5f;
        private Rect MainTimeLineRect => new Rect() { yMin = EditorGUIUtility.singleLineHeight + 4f, height = 2f * EditorGUIUtility.singleLineHeight, xMin = 0, width = position.width };

        private Rect SectionalAudioClipTimelineRect => new Rect() { yMin = EditorGUIUtility.singleLineHeight + 4f, height = 10f * EditorGUIUtility.singleLineHeight, xMin = 0, width = position.width };

        public float mainTimeLineLength = 60f;

        private float TimeFromPosition(float position, float width, float zoomFactor, float startTime) => position / width * zoomFactor + startTime;
        private float PositionFromTime(float time, float width, float zoomFactor, float startTime) => (time - startTime) * (width / zoomFactor);

        #endregion

        #region Main


        public void Setup(SerializedProperty audioDialogueProperty)
        {
            SerializedProperty copy = audioDialogueProperty.Copy();

            prompt = copy.serializedObject.targetObject as Prompt;

            paragraph =  (Paragraph)audioDialogueProperty.FindParentProperty().GetTargetObjectOfProperty();

            Setup(prompt, paragraph);
        }


        public void Setup(Prompt prompt, int paragraphIndex)
        {
            if (prompt == null || paragraphIndex < 0)
                return;

            if(prompt.paragraphs.Count <= paragraphIndex)
                return;

            paragraph = prompt.paragraphs[paragraphIndex];

            Setup(prompt, paragraph);
        }


        public void Setup(Prompt prompt, Paragraph paragraph)
        {
            if (this.prompt != null)
                SaveChanges();

            this.prompt = prompt;
            this.paragraph = paragraph;
            this.paragraphIndex = prompt.paragraphs.IndexOf(paragraph);

            audioDialogue = paragraph.audioDialogue;

            sectionalAudioClipTimeLine = new SectionalAudioClipTimeline(this, audioDialogue);

            //

            zoomedAreaOnMainTimeLineDraggableArea = new DraggableArea()
            {
                defaultIgnoreOtherDraggableRects = true,
                backgroundColorInner = Color.white.SetAlpha(0),

            };

            zoomedAreaRect = new DraggableRect(new Rect(MainTimeLineRect), false, true, Color.blue.SetAlpha(0.2f), Color.blue, true);
            zoomedAreaOnMainTimeLineDraggableArea.Add(1, zoomedAreaRect);

            //

            dialogueMixerGroupZoomedDraggableArea = new TimeLineDraggableArea(zoomedViewAmount, false, zoomedStartTime)
            {
                defaultDraggableRectSizes = new Vector2(10f, 10f),
                defaultInnerColor = Color.white,
                defaultIgnoreOtherDraggableRects = true,
                backgroundColorInner = (Color.grey * 0.5f).SetAlpha(1f),
                onRightClickRect = OnZoomedRightClickRect,
                beforeDraw = DrawZoomedMixerGroupLines,
                limitXPositionToRect = false,
                rightClickRectOptions = new Dictionary<string, GenericMenu.MenuFunction>()
                    {
                        { "Remove Mixer Group", RemoveZoomedDialogueItem},
                        { "Edit Mixer Group", EditDialogueItemZoomedTimeLine},
                    },
                rightClickSpaceOptions = new Dictionary<string, GenericMenu.MenuFunction>()
                    {
                        { "Add Mixer Group", HandleAddDialogueItemZoomedTimeLine},
                    },
            };

            dialogueMixerGroupZoomedDraggableArea.beforeDraw += (Rect rect) => TimelineDrawer.DrawTimeLine(rect, zoomedViewAmount, zoomedStartTime);

            //

            Show();
        }

        public static void Init(SerializedProperty audioDialogueProperty)
        {
            AudioDialogueEditorWindow window = (AudioDialogueEditorWindow)EditorWindow.GetWindow(typeof(AudioDialogueEditorWindow));

            window.Setup(audioDialogueProperty);
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            EditorUtility.SetDirty(prompt);
        }

        void OnGUI()
        {
            Handles.BeginGUI();
            GUI.depth = 5;

            if (audioDialogue == null)
            {
                Handles.color = Color.red;
                Handles.Label(new Vector2(position.width / 2f, position.height / 2f), new GUIContent("AudioDialogue is null, please open the window through a paragraph on an audio dialogue prompt"), new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                return;
            }

            if (sectionalAudioClipTimeLine == null)
            {
                if (prompt != null && paragraphIndex > -1)
                    Setup(prompt, paragraphIndex);
                else
                {
                    Debug.Log(prompt);
                    Debug.Log(paragraphIndex);
                }
                return;
            }

            Event e = Event.current;

            IfNullCreateAudioDialoguePlayer();

            HandleToolbar();
            HandleInput(e);

            HandlePlacePlayerAtPoint(e);


            sectionalAudioClipTimeLine.Draw(SectionalAudioClipTimelineRect);

            /*HandleAudioClipDraggableArea(e);
            HandleDialogueItemsDraggableArea(e);*/

            if (editingSectionalAudioClip)
            {
                
            }
            else
            {
                HandleDialogueItemsZoomedDraggableArea(e);
                HandleZoomedAreaOnMainTimeLineDraggableArea(e);
            }

            DrawZoomedAudio();

            DrawPlayLineOnZoomedTimeLine();
            DrawPlayLineOnMainTimeLine();
            ZoomAreaScrollBar(e);

            if (sectionalAudioClipTimeLine.Repaint)
            {
                Repaint();
            }

            OperationOrderHelper.Execute();

            Handles.EndGUI();
        }


        private void IfNullCreateAudioDialoguePlayer()
        {
            if (audioDialoguePlayer == null)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.hideFlags = HideFlags.HideAndDontSave;
                audioDialoguePlayer = obj.AddComponent<AudioDialoguePlayer>();
            }
        }
        #endregion

        #region Input

        private readonly KeyCode playPauseShortKey = KeyCode.Space;

        private void HandleInput(Event e)
        {
            HandleSpacePlayPause(e);
        }

        private void HandleSpacePlayPause(Event e)
        { 
            if (!e.isKey || e.keyCode != playPauseShortKey || e.type != EventType.KeyDown)
                return;


            if (audioDialoguePlayer.IsPlaying) 
                audioDialoguePlayer.Pause();
            else
                audioDialoguePlayer.Play(audioDialogue);
        }

        #endregion

        #region Player

        private bool draggingPlayerPoint;


        private AudioDialoguePlayer audioDialoguePlayer;

        private void HandlePlacePlayerAtPoint(Event e)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && AudioViewerRect.Contains(e.mousePosition))
            {
                draggingPlayerPoint = true;
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                draggingPlayerPoint = false;
            }

            if (!draggingPlayerPoint)
                return;

            if ((e.type != EventType.MouseDown && e.type != EventType.MouseDrag) || e.button != 0)
                return;

            if (!AudioViewerRect.Contains(e.mousePosition))
                return;

            if (audioDialoguePlayer == null)
                return;

            //X position in rect

            audioDialoguePlayer.CurrentTime = zoomedViewAmount * ((e.mousePosition.x - AudioViewerRect.xMin) / AudioViewerRect.width) + zoomedStartTime;

            Repaint();
        }

        private void HandleToolbar()
        {
            float buttonWidth = 80f;
            float promptObjectWidth = 240f;
            float dropdownButtonWidth = 160f;

            float xMinPos = position.width - buttonWidth * 3f; 

            Rect promptRect = new Rect() { xMin = 2, width = promptObjectWidth, yMin = 2, height = EditorGUIUtility.singleLineHeight };

            Rect dropdownButtonRect = new Rect() { xMin = promptRect.xMax + 2 , width = dropdownButtonWidth , yMin = 2, height = EditorGUIUtility.singleLineHeight };


            Rect playButtonRect = new Rect() { xMin = xMinPos, width = buttonWidth, yMin = 0, yMax = MainTimeLineRect.yMin };
            xMinPos += buttonWidth;

            Rect stopButtonRect = new Rect() { xMin = xMinPos, width = buttonWidth, yMin = 0, yMax = MainTimeLineRect.yMin };
            xMinPos += buttonWidth;

            Rect loopButtonRect = new Rect() { xMin = xMinPos, width = buttonWidth, yMin = 0, yMax = MainTimeLineRect.yMin };

            EditorGUI.BeginChangeCheck();

            prompt = EditorGUI.ObjectField(promptRect, prompt, typeof(Prompt), false) as Prompt;

            if (EditorGUI.EndChangeCheck())
            {
                Setup(prompt, 0);
            }

            string para = paragraph.paragraph.Replace("\n", "");
            para = para.Replace("\r", "");
            string name = audioDialogue != null ?  (paragraph.paragraph == "" ? "Empty Paragraph" : para) : "Audio Dialogue";

            if (EditorGUI.DropdownButton(dropdownButtonRect, new GUIContent(name), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu() { allowDuplicateNames = true };

                foreach(Paragraph paragraph in prompt.paragraphs)
                {
                    if (paragraph.paragraph == "")
                    {
                        menu.AddItem(new GUIContent("Empty Paragraph"), this.paragraph == paragraph, () => Setup(prompt, paragraph));
                    }
                    else
                    {
                        menu.AddItem(new GUIContent(paragraph.paragraph), this.paragraph == paragraph, () => Setup(prompt, paragraph));
                    }
                }

                menu.DropDown(dropdownButtonRect);
            }

            EditorGUI.BeginChangeCheck();

            string playButtonText = (audioDialoguePlayer.PlayerState == PlayerState.Playing) ? "Playing" :  (audioDialoguePlayer.PlayerState == PlayerState.Paused) ? "Paused" : "Play";

            bool play = GUI.Toggle(playButtonRect, audioDialoguePlayer.PlayerState == PlayerState.Playing, playButtonText, EditorStyles.toolbarButton);

            if (EditorGUI.EndChangeCheck())
            {
                switch (audioDialoguePlayer.PlayerState)
                {
                    case PlayerState.Playing:
                        audioDialoguePlayer.Pause();
                        break;
                    case PlayerState.Paused:
                        audioDialoguePlayer.Play(audioDialogue);
                        break;
                    case PlayerState.Stopped:
                        audioDialoguePlayer.Play(audioDialogue);
                        break;
                }
            }

            EditorGUI.BeginChangeCheck();

            bool stop = GUI.Toggle(stopButtonRect, audioDialoguePlayer.PlayerState == PlayerState.Stopped, (audioDialoguePlayer.PlayerState == PlayerState.Stopped) ? "Stopped" : "Stop", EditorStyles.toolbarButton);

            if (EditorGUI.EndChangeCheck() && stop)
            {
                audioDialoguePlayer.Stop();
            }

            audioDialoguePlayer.Loop = GUI.Toggle(loopButtonRect, audioDialoguePlayer.Loop, (audioDialoguePlayer.Loop) ? "Looping" : "Loop", EditorStyles.toolbarButton);
        }


        #endregion

        #region AudioDrawer

        

        private Rect AudioViewerRect => new Rect() { yMin = ZoomedTimeLineRect.yMax, yMax = position.height - EditorGUIUtility.singleLineHeight, xMin = 0, width = position.width };

        private bool editingSectionalAudioClip = false;
        private SectionalAudioClipViewer currentlyEditingSectionalAudioClipViewer;

        private SectionalAudioClipEditorViewer sectionalAudioClipEditorViewer;

        private void DrawPlayLineOnZoomedTimeLine()
        {
            if (audioDialoguePlayer != null && (audioDialoguePlayer.PlayerState == PlayerState.Playing || audioDialoguePlayer.PlayerState == PlayerState.Paused))
            {
                
                //t is 0 when CurrentTime == ZoomedStartAmount, and is 1 when CurrentTime == ZoomedEndAmount
                float t = (1 / zoomedViewAmount) * audioDialoguePlayer.CurrentTime - (zoomedStartTime / zoomedViewAmount);

                //X position in rect
                float x = AudioViewerRect.xMin + t * AudioViewerRect.width;
                Handles.color = Color.white;
                float width = 2;

                Handles.DrawAAConvexPolygon(new Vector2(x - width / 2f, ZoomedTimeLineRect.yMin), new Vector2(x - width / 2f, AudioViewerRect.yMax), new Vector2(x + width / 2f, AudioViewerRect.yMax), new Vector2(x + width / 2f, ZoomedTimeLineRect.yMin));

                if (audioDialoguePlayer.PlayerState != PlayerState.Paused)
                {
                    Repaint();
                }
            }
        }

        private void DrawPlayLineOnMainTimeLine()
        {
            if (audioDialoguePlayer != null && (audioDialoguePlayer.PlayerState == PlayerState.Playing || audioDialoguePlayer.PlayerState == PlayerState.Paused))
            {
                //t is 0 when CurrentTime == ZoomedStartAmount, and is 1 when CurrentTime == ZoomedEndAmount
                float t =  audioDialoguePlayer.CurrentTime / mainTimeLineLength;

                //X position in rect
                float x = SectionalAudioClipTimelineRect.xMin + t * SectionalAudioClipTimelineRect.width;
                Handles.color = Color.white;
                float width = 2;

                Handles.DrawAAConvexPolygon(new Vector2(x - width / 2f, SectionalAudioClipTimelineRect.yMin), new Vector2(x - width / 2f, SectionalAudioClipTimelineRect.yMax), new Vector2(x + width / 2f, SectionalAudioClipTimelineRect.yMax), new Vector2(x + width / 2f, SectionalAudioClipTimelineRect.yMin));

                if (audioDialoguePlayer.PlayerState != PlayerState.Paused)
                {
                    Repaint();
                }
            }
        }

        private void DrawZoomedAudio()
        {
            Handles.color = (Color.yellow * 0.2f).SetAlpha(1f);

            Handles.DrawAAConvexPolygon(new Vector2(AudioViewerRect.xMin, AudioViewerRect.center.y + 0.01f * AudioViewerRect.height),
                                        new Vector2(AudioViewerRect.xMin, AudioViewerRect.center.y - 0.01f * AudioViewerRect.height),
                                        new Vector2(AudioViewerRect.xMax, AudioViewerRect.center.y - 0.01f * AudioViewerRect.height),
                                        new Vector2(AudioViewerRect.xMax, AudioViewerRect.center.y + 0.01f * AudioViewerRect.height));

            if (editingSectionalAudioClip)
            {
                DrawSplitAudioClipEditor();
            }
            else
            {
                foreach (SectionalAudioClipDraggableRect audioClipRect in sectionalAudioClipTimeLine.sectionalAudioClipsDraggableArea.GetDraggableRects())
                {
                    float minTime = audioClipRect.rect.xMin * (mainTimeLineLength / position.width);
                    float maxTime = audioClipRect.rect.xMax * (mainTimeLineLength / position.width);
                    float zoomedEndTime = zoomedStartTime + zoomedViewAmount;

                    if (maxTime < zoomedStartTime || minTime > zoomedEndTime)
                        continue;

                    float xMin = position.width / zoomedViewAmount * (minTime - zoomedStartTime);
                    float xMax = position.width / zoomedViewAmount * (maxTime - zoomedStartTime);

                    Rect rect = new Rect(AudioViewerRect) { xMin = Mathf.Max(xMin, 0), xMax = Mathf.Min(xMax, position.width) };

                    float width = xMax - xMin;

                    float sliderMin = Mathf.Clamp01(-xMin / width);
                    float sliderMax = Mathf.Clamp01(1 - ((xMax - position.width) / width));

                    float minValue = audioClipRect.sectionalAudioClipViewer.MinValue;
                    float maxValue = audioClipRect.sectionalAudioClipViewer.MaxValue;

                    SectionalAudioClipDrawer.DrawAudioClip(rect, audioClipRect.sectionalAudioClipViewer.AudioClipWaveData.LodAndWaveDatas[(int)WaveDataLOD.High], new Vector2(sliderMin, sliderMax), minValue, maxValue);
                }
            }
        }


        
        public void EditSectionalAudioClip(SectionalAudioClipViewer audioClipViewer)
        {
            editingSectionalAudioClip = true;

            sectionalAudioClipEditorViewer = new SectionalAudioClipEditorViewer(audioClipViewer.sectionalAudioClip, audioClipViewer.AudioClipWaveData, () => editingSectionalAudioClip = false);
        }

        private void OnCloseSectionalAudioClipEditor()
        {
            editingSectionalAudioClip = false;
        }

        private void DrawSplitAudioClipEditor()
        {
            if(sectionalAudioClipEditorViewer == null)
            {
                editingSectionalAudioClip = false;
                return;
            }

            Rect viewerRect = new Rect(AudioViewerRect) { yMin = AudioViewerRect.yMin - EditorGUIUtility.singleLineHeight * 2f};

            sectionalAudioClipEditorViewer?.Draw(Event.current, viewerRect);

            //DrawSplitAudioClip();

        }

        private void DrawSplitAudioClip()
        {
            SectionalAudioClipViewer viewer = currentlyEditingSectionalAudioClipViewer;

            SectionalAudioClipDrawer.DrawAudioClip(AudioViewerRect, viewer.GetWaveData((int)WaveDataLOD.High), DrawMethod.Fade, viewer.MinValue, viewer.MaxValue);


            if (GUI.Button(new Rect(AudioViewerRect) { yMin = AudioViewerRect.yMax - 20}, "Exit"))
            {
                editingSectionalAudioClip = false;
            }
        }

        #endregion

        #region Zoom View

        private Rect ZoomedTimeLineRect => new Rect() { yMin = SectionalAudioClipTimelineRect.yMax, height = 2f * EditorGUIUtility.singleLineHeight, xMin = 0, width = position.width };
        private Rect ZoomedAreaOnMainTimeLineRect => new Rect(MainTimeLineRect) { xMin = zoomedStartTime / mainTimeLineLength * MainTimeLineRect.width, xMax = (zoomedStartTime + zoomedViewAmount) / mainTimeLineLength * MainTimeLineRect.width };
        /// <summary>
        /// Start time of zoom view
        /// </summary>
        private float zoomedStartTime = 20f;
        /// <summary>
        /// Amount of seconds to zoom in on
        /// </summary>
        private float zoomedViewAmount = 20f;
        /// <summary>
        /// Minimum zoom amount
        /// </summary>
        private const float zoomMinLimit = 5f;

        private TimeLineDraggableArea dialogueMixerGroupZoomedDraggableArea;
        private TimeLineDraggableRect mixerGroupCurrentlyEditingRect;

        private DraggableArea zoomedAreaOnMainTimeLineDraggableArea;
        private DraggableRect zoomedAreaRect;

        private void HandleDialogueItemsZoomedDraggableArea(Event e)
        {
            if (dialogueMixerGroupZoomedDraggableArea == null)
                return;

            dialogueMixerGroupZoomedDraggableArea.timeScale = zoomedViewAmount;
            dialogueMixerGroupZoomedDraggableArea.timeStart = zoomedStartTime;

            dialogueMixerGroupZoomedDraggableArea.areaRect = ZoomedTimeLineRect;

            dialogueMixerGroupZoomedDraggableArea.HandleDraggableRects(e, audioDialogue.audioDialogueMixerGroups);
        }

        private void HandleZoomedAreaOnMainTimeLineDraggableArea(Event e)
        {
            if (zoomedAreaOnMainTimeLineDraggableArea == null)
                return;

            zoomedAreaOnMainTimeLineDraggableArea.areaRect = MainTimeLineRect;
            GetZoomedArea(e);
            zoomedAreaOnMainTimeLineDraggableArea.HandleDraggableRects(e);
            SetZoomedArea(e);
        }

        private void OnZoomedRightClickRect(DraggableRect rect)
        {
            if (dialogueMixerGroupZoomedDraggableArea.rectRightClicked is TimeLineDraggableRect timeLineDraggableRect)
            {
                mixerGroupCurrentlyEditingRect = timeLineDraggableRect;
            }
        }

        private void GetZoomedArea(Event e)
        {
            float xMin = zoomedStartTime / mainTimeLineLength * MainTimeLineRect.width;
            float xMax = (zoomedStartTime + zoomedViewAmount) / mainTimeLineLength * MainTimeLineRect.width;

            zoomedAreaRect.rect.xMin = xMin;
            zoomedAreaRect.rect.xMax = xMax;
        }

        private void SetZoomedArea(Event e)
        {
            zoomedStartTime = zoomedAreaRect.rect.xMin * mainTimeLineLength / MainTimeLineRect.width;
            zoomedViewAmount = zoomedAreaRect.rect.width * mainTimeLineLength / MainTimeLineRect.width;
        }

        private void ZoomAreaScrollBar(Event e)
        {
            Rect rect = new Rect() { yMin = AudioViewerRect.yMax, height = EditorGUIUtility.singleLineHeight, width = position.width };
            zoomedStartTime = GUI.HorizontalScrollbar(rect, zoomedStartTime, zoomedViewAmount, 0, mainTimeLineLength);

            if (e.type == EventType.ScrollWheel && AudioViewerRect.Contains(e.mousePosition))
            {
                if (e.shift)
                {
                    zoomedViewAmount = Mathf.Clamp(zoomedViewAmount + e.delta.x, zoomMinLimit, mainTimeLineLength);
                }
                else
                {
                    zoomedStartTime = Mathf.Clamp(zoomedStartTime + (e.delta.y * zoomedViewAmount / mainTimeLineLength), 0, mainTimeLineLength - zoomedViewAmount);
                }
                Repaint();
            }
        }

        private void RemoveZoomedDialogueItem()
        {
            dialogueMixerGroupZoomedDraggableArea.RemoveRightClicked(audioDialogue.audioDialogueMixerGroups);
        }

        private void HandleAddDialogueItemZoomedTimeLine()
        {
            AudioDialogueMixerGroup audioDialogueMixerGroup = new AudioDialogueMixerGroup();
            audioDialogueMixerGroup.Time = Mathf.Lerp(zoomedStartTime, zoomedStartTime + zoomedViewAmount, Mathf.InverseLerp(0, MainTimeLineRect.width, dialogueMixerGroupZoomedDraggableArea.positionClicked.x));
            audioDialogue.audioDialogueMixerGroups.Add(audioDialogueMixerGroup);
        }


        private void EditDialogueItemZoomedTimeLine()
        {
            float timeValue = mixerGroupCurrentlyEditingRect.ListOverTimeItem.StartTime;
            float width = 200f;
            float xPos = (position.width / zoomedViewAmount) * (timeValue - zoomedStartTime);
            Rect rectangle = new Rect() { yMin = ZoomedTimeLineRect.yMax, height = EditorGUIUtility.singleLineHeight, xMin = Mathf.Clamp(xPos - width / 2f - space, space, ZoomedTimeLineRect.xMax - width - 3f * space), width = width + 2f * space };

            PopupWindow.Show(rectangle, new DialogueMixerGroupEditorWindowDropDown((AudioDialogueMixerGroup)mixerGroupCurrentlyEditingRect.ListOverTimeItem, this));
        }

        private void DrawZoomedMixerGroupLines(Rect rect)
        {
            Handles.color = Color.blue;
            float width = 2f;
            float yMin = ZoomedTimeLineRect.yMin;
            float yMax = AudioViewerRect.yMax;
            foreach (DraggableRect draggableRect in dialogueMixerGroupZoomedDraggableArea.GetDraggableRects(2))
            {
                float xPos = draggableRect.rect.center.x;

                Rect rectangle = new Rect() { xMin = xPos - width / 2f, xMax = xPos + width / 2f, yMin = yMin, yMax = yMax };

                EditorGUI.DrawRect(rectangle, Color.cyan);
            }
        }

        
        #endregion
    }
}