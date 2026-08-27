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
    }

    public void Setup(Direction direction, Vector2 size, Vector2 position)
    {
        this.direction = direction;
        boxCollider.size = Vector2.one;
        transform.localScale = size;
        transform.position = position;
        _renderer.material.SetKeyword("_VERTICAL", vertical);
        _renderer.material.SetKeyword("_POSITIVE", positive);
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
