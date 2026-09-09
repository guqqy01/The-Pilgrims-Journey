using LocalMap;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Heros
{
    public class HeroMovement : BaseMovement
    {
        [SerializeField]
        private Animator ani;

        Vector2 input;
        int hashUp, hashDown, hashLeft, hashRight, hashSpeed;


        public float Deadzone = 0.1f;

        private bool overrideMovementToMoveOutOfMap = false;
        private Direction directionOutOfMap;

        protected override void Awake()
        {
            base.Awake();
            ani = GetComponent<Animator>();

            // Cache animator parameter hashes
            hashUp = Animator.StringToHash("Up");
            hashDown = Animator.StringToHash("Down");
            hashLeft = Animator.StringToHash("Left");
            hashRight = Animator.StringToHash("Right");
            hashSpeed = Animator.StringToHash("Speed");
        }

        protected override void Update()
        {
            base.Update();

            input = InputManager.Instance.Actions.Player.Move.ReadValue<Vector2>();

            // Apply deadzone
            if (Mathf.Abs(input.x) < Deadzone) input.x = 0f;
            if (Mathf.Abs(input.y) < Deadzone) input.y = 0f;

            if (overrideMovementToMoveOutOfMap)
            {
                switch (directionOutOfMap)
                {
                    case Direction.South:
                        input = Vector2.down;
                        break;
                    case Direction.North:
                        input = Vector2.up;
                        break;
                    case Direction.East:
                        input = Vector2.right;
                        break;
                    case Direction.West:
                        //Not Moveing West;
                        break;
                }
            }

            UpdateAnimator(input);
        }

        public void ResetMovement()
        {
            overrideMovementToMoveOutOfMap = false;
        }

        public void MoveInDirectionOutOfMap(Direction direction)
        {
            overrideMovementToMoveOutOfMap = true;
            directionOutOfMap = direction;
        }

        //Set in UpdateAnimator, so we don't need to calculate it here
        protected override void CalculateLastDirection(){ }

        void UpdateAnimator(Vector2 dir)
        {
            if (ani == null) return;

            // Speed as blended float (0..1)
            float speed = Mathf.Clamp01(dir.magnitude);
            ani.SetFloat(hashSpeed, speed);

            // If no movement, clear all directional bools but keep LastDirection unchanged
            if (dir == Vector2.zero)
            {
                ani.SetBool(hashUp, false);
                ani.SetBool(hashDown, false);
                ani.SetBool(hashLeft, false);
                ani.SetBool(hashRight, false);
                return;
            }

            // Determine dominant axis with clearer absolute checks and set LastDirection
            if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            {
                bool up = dir.y > 0f;

                // Record last faced direction (vertical)
                LastDirection = up ? Direction.North : Direction.South;

                ani.SetBool(hashUp, up);
                ani.SetBool(hashDown, !up);
                ani.SetBool(hashLeft, false);
                ani.SetBool(hashRight, false);
            }
            else
            {
                bool right = dir.x > 0f;

                // Record last faced direction (horizontal)
                LastDirection = right ? Direction.East : Direction.West;

                ani.SetBool(hashRight, right);
                ani.SetBool(hashLeft, !right);
                ani.SetBool(hashUp, false);
                ani.SetBool(hashDown, false);
            }
        }


        public override void HandleFixedUpdateMovement()
        {
            if (rb == null) return;

            Vector2 move = input;
            if (move.sqrMagnitude > 1f) move = move.normalized;

            Vector2 movementVector = move * SpeedValue * Time.fixedDeltaTime;

            Vector2 newPos = rb.position + move * SpeedValue * Time.fixedDeltaTime;

            if (LocalMapGenerator.Instance.IsPositionOutOfMap(newPos))
            {
                //Walking to next stage, allow to happen
                rb.MovePosition(newPos);
                return;
            }

            if (LocalMapGenerator.Instance.IsPositionWalkable(newPos))
            {
                rb.MovePosition(newPos);
                return;
            }

            if(LocalMapGenerator.Instance.TryFindNearestWalkable(newPos, out Vector2 nearestWalkable, 1))
            {
                rb.MovePosition(nearestWalkable);
                return;
            }


            // Try sliding along each axis independently
            Vector2 newPosX = rb.position + new Vector2(move.x, 0) * SpeedValue * Time.fixedDeltaTime;
            Vector2 newPosY = rb.position + new Vector2(0, move.y) * SpeedValue * Time.fixedDeltaTime;

            if (LocalMapGenerator.Instance.IsPositionWalkable(newPosX))
            {
                rb.MovePosition(newPosX);
            }
            else if (LocalMapGenerator.Instance.IsPositionWalkable(newPosY))
            {
                rb.MovePosition(newPosY);

            }
            // else: fully blocked, don't move at all
        }

        protected override bool GetIfAtTargetPosition()
        {
            return input.sqrMagnitude > 0.1f;
        }
    }

}
