using GameManagerCalls;
using Heros;
using UnityEngine;

public class LocalMapWall : MonoBehaviour, IOnAfterCreateLocalMap
{
    private Direction direction;

    [SerializeField]
    private LayerMask heroLayerMask;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Setup(Direction direction, Vector2 size, Vector2 position)
    {
        this.direction = direction;
        boxCollider.size = size;
        transform.position = position;
    }

    public void LetHeroPassInDirection()
    {
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!heroLayerMask.ContainsLayer(collision.gameObject.layer))
            return;

        if(collision.gameObject.TryGetComponent(out Hero hero))
        {
            //Move Hero
            hero.HeroMovement.MoveInDirectionOutOfMap(direction);
            hero.HealthAndShieldManager.MakeInvunrableForTime(60f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!heroLayerMask.ContainsLayer(collision.gameObject.layer))
            return;

        LocalMapLevelCompletedManager.Instance.CompleteLevel();
    }

    public void AfterCreateLocalMap()
    {
        boxCollider.isTrigger = false;
    }
}
