using LocalMap;
using UnityEngine;

[RequireComponent(typeof(Bounce), typeof(Sinkable))]
public class BounceSinkableGlue : MonoBehaviour
{
    private Bounce _bounce;
    private Sinkable _sinkable;

    [SerializeField]
    private GameObject waterRipplePrefab;

    private void Awake()
    {
        _bounce = GetComponent<Bounce>();
        _sinkable = GetComponent<Sinkable>();
    }
    
    private void Start()
    {
        _bounce.OnBounce += _sinkable.InstantSinkCheck;
        _sinkable.OnSunk += OnSink;
    }

    private void OnDestroy()
    {
        _bounce.OnBounce -= _sinkable.InstantSinkCheck;
        _sinkable.OnSunk -= OnSink;
    }

    private void OnSink()
    {
        Instantiate(waterRipplePrefab, transform.position, Quaternion.identity, transform.parent);
    }
}
