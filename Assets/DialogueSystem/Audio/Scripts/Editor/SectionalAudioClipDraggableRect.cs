using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using DialogueSystem;
using UnityEngine.Audio;

public class SectionalAudioClipDraggableRect : TimeLineDraggableRect
{
    public readonly SectionalAudioClipViewer sectionalAudioClipViewer;

    public Rect AudioClipRect => new Rect() { xMin = rect.xMin + 5, width = rect.width - 10, yMin = rect.yMin + 5, height = EditorGUIUtility.singleLineHeight };

    public SectionalAudioClipDraggableRect(SectionalAudioClip sectionalAudioClip, Rect rect, Color innerColor, Color outlineColor) : base(sectionalAudioClip, rect, false, true, innerColor, outlineColor, false)
    {
        sectionalAudioClipViewer = new SectionalAudioClipViewer(sectionalAudioClip, 2, DrawMethod.Cutout);
    }

    public override void DrawRect(Event e)
    {
        base.DrawRect(e);

        SectionalAudioClipDrawer.DrawAudioClip(rect, sectionalAudioClipViewer);

        OperationOrderHelper.Add(1, DrawAudioClipObject);
    }

    public override void BeforeDrag()
    {
        _ignoreDragRects = new List<Rect> { AudioClipRect };
    }

    private void DrawAudioClipObject()
    {
        if(ListOverTimeItem is SectionalAudioClip sectionalAudioClip)
        {
            sectionalAudioClipViewer.AudioClip = (AudioClip)EditorGUI.ObjectField(AudioClipRect, sectionalAudioClip.audioClip, typeof(AudioClip), false);
        }
    }

}