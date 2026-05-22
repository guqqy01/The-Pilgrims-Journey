using UnityEngine;

[CreateAssetMenu(fileName = "TileUIStyle", menuName = "Tiles/Tile UI Style", order = 0)]
public class TileUIStyle : ScriptableObject
{
    [SerializeField]
    private int spritePixelSize;
    private int previousSpritePixelSize;
    public int SpritePixelSize => spritePixelSize;

    [Header("Sprites — all must be cellPixelSize × cellPixelSize")]
    [Tooltip("Solid interior fill, no border.")]
    [SerializeField] private Sprite fill;
    public Sprite Fill => fill;
    private Sprite previousFill;

    [Tooltip("Border on top and bottom. Top half of sprite = top edge, bottom half = bottom edge.")]
    [SerializeField] private Sprite edgeHorizontal;
    public Sprite EdgeHorizontal => edgeHorizontal;
    private Sprite previousEdgeHorizontal;

    [Tooltip("Border on left and right. Left half of sprite = left edge, right half = right edge.")]
    [SerializeField] private Sprite edgeVertical;
    public Sprite EdgeVertical => edgeVertical;
    private Sprite previousEdgeVertical;

    [Tooltip("All four outer corner pieces. Each quadrant of the sprite is one corner.")]
    [SerializeField] private Sprite outerCorners;
    public Sprite OuterCorners => outerCorners;
    private Sprite previousOuterCorners;

    [Tooltip("All four inner corner caps. Each quadrant of the sprite is one corner.")]
    [SerializeField] private Sprite innerCorners;
    public Sprite InnerCorners => innerCorners;
    private Sprite previousInnerCorners;

    private void OnValidate()
    {
        if(previousSpritePixelSize != spritePixelSize)
        {
            previousSpritePixelSize = spritePixelSize;
            MakeSureSpriteHasCorrectSize(ref fill);
            MakeSureSpriteHasCorrectSize(ref edgeHorizontal);
            MakeSureSpriteHasCorrectSize(ref edgeVertical);
            MakeSureSpriteHasCorrectSize(ref outerCorners);
            MakeSureSpriteHasCorrectSize(ref innerCorners);
        }

        if (previousFill != fill && MakeSureSpriteHasCorrectSize(ref fill))
        {
            previousFill = fill;
        }
        if (previousEdgeHorizontal != edgeHorizontal && MakeSureSpriteHasCorrectSize(ref edgeHorizontal))
        {
            previousEdgeHorizontal = edgeHorizontal;
        }
        if (previousEdgeVertical != edgeVertical && MakeSureSpriteHasCorrectSize(ref edgeVertical))
        {
            previousEdgeVertical = edgeVertical;
        }
        if (previousOuterCorners != outerCorners && MakeSureSpriteHasCorrectSize(ref outerCorners))
        {
            previousOuterCorners = outerCorners;
        }
        if (previousInnerCorners != innerCorners && MakeSureSpriteHasCorrectSize(ref innerCorners))
        {
            previousInnerCorners = innerCorners;
        }
    }

    private bool MakeSureSpriteHasCorrectSize(ref Sprite sprite)
    {
        if (sprite == null)
            return true;

        if (sprite.rect.size.x != spritePixelSize || sprite.rect.size.y != spritePixelSize)
        {
            Debug.LogError($"Sprite {sprite.name} has size {sprite.rect.size.x}×{sprite.rect.size.y} pixels, but must be {spritePixelSize}×{spritePixelSize} pixels.");
            sprite = null;
            return false;
        }

        return true;
    }
}
