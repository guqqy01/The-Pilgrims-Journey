using GameManagerCalls;
using Heros;
using UnityEngine;
using System.Collections.Generic;
public class LocalMapWall : MonoBehaviour, IOnAfterCreateLocalMap
{
    private Direction direction;

    [SerializeField]
    private LayerMask heroLayerMask;

    private BoxCollider2D boxCollider;

    [SerializeField]
    private GameObject fogWall;
    private Renderer _fogWallRenderer;
    [SerializeField]
    private float fogWallEncroachDistance = 3f;
    private Renderer _renderer;
    [Header("Shader Values")]
    [SerializeField]
    private bool vertical;
    [SerializeField]
    private bool positive;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        _renderer = GetComponent<Renderer>();
        _fogWallRenderer = fogWall.GetComponent<Renderer>();
    }

    public void Setup(Direction direction, Vector2 size, Vector2 position)
    {
        this.direction = direction;
        boxCollider.size = Vector2.one;
        transform.localScale = size;
        transform.position = position;
        _renderer.material.SetKeyword("_VERTICAL", vertical);
        _renderer.material.SetKeyword("_POSITIVE", positive);

        _fogWallRenderer.material.SetKeyword("_VERTICAL", vertical);
        _fogWallRenderer.material.SetKeyword("_POSITIVE", !positive);

        float maxSize = size.Max();
        fogWall.transform.localScale = 2f * new Vector2(maxSize, maxSize);

        if (vertical)
        {
            if (positive)
            {
                fogWall.transform.position = new Vector2(position.x + maxSize - fogWallEncroachDistance, position.y);
            }
            else
            {
                fogWall.transform.position = new Vector2(position.x - maxSize + fogWallEncroachDistance, position.y);
            }
        }
        else
        {
            if (positive)
            {
                fogWall.transform.position = new Vector2(position.x, position.y + maxSize - fogWallEncroachDistance);
            }
            else
            {
                fogWall.transform.position = new Vector2(position.x, position.y - maxSize + fogWallEncroachDistance);
            }
        }
    }

    private void Update()
    {
        if(HeroManager.Instance != null && HeroManager.Instance.ActiveHero != null)
        {
            Vector2 heroPos = HeroManager.Instance.ActiveHero.transform.position;
            _renderer.material.SetVector("_Hero_Position", heroPos);
        }
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
