using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 1f;

    public Animator ani;

    public List<Vector2Int> Path { private set; get; }

    private int currentPathIndex = 0;

    public void Init(List<Vector2Int> path)
    {
        Path = path;
    }

    private void Update()
    {
        Vector2 targetPos = Path[currentPathIndex];

        Vector2 direction = targetPos - (Vector2)transform.position;

        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
    }
}

