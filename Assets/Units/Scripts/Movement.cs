using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class Movement : MonoBehaviour
    {
        private float baseSpeed;
        private float speedMultiplier;
        public float speed => baseSpeed * speedMultiplier;

        public Animator ani;

        public List<Vector2Int> Path { private set; get; }

        private int currentPathIndex = 0;

        private bool finishedPath = false;

        [SerializeField]
        private Transform visualTransform;

        private float initialXScale;

        private void Awake()
        {
            initialXScale = visualTransform.localScale.x;
        }

        public void Init(List<Vector2Int> path, UnitSO unitSO, TileStats tileStats)
        {
            baseSpeed = unitSO.MovementSpeed;
            speedMultiplier = tileStats.movementMultiplier;
            Path = path;
            finishedPath = false;
        }

        private void Update()
        {
            if (finishedPath || Path == null)
                return;

            Vector2 targetPos = Path[currentPathIndex];

            Vector2 toVector = targetPos - (Vector2)transform.position;

            ani.SetBool("Right", Vector2.Angle(toVector, Vector2.right) < 10f);
            ani.SetBool("Left", Vector2.Angle(toVector, Vector2.left) < 10f);
            ani.SetBool("Up", Vector2.Angle(toVector, Vector2.up) < 10f);
            ani.SetBool("Down", Vector2.Angle(toVector, Vector2.down) < 10f);

            visualTransform.localScale = new Vector3(Vector2.Angle(toVector, Vector2.left) < 10f ? -initialXScale : initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);

            float distance = toVector.magnitude;

            float distanceToMove = speed * Time.deltaTime;

            if (distance < distanceToMove)
            {
                transform.position = (Vector2)Path[currentPathIndex];

                if (currentPathIndex == Path.Count - 1)
                {
                    finishedPath = true;
                    return;
                }

                currentPathIndex++;

                distanceToMove = distanceToMove - distance;
            }

            transform.position += (Vector3)toVector.normalized * distanceToMove;


        }
    }
}


