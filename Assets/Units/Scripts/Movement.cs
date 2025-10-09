using System.Collections.Generic;
using UnityEngine;
using Equipment;
using Tiles;
using static Units.Movement;

namespace Units
{
    public class Movement : MonoBehaviour
    {
        public BuffableStat speed;
        public float Speed => (float)speed.Value.Value;

        public Animator ani;

        public List<Vector2Int> Path { private set; get; }

        public int CurrentPathIndex { private set; get; } = 0;


        private bool finishedPath = false;

        [SerializeField]
        private Transform visualTransform;

        private float initialXScale;

        public delegate void OnPathCompleted();
        public OnPathCompleted onPathCompleted;


        private void Awake()
        {
            initialXScale = visualTransform.localScale.x;
        }

        public void Init(List<Vector2Int> path, int currentPathIndex, UnitSO unitSO)
        {
            speed = new BuffableStat(unitSO.MovementSpeed);

            speed.AddBuff("UpgradeManager", UpgradeManager.Instance.GetBuff(UnitStatsEnum.MovementSpeed));

            CurrentPathIndex = currentPathIndex;

            Path = path;
            finishedPath = false;
        }

        public void Init(List<Vector2Int> path, int currentPathIndex, Tile tile)
        {
            speed = new BuffableStat(tile.UnitSO.MovementSpeed);

            speed.AddBuff("UpgradeManager", UpgradeManager.Instance.GetBuff(UnitStatsEnum.MovementSpeed));

            CurrentPathIndex = currentPathIndex;

            speed.AddBuff("Tile", tile.GetTileBuff(UnitStatsEnum.MovementSpeed));

            EquipmentSO[] equipmentSOs = tile.Equipment;

            if (equipmentSOs != null)
            {
                for (int i = 0; i < equipmentSOs.Length; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    if (equipmentSOs[i].TryGetBuff(UnitStatsEnum.MovementSpeed, out Buff outBuff))
                    {
                        speed.AddBuff("Equipment" + (i + 1).ToString(), outBuff);
                    }
                }
            }
                

            Path = path;
            finishedPath = false;
        }

        private void Update()
        {
            HandleMovement();
            HandleAdditionalTimedBuff();
        }

        private void HandleMovement()
        {
            if (finishedPath)
            {
                onPathCompleted?.Invoke();

                GameManager.Instance.UnitPassed();
                SpawningManager.Instance.SpawnedUnits.Remove(GetComponent<Unit>());
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

            float distanceToMove = Speed * Time.deltaTime;

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

        private Dictionary<string, float> referenceAndTimeToRemoveBuff = new Dictionary<string, float>();

        public void ModifySpeedForTime(string reference, Buff movementBuff, float time)
        {
            speed.AddBuff(reference, movementBuff);

            if (referenceAndTimeToRemoveBuff.ContainsKey(reference))
            {
                referenceAndTimeToRemoveBuff[reference] = Time.time + time;
            }
            else
            {
                referenceAndTimeToRemoveBuff.Add(reference, Time.time + time);
            }
        }

        private void HandleAdditionalTimedBuff()
        {
            if (referenceAndTimeToRemoveBuff.Count <= 0)
                return;

            List<string> toRemove = new List<string>();

            foreach (KeyValuePair<string, float> kvp in referenceAndTimeToRemoveBuff)
            {
                if (Time.time >= kvp.Value)
                {
                    speed.RemoveBuff(kvp.Key);

                    toRemove.Add(kvp.Key);
                }
            }

            if (toRemove.Count <= 0)
                return;

            for (int i = 0; i < toRemove.Count; i++)
            {
                referenceAndTimeToRemoveBuff.Remove(toRemove[i]);
            }


        }
    }
}


