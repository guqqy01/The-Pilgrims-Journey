using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private InputManager inputManager;

    public GameObject coin1;
    public GameObject coin5;
    public GameObject coin10;

    [SerializeField]
    private int gold;

    [SerializeField]
    private float collectRange = 1f;

    [SerializeField]
    private float attractRange = 5f;

    [SerializeField]
    private float attractForce = 10f;

    [SerializeField]
    private LayerMask resourceLayerMask;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        Collider2D[] collectColliders = Physics2D.OverlapCircleAll(inputManager.MousePosition, collectRange, resourceLayerMask);

        foreach(Collider2D collider in collectColliders)
        {
            if (collider.TryGetComponent(out Coin coin))
            {
                gold += coin.Value;
                Destroy(coin.gameObject);
            }
        }

        Collider2D[] attractColliders = Physics2D.OverlapCircleAll(inputManager.MousePosition, attractRange, resourceLayerMask);

        foreach (Collider2D collider in attractColliders)
        {
            if (collider.TryGetComponent(out Rigidbody2D rb))
            {
                rb.AddForce(attractForce * (inputManager.MousePosition - rb.position), ForceMode2D.Force);
            }
        }
    }
}
