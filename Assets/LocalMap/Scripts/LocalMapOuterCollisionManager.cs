using UnityEngine;
using GameManagerCalls;
using LocalMap;

public class LocalMapOuterCollisionManager : Singleton<LocalMapOuterCollisionManager>, IOnAfterCreateLocalMap
{
    [SerializeField]
    private LocalMapWall leftCollider, rightCollider, topCollider, bottomCollider;

    [SerializeField]
    private LayerMask heroLayerMask;

    private void Awake()
    {
        LocalMapLevelCompletedManager.Instance.onRequiredUnitsPassed += LetHeroPassInWantedDirection;
    }

    protected override void CleanUp()
    {
        if (LocalMapLevelCompletedManager.Instance != null)
            LocalMapLevelCompletedManager.Instance.onRequiredUnitsPassed -= LetHeroPassInWantedDirection;
    }

    public void AfterCreateLocalMap()
    {
        SetupColliders();
    }

    private void SetupColliders()
    {
        if(leftCollider == null || rightCollider == null || topCollider == null || bottomCollider == null)
        {
            Debug.Log("Collider(s) not set");
            return;
        }

        float sizeX = LocalMapGenerator.Instance.MapSizeX;
        float sizeY = LocalMapGenerator.Instance.MapSizeY;

        float verticalCollidersYPos = (sizeY / 2f) - 0.5f;
        float horizontalCollidersXPos = (sizeX / 2f) - 0.5f;

        leftCollider.Setup(Direction.West, new Vector2(1f, sizeY), new Vector2(-1f, verticalCollidersYPos));
        rightCollider.Setup(Direction.East, new Vector2(1f, sizeY), new Vector2(sizeX, verticalCollidersYPos));
        bottomCollider.Setup(Direction.South, new Vector2(sizeX, 1f), new Vector2(horizontalCollidersXPos, -1f));
        topCollider.Setup(Direction.North, new Vector2(sizeX, 1f), new Vector2(horizontalCollidersXPos, sizeY));
    }



    private void LetHeroPassInWantedDirection()
    {
        if (!PathPicker.Instance.PickedDirection.HasValue)
            return;

        Direction pickedDirection = (Direction)PathPicker.Instance.PickedDirection;

        switch (pickedDirection)
        {
            case Direction.South:
                bottomCollider.LetHeroPassInDirection();
                break;
            case Direction.North:
                topCollider.LetHeroPassInDirection();
                break;
            case Direction.West:
                //Cant pick west direction
                break;
            case Direction.East:
                rightCollider.LetHeroPassInDirection();
                break;
        }
    }
}
