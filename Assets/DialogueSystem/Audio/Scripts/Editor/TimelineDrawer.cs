using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
    internal static class TimelineDrawer
    {
        public static float PositionFromTime(float time, float width, float zoomFactor, float startTime) => (time - startTime) * (width / zoomFactor);
        public static float TimeFromPosition(float position, float width, float zoomFactor, float startTime) => position / width * zoomFactor + startTime;

        public static void DrawTimeLine(Rect rect, float zoomAmount, float zoomStart)
        {
            Handles.color = Color.black;

            int MaximumAmountOfBigLines = Mathf.FloorToInt(rect.width / 55f);


            float minimumTimeBetweenBigLines = zoomAmount / MaximumAmountOfBigLines;

            float timeBetweenBigLines = 0.1f;
            if (minimumTimeBetweenBigLines <= 0.5f && minimumTimeBetweenBigLines > 0.1f)
            {
                timeBetweenBigLines = 0.5f;
            }
            else if (minimumTimeBetweenBigLines <= 1f && minimumTimeBetweenBigLines > 0.5f)
            {
                timeBetweenBigLines = 1f;
            }
            else if (minimumTimeBetweenBigLines <= 5f && minimumTimeBetweenBigLines > 1f)
            {
                timeBetweenBigLines = 5f;
            }
            else if (minimumTimeBetweenBigLines <= 30f && minimumTimeBetweenBigLines > 5f)
            {
                timeBetweenBigLines = 30f;
            }
            else if (minimumTimeBetweenBigLines <= 60f && minimumTimeBetweenBigLines > 30f)
            {
                timeBetweenBigLines = 60f;
            }
            else if (minimumTimeBetweenBigLines <= 300f && minimumTimeBetweenBigLines > 60f)
            {
                timeBetweenBigLines = 300f;
            }

            int amountOfBigLines = Mathf.FloorToInt(zoomAmount / timeBetweenBigLines);

            float remainder = zoomStart % timeBetweenBigLines;
            float startLineTime = zoomStart - remainder;

            float currentTime = startLineTime;

            float spaceBetweenBigLines = timeBetweenBigLines / zoomAmount * rect.width;

            int amountOfSmallLinesBetweenBiglines = spaceBetweenBigLines < 80f ? 4 : 9;

            float spaceBetweenSmallLines = spaceBetweenBigLines / (amountOfSmallLinesBetweenBiglines + 1);

            for (int i = 0; i < amountOfBigLines + 2; i++)
            {
                float xPos = PositionFromTime(currentTime, rect.width, zoomAmount, zoomStart);

                Handles.DrawAAPolyLine(new Vector2(xPos, rect.yMin), new Vector2(xPos, rect.yMin + EditorGUIUtility.singleLineHeight));

                if ((xPos > 55f / 2f) && (xPos < rect.width - 55f / 2f))
                {
                    Handles.Label(
                        new Vector2(xPos, rect.yMin + EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight / 2f),
                        new GUIContent(GetTimeString(xPos, rect.width, zoomAmount, zoomStart)),
                        new GUIStyle() { alignment = TextAnchor.MiddleCenter });
                }



                for (int j = 0; j < amountOfSmallLinesBetweenBiglines; j++)
                {
                    xPos += spaceBetweenSmallLines;

                    Handles.DrawAAPolyLine(new Vector2(xPos, rect.yMin), new Vector2(xPos, rect.yMin + EditorGUIUtility.singleLineHeight / 2f));
                }

                currentTime += timeBetweenBigLines;
            }

            Handles.color = Color.white;
        }

        public static string GetTimeString(float xPos, float width, float zoomFactor, float startTime)
        {
            float time = xPos / width * zoomFactor + startTime;

            int minutes = Mathf.FloorToInt(time / 60f);
            time -= minutes * 60f;
            int seconds = Mathf.FloorToInt(time);
            time -= seconds;
            int milliSeconds = Mathf.RoundToInt(time * 100f);

            if (milliSeconds == 100)
            {
                seconds += 1;
                milliSeconds = 0;
            }

            string minutesString = minutes.ToString();
            if (minutesString.Length < 1)
            {
                while (minutesString.Length < 1)
                {
                    minutesString = "0" + minutesString;
                }
            }

            string secondsString = seconds.ToString();
            if (secondsString.Length < 2)
            {
                while (secondsString.Length < 2)
                {
                    secondsString = "0" + secondsString;
                }
            }

            string milliSecondsString = milliSeconds.ToString();
            if (milliSecondsString.Length < 2)
            {
                while (milliSecondsString.Length < 2)
                {
                    milliSecondsString = "0" + milliSecondsString;
                }
            }

            return $"{minutesString}:{secondsString}:{milliSecondsString}";
        }
    }
}
