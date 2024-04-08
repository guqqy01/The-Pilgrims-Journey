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

        public int CurrentPathIndex { private set; get; } = 0;


        private bool finishedPath = false;

        [SerializeField]
        private Transform visualTransform;

        private float initialXScale;

        private void Awake()
        {
            initialXScale = visualTransform.localScale.x;
        }

        public void Init(List<Vector2Int> path, int currentPathIndex, UnitSO unitSO)
        {
            speed = new BuffableFloatStat(unitSO.MovementSpeed);

            CurrentPathIndex = currentPathIndex;

            Path = path;
            finishedPath = false;
        }

        public void Init(List<Vector2Int> path, int currentPathIndex, Tile tile)
        {
            speed = new BuffableFloatStat(tile.Unit.MovementSpeed);

            CurrentPathIndex = currentPathIndex;

            speed.AddMultiplierBuff("Tile", tile.movementMultiplier);

            List<EquipmentSO> equipmentSOs = tile.Equipment;

            if (equipmentSOs != null)
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
            HandleMovement();
            HandleAdditionalTimedShield();
        }

        private void HandleMovement()
        {
            if (finishedPath)
            {
                GameManager.Instance.UnitPassed();
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

        public void ModifySpeedForTime(string reference, float amount, float time)
        {
            speed.AddMultiplierBuff(reference, amount);

            if (referenceAndTimeToRemoveBuff.ContainsKey(reference))
            {
                referenceAndTimeToRemoveBuff[reference] = Time.time + time;
            }
            else
            {
                referenceAndTimeToRemoveBuff.Add(reference, Time.time + time);
            }
        }

        private void HandleAdditionalTimedShield()
        {
            if (referenceAndTimeToRemoveBuff.Count <= 0)
                return;

            List<string> toRemove = new List<string>();

            foreach (KeyValuePair<string, float> kvp in referenceAndTimeToRemoveBuff)
            {
                if (Time.time >= kvp.Value)
                {
                    speed.RemoveMultiplierBuff(kvp.Key);

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


