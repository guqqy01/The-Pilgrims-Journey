using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GlobalMap;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using DialogueSystem;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using Heros;
using Units;
public static class ExtensionsAndHelper
{
    public static Color SetAlpha(this Color color, float alpha)
    {
        Color holder = new Color(color.r, color.g, color.b, alpha);
        return holder;
    }

    public static void DrawSpriteInRect(this Sprite sprite, Rect rect)
    {
        if (sprite == null) return;
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.rect;
        Rect uvRect = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height
        );
        GUI.DrawTextureWithTexCoords(rect, texture, uvRect);
    }

    public static void AppendLineToTarget(this string line, ref string target)
    {
        if (string.IsNullOrEmpty(line))
            return;

        if (!string.IsNullOrEmpty(target))
            target += "\r\n";

        target += line;
    }

    public static bool TryGetComponentInParent<T>(this Component component, out T outComponent)
    {
        outComponent = component.GetComponentInParent<T>();

        return outComponent != null;
    }

    public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T outComponent)
    {
        outComponent = gameObject.GetComponentInParent<T>();

        return outComponent != null;
    }

    public static List<Vector2> SplitConeDirections(this Vector2 mainDirection, int numOfSplits, float coneAngleRad)
    {
        if(numOfSplits <= 0)
            return new List<Vector2>();

        if(numOfSplits == 1)
            return new List<Vector2> { mainDirection };

        List<Vector2> returnList = new List<Vector2>();

        float angleDeltaRad = coneAngleRad / (numOfSplits - 1);

        Vector2 currentDirection = mainDirection.RotateCounterClockwise(coneAngleRad / 2f);

        for (int i = 0; i < numOfSplits; i++)
        {
            returnList.Add(currentDirection);

            currentDirection = currentDirection.RotateClockwise(angleDeltaRad);
        }

        return returnList;
    }

    public static bool Odd(this int value)
    {
        return !Even(value);
    }

    public static bool Even(this int value)
    {
        return value % 2 == 0;
    }

    public static List<Vector2> SplitConeDirectionsAlwaysHaveMainDirection(this Vector2 mainDirection, int numOfSplits, float coneAngleRad)
    {
        if (numOfSplits <= 0)
            return new List<Vector2>();

        if (numOfSplits.Odd())
            return SplitConeDirections(mainDirection, numOfSplits, coneAngleRad);

        List<Vector2> returnList = SplitConeDirections(mainDirection, numOfSplits + 1, coneAngleRad);

        int ran = Random.Range(0, 2);

        if(ran == 0)
            returnList.RemoveAt(0);
        else
            returnList.RemoveAt(returnList.Count - 1);

        return returnList;
    }

    public static bool IsIntStat(this HeroStats stat)
    {
        return stat switch
        {
            HeroStats.Health => true,
            HeroStats.Shield => true,
            HeroStats.Damage => true,
            HeroStats.NumberOfProjectiles => true,

            _ => false,
        };
    }

    public static bool IsIntStat(this UnitStatsEnum stat)
    {
        return stat switch
        {
            UnitStatsEnum.Health => true,
            UnitStatsEnum.Shield => true,

            _ => false,
        };
    }

    public static List<Vector2Int> AddToAll(this List<Vector2Int> v2IntList, Vector2Int v2Int)
    {
        List<Vector2Int> returnList = new List<Vector2Int>();

        for (int i = 0; i < v2IntList.Count; i++)
        {
            returnList.Add(v2IntList[i] + v2Int);
        }

        return returnList;
    }

    public static List<T> GetRandomAmount<T>(this List<T> list, int amount, bool allowSame = false)
    {
        if(list == null || list.Count == 0)
            return new List<T>();

        List<T> returnlist = new List<T>();

        if (allowSame)
        {
            for (int i = 0; i < amount; i++)
            {
                returnlist.Add(list.GetRandom());
            }
        }
        else
        {
            List<T> copyOfList = new List<T>();

            copyOfList.AddRange(list);

            while(returnlist.Count < Mathf.Min(amount, list.Count) && copyOfList.Count > 0)
            {
                T item = copyOfList.GetRandom();
                returnlist.Add(item);
                copyOfList.Remove(item);
            }
        }

        return returnlist;
    }

    public static T GetRandom<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static bool ContainsLayer(this LayerMask layerMask, int layer)
    {
        return (layerMask & (1 << layer)) != 0;
    }
    
    public static object GetRandom(this System.Array array)
    {
        return array.GetValue(Random.Range(0, array.Length));
    }

    public static int RandomSign(this int value)
    {
        float random = Random.Range(1, 3);
        int randomSign = 1;

        if (random == 2)
            randomSign = -1;

        return value * randomSign;
    }

    public static string AddSpacesBeforeCapsRegex(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // This regex finds a lowercase letter followed by an uppercase letter
        // and inserts a space between them.
        return Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
    }

    public static int RoundToNearedInt(this int value, int nearestInterger)
    {
        int remainder = value % nearestInterger;

        if (remainder >= nearestInterger / 2f)
            return value + (nearestInterger - remainder);
        else
            return value - remainder;
    }

    public static float RandomSign(this float value)
    {
        float random = Random.Range(1, 3);
        int randomSign = 1;

        if (random == 2)
            randomSign = -1;

        return value * randomSign;
    }

    public static int RandomSign()
    {
        List<int> holder = new List<int>() { -1, 1};

        return holder.GetRandom();
    }

    public static bool IsWithin(this float value, float f1, float f2)
    {
        float min = Mathf.Min(f1, f2);
        float max = Mathf.Max(f1, f2);

        return min <= value && value <= max;
    }

    public static Vector2 Middle(this Vector2 v1, Vector2 v2)
    {
        return (v2 + v1) / 2f;
    }

    public static Vector2Int V2ToV2Int(this Vector2 v2)
    {
        return new Vector2Int(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
    }

    public static List<Vector2Int> Rotate(this List<Vector2Int> v2Ints, float rotation)
    {
        List<Vector2Int> returnV2Ints = new List<Vector2Int>();

        for (int i = 0; i < v2Ints.Count; i++)
        {
            int x = Mathf.RoundToInt(v2Ints[i].x * Mathf.Cos(rotation * Mathf.Deg2Rad) - v2Ints[i].y * Mathf.Sin(rotation * Mathf.Deg2Rad));
            int y = Mathf.RoundToInt(v2Ints[i].x * Mathf.Sin(rotation * Mathf.Deg2Rad) + v2Ints[i].y * Mathf.Cos(rotation * Mathf.Deg2Rad));
            returnV2Ints.Add(new Vector2Int(x, y));
        }

        return returnV2Ints;
    }

    public static bool Negative(this float value)
    {
        return (value < 0);
    }

    public static bool Negative(this int value)
    {
        return (value < 0);
    }

    public static bool Positive(this float value)
    {
        return (value > 0);
    }

    public static bool Positive(this int value)
    {
        return (value > 0);
    }

    public static Direction OppositeDirection(this Direction direction)
    {
        return (Direction)(-(int)direction);
    }

    public static List<Direction> TangentDirections(this Direction direction)
    {
        List<Direction> returnList = new List<Direction>();
        List<Direction> allDirections = new List<Direction>() { Direction.South, Direction.West, Direction.East, Direction.North };


        foreach (Direction dir in allDirections)
        {
            if (dir != direction && dir != direction.OppositeDirection())
                returnList.Add(dir);
        }

        return returnList;
    }

    public static void RemoveDuplicates<T>(this List<T> list)
    {
        list.Distinct().ToList();
    }

    public static List<Transform> GetChildrenAtDepth(this Transform parent, int depth)
    {
        if (depth <= 0) return null;

        List<Transform> childrenAtDepth = new List<Transform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.GetDepth(parent) == depth)
            {
                childrenAtDepth.Add(child);
            }
        }
        
        return childrenAtDepth;
    }
    public static ParentList<T> ToParentList<T>(this IEnumerable<T> enumerable, Func<T, T> parentFunction) where T : IParentListItem<T>
    {
        ParentList<T> returnList = new ParentList<T>();

        foreach (T item in enumerable)
        {
            item.Parent = parentFunction(item);
            returnList.Add(item);
        }

        return returnList;
    }

    public static ParentList<T> ToParentList<T>(this IEnumerable<T> enumerable) where T : IParentListItem<T>
    {
        ParentList<T> returnList = new ParentList<T>();

        foreach (T item in enumerable)
        {
            returnList.Add(item);
        }

        return returnList;
    }
    public static List<RectTransform> GetChildrenAtDepth(this RectTransform parent, int depth)
    {
        if (depth <= 0) return null;

        List<RectTransform> childrenAtDepth = new List<RectTransform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            RectTransform child = (RectTransform)parent.GetChild(i);
            if (child.GetDepth(parent) == depth)
            {
                childrenAtDepth.Add(child);
            }
        }

        return childrenAtDepth;
    }

    private static int GetDepth(this Transform t, Transform parent)
    {
        int depth = 1;
        while (t.parent != null && t.parent != parent)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }

    public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> kvp)
    {
        if (dictionary == null)
            return;

        dictionary.Add(kvp.Key, kvp.Value);
    }

    public static float Lerp(float v1, float v2, float t)
    {
        return v1 + (v2 - v1) * t;
    }

    public static float QuadraticBezier(float v0, float v1, float v2, float t)
    {
        float value = v1 + Mathf.Pow((1f - t), 2f) * (v0 - v1) + Mathf.Pow(t, 2f) * (v2 - v1);
        Debug.Log($"v0: {v0}, v1: {v1}, v2: {v2}, t: {t}, value: {value}");

        return value;
    }

    public static float LerpBetweenValues(this Vector2 vector, float t)
    {
        return Lerp(vector.x, vector.y, t);
    }

    public static bool ContainsWholeRect(this Rect rect, Rect other)
    {
        return rect.max.x >= other.max.x &&
            rect.max.y >= other.max.y &&
            rect.min.x <= other.min.x &&
            rect.min.y <= other.min.y;
    }

    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }

    public static float Min(this Vector2 v2)
    {
        return Mathf.Min(v2.x, v2.y);
    }

    public static float Max(this Vector2 v2)
    {
        return Mathf.Max(v2.x, v2.y);
    }

    public static float Min(this Vector3 v3)
    {
        return Mathf.Min(v3.x, v3.y, v3.z);
    }

    public static float Max(this Vector3 v3)
    {
        return Mathf.Max(v3.x, v3.y , v3.z);
    }

    public static float Min(this Vector2Int v2)
    {
        return Mathf.Min(v2.x, v2.y);
    }

    public static float Max(this Vector2Int v2)
    {
        return Mathf.Max(v2.x, v2.y);
    }

    public static float Min(this Vector3Int v3)
    {
        return Mathf.Min(v3.x, v3.y, v3.z);
    }

    public static float Max(this Vector3Int v3)
    {
        return Mathf.Max(v3.x, v3.y, v3.z);
    }

    public static bool EqualContents(this SerializedProperty serializedProperty1, SerializedProperty serializedProperty2)
    {
        return SerializedProperty.EqualContents(serializedProperty1, serializedProperty2);
    }

    public static bool Between(this int interger, int minInclusive, int maxInclusive)
    {
        if (interger < minInclusive || interger > maxInclusive)
            return false;

        return true;
    }

    public static object GetTargetObjectOfProperty(this SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    public static SerializedProperty FindParentProperty(this SerializedProperty serializedProperty)
    {
        var propertyPaths = serializedProperty.propertyPath.Split('.');
        if (propertyPaths.Length <= 1)
        {
            return default;
        }

        var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
        for (var index = 1; index < propertyPaths.Length - 1; index++)
        {
            if (propertyPaths[index] == "Array")
            {
                if (index + 1 == propertyPaths.Length - 1)
                {
                    // reached the end
                    break;
                }
                if (propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                {
                    var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
            }
            else
            {
                parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
            }
        }

        return parentSerializedProperty;
    }


    public static bool CloseTo(this float f, float other, float limit = 0.0001f) 
    {
        if (f == other) return true;
        if(f > other)
        {
            float newF = f - limit;
            if(newF <= other) return true;
        }
        else
        {
            float newF = f + limit;
            if (newF >= other) return true;
        }

        return false;
    }
    public static void DestroyAndClearList<T>(this List<T> list) where T : MonoBehaviour
    {
        if(list == null) return;

        foreach (T item in list)
        {
            if (item == null)
                continue;

            if (Application.isPlaying)
            {
                GameObject.Destroy(item.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
        }

        list.Clear();
    }

    public static void DestroyAndClearList(this List<GameObject> list)
    {
        foreach (GameObject item in list)
        {
            if (item == null)
                continue;

            if (Application.isPlaying)
            {
                GameObject.Destroy(item);
            }
            else
            {
                GameObject.DestroyImmediate(item);
            }
        }

        list.Clear();
    }

    public static List<BoxVector2IntDimensions> GetVector2IntBoxDimensions(this List<Vector2Int> oneByOneBoxes)
    {
        List<Vector2Int> unusedPoints = oneByOneBoxes;

        if(unusedPoints == null || unusedPoints.Count <= 0)
            return new List<BoxVector2IntDimensions>();

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


    public static Vector3 QuadraticBezier(Vector3 v1, Vector3 v2, Vector3 v3, float t)
    {
        return Mathf.Pow(1f - t, 2f) * v1 + 2f * (1f - t) * v2 + Mathf.Pow(t, 2f) * v3;
    }

    public static void ScaleToSize(this RectTransform transform, float size)
    {
        if (size == 0f)
        {
            transform.sizeDelta = Vector2.zero;
            return;
        }

        bool xLarger = transform.sizeDelta.x >= transform.sizeDelta.y;
        float min = Mathf.Min(transform.sizeDelta.x, transform.sizeDelta.y);
        float max = Mathf.Max(transform.sizeDelta.x, transform.sizeDelta.y);
        float value = min / max * size;
        transform.sizeDelta = new Vector2(xLarger ? size : value, xLarger ? value : size);
    }
    public static void ScaleToSize(this Image image, float size)
    {
        image.rectTransform.ScaleToSize(size);
    }

    public static void ScaleHeightToSize(this RectTransform rt, float newHeight)
    {
        if (rt == null) return;
        if (newHeight <= 0f)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
            return;
        }

        // Use current rect size (accounts for anchors/layout) not sizeDelta directly
        float currentWidth = rt.rect.width;
        float currentHeight = rt.rect.height;

        if (currentHeight == 0f)
        {
            // If current height is zero, preserve width and only set height
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
            return;
        }

        float newWidth = currentWidth * (newHeight / currentHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
    }

    public static void ScaleWidthToSize(this RectTransform rt, float newWidth)
    {
        if (rt == null) return;
        if (newWidth <= 0f)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
            return;
        }

        float currentWidth = rt.rect.width;
        float currentHeight = rt.rect.height;

        if (currentWidth == 0f)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            return;
        }

        float newHeight = currentHeight * (newWidth / currentWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
    }

    public static void ScaleWidthToSize(this Image image, float size)
    {
        image.rectTransform.ScaleWidthToSize(size);
    }


    public static void ScaleHeightToSize(this Image image, float size)
    {
        image.rectTransform.ScaleHeightToSize(size);
    }

    /// <summary>
    /// Scales the image to fit a size with the native ratio of the sprite.
    /// </summary>
    public static void NativeRatioScaleToSize(this Image image, float size)
    {
        image.SetNativeSize();
        image.ScaleToSize(size);
    }

    public static void NativeRatioScaleWidthToSize(this Image image, float size)
    {
        image.SetNativeSize();
        image.ScaleWidthToSize(size);
    }

    public static void NativeRatioScaleHeightToSize(this Image image, float size)
    {
        image.SetNativeSize();
        image.ScaleHeightToSize(size);
    }

    public static void AddOrUpdateByOne<T>(this Dictionary<T, int> dictionary, T toAdd)
    {
        if (dictionary.ContainsKey(toAdd))
        {
            dictionary[toAdd]++;
        }
        else
        {
            dictionary.Add(toAdd, 1);
        }
    }


    public static IEnumerator WaitForObjectNotNullThenPerformAction(object objectToWaitFor, Action action, float maxTimeToWait = 60)
    {
        float startTime = Time.time;

        while (objectToWaitFor == null && Time.time < startTime + maxTimeToWait)
        {
            yield return null;
        }

        if (objectToWaitFor != null)
            action?.Invoke();
    }

    public static float RandomBetweenTwoValues(this Vector2 v2)
    {
        return Random.Range(Mathf.Min(v2.x, v2.y), Mathf.Max(v2.x, v2.y));
    }

    public static int RandomBetweenTwoValuesInclusive(this Vector2Int v2)
    {
        return Random.Range(Mathf.Min(v2.x, v2.y), Mathf.Max(v2.x, v2.y) + 1);
    }

    public static Vector2 RotateCounterClockwise(this Vector2 v2, float angleRad)
    {
        float x = v2.x * Mathf.Cos(angleRad) - v2.y * Mathf.Sin(angleRad);
        float y = v2.x * Mathf.Sin(angleRad) + v2.y * Mathf.Cos(angleRad);

        return new Vector2(x, y);
    }

    public static Vector2 RotateClockwise(this Vector2 v2, float angleRad)
    {
        return RotateCounterClockwise(v2, -angleRad);
    }

    public static bool InBetweenValues(this Vector2 v2, float value)
    {
        return value > v2.Min() && value < v2.Max();
    }
}


public struct BoxVector2IntDimensions
{
    public Vector2 position;
    public Vector2 size;

    public BoxVector2IntDimensions(int top, int bottom, int left, int right)
    {
        size = new Vector2(right + 1 - left, top + 1 - bottom);
        position = new Vector2(left + right, top + bottom) / 2f;
    }
}