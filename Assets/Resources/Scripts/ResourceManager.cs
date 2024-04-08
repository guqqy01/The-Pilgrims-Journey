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

    public int Gold { private set; get; } = 100;
    public int Diamonds { private set; get; } = 1;

    [SerializeField]
    private float collectRange = 1f;

    [SerializeField]
    private float attractRange = 5f;

    [SerializeField]
    private float attractForce = 10f;

    [SerializeField]
    private LayerMask resourceLayerMask;

    public delegate void OnGoldChange(int currentGold);
    public OnGoldChange onGoldChange;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
        onGoldChange?.Invoke(Gold);
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
                Gold += coin.Value;
                onGoldChange?.Invoke(Gold);
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

    public void ModifyGold(int amount)
    {
        Gold += amount;

        onGoldChange?.Invoke(Gold);
    }
}
