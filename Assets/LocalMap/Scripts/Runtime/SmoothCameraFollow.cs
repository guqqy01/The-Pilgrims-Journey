using UnityEngine;

namespace LocalMap
{
    // NOTE: Lives on the CAMERA RIG (same object as LocalCameraMovementManager).
    // All dead-zone/look-ahead math below is computed from this rig's own
    // position rather than from the live Camera transform, so a running
    // ScreenShakeManager (which offsets the child Camera's LOCAL transform
    // only) can never feed jitter back into the follow logic.
    public class SmoothCameraFollow : MonoBehaviour
    {
        public Transform target;

        [Header("Camera Smoothing")]
        [SerializeField] float smoothTime = 0.25f;

        [Header("Dead Zone (Viewport Space)")]
        [SerializeField] float leftPadding = 0.1f;
        [SerializeField] float rightPadding = 0.1f;
        [SerializeField] float topPadding = 0.1f;
        [SerializeField] float bottomPadding = 0.1f;

        [Header("Look Ahead")]
        [SerializeField] float lookAheadDistance = 2f;
        [SerializeField] float lookAheadSmooth = 0.25f;
        [SerializeField] float lookAheadThreshold = 0.1f;

        Camera cam;
        Vector3 camVelocity;
        Vector3 lookAheadOffset;
        Vector3 lookAheadVel;

        Vector3 lastTargetPos;

        void Start()
        {
            if (target != null)
                lastTargetPos = target.position;
        }

        // Manual viewport-space projection using the RIG's position rather
        // than cam.WorldToViewportPoint(), which would read the live
        // (possibly shake-offset) Camera transform instead.
        Vector3 WorldToRigViewport(Vector3 worldPos, Vector3 rigPos)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float vpX = 0.5f + (worldPos.x - rigPos.x) / (2f * halfWidth);
            float vpY = 0.5f + (worldPos.y - rigPos.y) / (2f * halfHeight);

            return new Vector3(vpX, vpY, 0f);
        }

        public void FollowTarget(Transform target)
        {
            this.target = target;
            if (target == null) return;

            cam = Camera.main;

            // Use the rig's own transform, not cam.transform - the Camera is
            // a child whose local position may currently be offset by shake.
            Vector3 camPos = transform.position;
            Vector3 targetPos = target.position;

            // -----------------------------------------------------
            // 1. Compute target velocity for look-ahead
            // -----------------------------------------------------
            Vector3 delta = targetPos - lastTargetPos;
            lastTargetPos = targetPos;

            Vector3 lookDir = Vector3.zero;
            if (delta.magnitude > lookAheadThreshold)
                lookDir = delta.normalized;

            Vector3 desiredLookAhead = lookDir * lookAheadDistance;
            lookAheadOffset = Vector3.SmoothDamp(
                lookAheadOffset,
                desiredLookAhead,
                ref lookAheadVel,
                lookAheadSmooth
            );

            // -----------------------------------------------------
            // 2. Dead-zone: compute desired camera position
            // -----------------------------------------------------
            Vector3 viewport = WorldToRigViewport(target.position, camPos);
            float minX = leftPadding;
            float maxX = 1f - rightPadding;
            float minY = bottomPadding;
            float maxY = 1f - topPadding;

            Vector3 desired = camPos;

            if (viewport.x < minX)
            {
                float dx = viewport.x - minX;
                desired.x += dx * cam.orthographicSize * cam.aspect * 2f;
            }
            else if (viewport.x > maxX)
            {
                float dx = viewport.x - maxX;
                desired.x += dx * cam.orthographicSize * cam.aspect * 2f;
            }

            if (viewport.y < minY)
            {
                float dy = viewport.y - minY;
                desired.y += dy * cam.orthographicSize * 2f;
            }
            else if (viewport.y > maxY)
            {
                float dy = viewport.y - maxY;
                desired.y += dy * cam.orthographicSize * 2f;
            }

            // -----------------------------------------------------
            // 3. Apply clamped look-ahead
            // -----------------------------------------------------
            Vector3 finalPos = desired + lookAheadOffset;
            Vector3 projectedViewport = WorldToRigViewport(target.position - lookAheadOffset, camPos);

            if (projectedViewport.x < minX || projectedViewport.x > maxX)
                finalPos.x = desired.x;

            if (projectedViewport.y < minY || projectedViewport.y > maxY)
                finalPos.y = desired.y;

            // -----------------------------------------------------
            // 4. Smooth move the rig (not the Camera itself)
            // -----------------------------------------------------
            transform.position = Vector3.SmoothDamp(
                camPos,
                new Vector3(finalPos.x, finalPos.y, camPos.z),
                ref camVelocity,
                smoothTime
            );
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) cam = Camera.main;
            if (cam == null) return;

            float minX = leftPadding;
            float maxX = 1f - rightPadding;
            float minY = bottomPadding;
            float maxY = 1f - topPadding;

            Vector3 bl = cam.ViewportToWorldPoint(new Vector3(minX, minY, cam.nearClipPlane + 10));
            Vector3 br = cam.ViewportToWorldPoint(new Vector3(maxX, minY, cam.nearClipPlane + 10));
            Vector3 tl = cam.ViewportToWorldPoint(new Vector3(minX, maxY, cam.nearClipPlane + 10));
            Vector3 tr = cam.ViewportToWorldPoint(new Vector3(maxX, maxY, cam.nearClipPlane + 10));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);
        }
    }
}