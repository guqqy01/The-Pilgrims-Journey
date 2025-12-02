using UnityEngine;

namespace LocalMap
{
    public class  SmoothCameraFollow : MonoBehaviour
    {
        private Camera cam;
        public Camera Cam
        {
            get
            {
                if (cam == null)
                    cam = Camera.main;

                return cam;
            }
        }

        [SerializeField]
        private float smoothSpeed = 0.125f;

        [SerializeField]
        private float leftPadding = 0f;
        [SerializeField]
        private float rightPadding = 0f;
        [SerializeField]
        private float topPadding = 0f;
        [SerializeField]
        private float bottomPadding = 0f;

        public void FollowTarget(Transform target)
        {
            // Current camera position
            Vector3 camPos = Cam.transform.position;

            // Desired position is target’s position but keep camera’s Z
            Vector3 desiredPos = new Vector3(target.position.x, target.position.y, camPos.z);

            // Convert target position to viewport space
            Vector3 viewportPos = Cam.WorldToViewportPoint(target.position);

            // Clamp viewport position based on padding
            float minX = leftPadding / Cam.pixelWidth;
            float maxX = 1f - (rightPadding / Cam.pixelWidth);
            float minY = bottomPadding / Cam.pixelHeight;
            float maxY = 1f - (topPadding / Cam.pixelHeight);

            // If target is outside padding, adjust desired position
            if (viewportPos.x < minX)
                desiredPos.x = Cam.ViewportToWorldPoint(new Vector3(minX, 0, camPos.z - Cam.transform.position.z)).x;
            else if (viewportPos.x > maxX)
                desiredPos.x = Cam.ViewportToWorldPoint(new Vector3(maxX, 0, camPos.z - Cam.transform.position.z)).x;

            if (viewportPos.y < minY)
                desiredPos.y = Cam.ViewportToWorldPoint(new Vector3(0, minY, camPos.z - Cam.transform.position.z)).y;
            else if (viewportPos.y > maxY)
                desiredPos.y = Cam.ViewportToWorldPoint(new Vector3(0, maxY, camPos.z - Cam.transform.position.z)).y;

            // Smoothly move camera
            Cam.transform.position = Vector3.Lerp(camPos, desiredPos, smoothSpeed);
        }

        private void OnDrawGizmos()
        {
            if (Cam == null) return;

            Gizmos.color = Color.yellow;

            // Draw padding rectangle in world space
            Vector3 bottomLeft = Cam.ViewportToWorldPoint(new Vector3(leftPadding / Cam.pixelWidth, bottomPadding / Cam.pixelHeight, Cam.nearClipPlane));
            Vector3 topRight = Cam.ViewportToWorldPoint(new Vector3(1f - (rightPadding / Cam.pixelWidth), 1f - (topPadding / Cam.pixelHeight), Cam.nearClipPlane));

            Vector3 topLeft = new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);
            Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}
