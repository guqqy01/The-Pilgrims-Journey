using UnityEngine;

namespace Hero
{
    public class HeroMovement : BaseMovement
    {


        [SerializeField]
        private Animator ani;

        [SerializeField]
        private Rigidbody2D rb;

        Vector2 input;
        int hashUp, hashDown, hashLeft, hashRight, hashSpeed;


        public float Deadzone = 0.1f;


        private void Start()
        {
            BaseInit(2f);
        }

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
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

            // Read input in Update (keeps input responsive)
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");

            // Apply deadzone
            if (Mathf.Abs(input.x) < Deadzone) input.x = 0f;
            if (Mathf.Abs(input.y) < Deadzone) input.y = 0f;

            UpdateAnimator(input);
        }

        void UpdateAnimator(Vector2 dir)
        {
            if (ani == null) return;

            // Speed as blended float (0..1)
            float speed = Mathf.Clamp01(dir.magnitude);
            ani.SetFloat(hashSpeed, speed);

            // If no movement, clear all directional bools
            if (dir == Vector2.zero)
            {
                ani.SetBool(hashUp, false);
                ani.SetBool(hashDown, false);
                ani.SetBool(hashLeft, false);
                ani.SetBool(hashRight, false);
                return;
            }

            // Determine dominant axis with clearer absolute checks
            if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            {
                bool up = dir.y > 0f;
                ani.SetBool(hashUp, up);
                ani.SetBool(hashDown, !up);
                ani.SetBool(hashLeft, false);
                ani.SetBool(hashRight, false);
            }
            else
            {
                bool right = dir.x > 0f;
                ani.SetBool(hashRight, right);
                ani.SetBool(hashLeft, !right);
                ani.SetBool(hashUp, false);
                ani.SetBool(hashDown, false);
            }
        }


        public override void HandleMovement()
        {
            if (rb == null) return;

            Vector2 move = input;
            if (move.sqrMagnitude > 1f) move = move.normalized; // prevents faster diagonal speed

            Vector2 newPos = rb.position + move * SpeedValue * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

}
