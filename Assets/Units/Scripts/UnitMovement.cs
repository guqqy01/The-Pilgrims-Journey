using System.Collections.Generic;
using UnityEngine;
using Equipment;
using Tiles;
using System.IO;

namespace Units
{
    public class UnitMovement : BaseMovement
    {
        public Animator ani;

        public List<Vector2Int> Path { private set; get; }

        public int CurrentPathIndex { private set; get; } = 0;


        private bool finishedPath = false;

        public delegate void OnPathCompleted();
        public OnPathCompleted onPathCompleted;

        public void Init(List<Vector2Int> path, int currentPathIndex, UnitSO unitSO)
        {
            BaseInit(unitSO.MovementSpeed);

            Speed.AddBuff("UpgradeManager", UpgradeManager.Instance.GetBuff(UnitStatsEnum.MovementSpeed));

            CurrentPathIndex = currentPathIndex;

            Path = path;
            finishedPath = false;
        }

        public void Init(List<Vector2Int> path, int currentPathIndex, Tile tile)
        {
            BaseInit(UnitStatBase.CreateBuffableStat(tile, tile.UnitSO.MovementSpeed, UnitStatsEnum.MovementSpeed));

            CurrentPathIndex = currentPathIndex;

            Path = path;
            finishedPath = false;
        }

        private void OnDestroy()
        {
            onPathCompleted = null;
        }

        public override void HandleFixedUpdateMovement()
        {
            if (finishedPath)
            {
                onPathCompleted?.Invoke();

                LocalMapLevelCompletedManager.Instance.UnitPassed();
                UnitSpawningManager.Instance.SpawnedUnits.Remove(GetComponent<Unit>());
                Destroy(gameObject);
                return;
            }

            if (finishedPath || Path == null)
                return;

            Vector2 targetPos = Path[CurrentPathIndex];

            Vector2 toVector = targetPos - (Vector2)transform.position;

            ani.SetBool("Right", Vector2.Angle(toVector, Vector2.right) < 10f);
            ani.SetBool("Left", Vector2.Angle(toVector, Vector2.left) < 10f);
            ani.SetBool("Up", Vector2.Angle(toVector, Vector2.up) < 10f);
            ani.SetBool("Down", Vector2.Angle(toVector, Vector2.down) < 10f);

            visualTransform.localScale = new Vector3(Vector2.Angle(toVector, Vector2.left) < 10f ? -initialXScale : initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);

            float distance = toVector.magnitude;

            float distanceToMove = SpeedValue * Time.fixedDeltaTime;

            if (distance < distanceToMove)
            {
                transform.position = (Vector2)Path[CurrentPathIndex];

                if (CurrentPathIndex == Path.Count - 1)
                {
                    finishedPath = true;
                    return;
                }

                CurrentPathIndex++;

                distanceToMove = distanceToMove - distance;
            }

            transform.position += (Vector3)toVector.normalized * distanceToMove;
        }
    }
}


