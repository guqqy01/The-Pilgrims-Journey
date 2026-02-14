using UnityEngine;
using UnityEngine.UI;

public static class UiExtensions
{
    public static void ScaleToSize(this RectTransform transform, float size)
    {
        if (transform == null) return;

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
        if (image == null) return;
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

        float currentWidth = rt.rect.width;
        float currentHeight = rt.rect.height;

        if (currentHeight == 0f)
        {
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
        if (image == null) return;
        image.rectTransform.ScaleWidthToSize(size);
    }


    public static void ScaleHeightToSize(this Image image, float size)
    {
        if (image == null) return;
        image.rectTransform.ScaleHeightToSize(size);
    }

    /// <summary>
    /// Scales the image to fit a size with the native ratio of the sprite.
    /// </summary>
    public static void NativeRatioScaleToSize(this Image image, float size)
    {
        if (image == null) return;
        image.SetNativeSize();
        image.ScaleToSize(size);
    }

    public static void NativeRatioScaleWidthToSize(this Image image, float size)
    {
        if (image == null) return;
        image.SetNativeSize();
        image.ScaleWidthToSize(size);
    }

    public static void NativeRatioScaleHeightToSize(this Image image, float size)
    {
        if (image == null) return;
        image.SetNativeSize();
        image.ScaleHeightToSize(size);
    }
}
