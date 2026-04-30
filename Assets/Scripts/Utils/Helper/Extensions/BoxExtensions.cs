using System.Collections.Generic;
using UnityEngine;

public static class BoxExtensions
{
    public static List<BoxVector2IntDimensions> GetVector2IntBoxDimensions(this List<Vector2Int> oneByOneBoxes)
    {
        if (oneByOneBoxes == null || oneByOneBoxes.Count <= 0)
            return new List<BoxVector2IntDimensions>();

        List<Vector2Int> unusedPoints = new List<Vector2Int>(oneByOneBoxes);

        List<BoxVector2IntDimensions> returnBoxList = new List<BoxVector2IntDimensions>();

        while (unusedPoints.Count > 0)
        {
            bool growLeft = true;
            bool growRight = true;
            bool growTop = true;
            bool growBottom = true;

            int bottom = unusedPoints[0].y;
            int top = unusedPoints[0].y;
            int right = unusedPoints[0].x;
            int left = unusedPoints[0].x;

            List<Vector2Int> currentBox = new List<Vector2Int>() { unusedPoints[0] };

            unusedPoints.RemoveAt(0);

            while (growBottom || growLeft || growRight || growTop)
            {
                if (growBottom)
                {
                    for (int i = left; i <= right; i++)
                    {
                        if (!unusedPoints.Contains(new Vector2Int(i, bottom - 1)))
                        {
                            growBottom = false;
                            break;
                        }
                    }

                    if (growBottom)
                    {
                        for (int i = left; i <= right; i++)
                        {
                            currentBox.Add(new Vector2Int(i, bottom - 1));
                            unusedPoints.Remove(new Vector2Int(i, bottom - 1));
                        }

                        bottom -= 1;
                    }
                }

                if (growLeft)
                {
                    for (int i = bottom; i <= top; i++)
                    {
                        if (!unusedPoints.Contains(new Vector2Int(left - 1, i)))
                        {
                            growLeft = false;
                            break;
                        }
                    }

                    if (growLeft)
                    {
                        for (int i = bottom; i <= top; i++)
                        {
                            currentBox.Add(new Vector2Int(left - 1, i));
                            unusedPoints.Remove(new Vector2Int(left - 1, i));
                        }
                        left -= 1;
                    }
                }

                if (growRight)
                {
                    for (int i = bottom; i <= top; i++)
                    {
                        if (!unusedPoints.Contains(new Vector2Int(right + 1, i)))
                        {
                            growRight = false;
                            break;
                        }
                    }

                    if (growRight)
                    {
                        for (int i = bottom; i <= top; i++)
                        {
                            currentBox.Add(new Vector2Int(right + 1, i));
                            unusedPoints.Remove(new Vector2Int(right + 1, i));
                        }
                        right += 1;
                    }
                }

                if (growTop)
                {
                    for (int i = left; i <= right; i++)
                    {
                        if (!unusedPoints.Contains(new Vector2Int(i, top + 1)))
                        {
                            growTop = false;
                            break;
                        }
                    }

                    if (growTop)
                    {
                        for (int i = left; i <= right; i++)
                        {
                            currentBox.Add(new Vector2Int(i, top + 1));
                            unusedPoints.Remove(new Vector2Int(i, top + 1));
                        }
                        top += 1;
                    }
                }
            }

            returnBoxList.Add(new BoxVector2IntDimensions(top, bottom, left, right));
        }

        return returnBoxList;
    }
}
