using UnityEngine;
using UnityEditor;
using DialogueSystem;
using System.Collections.Generic;
public class SectionalAudioClipDraggableArea : TimeLineDraggableArea
{
    public SectionalAudioClipDraggableArea(float timeScale, bool fillOtherAxis) : base(timeScale, fillOtherAxis, 0f){ }

    protected override void AddTimelineDraggableRect<T>(T item, Rect rect, bool constrainX, bool constrainY)
    {
        if(item is SectionalAudioClip sectionalAudioClip)
        {
            SectionalAudioClipDraggableRect draggableRect = new SectionalAudioClipDraggableRect(sectionalAudioClip, rect, defaultInnerColor, defaultOuterColor);
            Add(0, draggableRect);
        }
        else
        {
            base.AddTimelineDraggableRect(item, rect, constrainX, constrainY);
        }
    }
/*
    protected override void AddTimelineDraggableRect<T>(ref SectionalAudioClip sectionalAudioClip, Rect rect, bool constrainX, bool constrainY, object obj)
    {
        SectionalAudioClipDraggableRect draggableRect = new SectionalAudioClipDraggableRect(sectionalAudioClip, rect, defaultInnerColor, defaultOuterColor);
        Add(0, draggableRect);
    }
*/
}