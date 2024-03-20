using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class Movement : MonoBehaviour
    {
        public BuffableFloatStat speed;
        public float Speed => speed.Value;

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

        public void Init(List<Vector2Int> path, UnitSO unitSO, TileStats tileStats, List<EquipmentSO> equipmentSOs)
        {
            speed = new BuffableFloatStat(unitSO.MovementSpeed);
            speed.AddMultiplierBuff("Tile", tileStats.movementMultiplier);

            for (int i = 0; i < equipmentSOs.Count; i++)
            {
                if (equipmentSOs[i] == null)
                    continue;

                speed.AddBaseBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].FloatBaseValue(UnitFloatStats.Movement));
                speed.AddMultiplierBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].FloatMultiplierValue(UnitFloatStats.Movement));
            }

            Path = path;
            finishedPath = false;
        }

        private void Update()
        {
            if (finishedPath)
            {
                GameManager.Instance.UnitPassed();
                Destroy(gameObject);
                return;
            }

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

            float distanceToMove = Speed * Time.deltaTime;

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


