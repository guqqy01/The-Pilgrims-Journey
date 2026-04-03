using UnityEditor;
using UnityEngine;

public class ScrollableToolbar
{
    public float sideButtonWidth = 50f;

    public float minWidth = 200f;

    private int currentLeftIndex = 0;

    private int _selectedIndex;
    public int SelectedIndex { get { return _selectedIndex; } set { _selectedIndex = value; } }


    public void HandleScrollableToolbar(Rect rect, string[] texts)
    {
        int maxAmountOfTitles = Mathf.FloorToInt((rect.width + sideButtonWidth * 2f) / minWidth);

        int previousIndex = _selectedIndex;

        if (maxAmountOfTitles < texts.Length)
        {
            string[] stringsToFit = new string[maxAmountOfTitles];

            while(currentLeftIndex + maxAmountOfTitles > texts.Length)
            {
                currentLeftIndex--;
            }

            for (int i = 0; i < maxAmountOfTitles; i++)
            {
                stringsToFit[i] = texts[currentLeftIndex + i];
            }

            Rect leftRectButton = new Rect(rect) { width = sideButtonWidth };
            Rect rightRectButton = new Rect(rect) { xMin = rect.xMax - sideButtonWidth };
            Rect toolbarRect = new Rect(rect) { xMin = leftRectButton.xMax, xMax = rightRectButton.xMin};


            if(GUI.Button(leftRectButton, new GUIContent("<")))
            {
                currentLeftIndex = Mathf.Clamp(currentLeftIndex - 1, 0, texts.Length - maxAmountOfTitles);
            }

            if(GUI.Button(rightRectButton, new GUIContent(">")))
            {
                currentLeftIndex = Mathf.Clamp(currentLeftIndex + 1, 0, texts.Length - maxAmountOfTitles);
            }

            int shiftedSelected = _selectedIndex - currentLeftIndex;

            _selectedIndex = GUI.Toolbar(toolbarRect, shiftedSelected, stringsToFit) + currentLeftIndex;
        }
        else
        {
            _selectedIndex = GUI.Toolbar(rect, _selectedIndex, texts);
        }

        if(previousIndex != _selectedIndex)
        {
            GUI.FocusControl("");
        }
    }
}
