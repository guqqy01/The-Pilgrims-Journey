using Heros;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalMap
{
    // NOTE: This component should live on the CAMERA RIG (the parent object),
    // not on the Camera itself. The actual Camera is a child of this object.
    // That way this script owns "where the camera logically is" (rig.position),
    // and ScreenShakeManager can independently offset the child Camera's local
    // transform for shake without the two ever fighting over the same values.
    [RequireComponent(typeof(SmoothCameraFollow))]
    public class LocalCameraMovementManager : Singleton<LocalCameraMovementManager>
    {
        private Camera cam;
        private Camera Cam
        {
            get
            {
                if (cam == null)
                    cam = Camera.main;

                return cam;
            }
        }

        private SmoothCameraFollow smoothCameraFollow;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float zoomMin = 2f;

        [Header("Movement")]
        [SerializeField] private float cameraSpeed = 2f;

        [Header("Padding (in pixels)")]
        [SerializeField] private float topPaddingPixels;
        public float TopPaddingPixels { get { return topPaddingPixels; } }

        [SerializeField] private float bottomPaddingPixels;
        public float BottomPaddingPixels { get { return bottomPaddingPixels; } }

        private int MapSizeX => LocalMapGenerator.Instance.MapSizeX;
        private int MapSizeY => LocalMapGenerator.Instance.MapSizeY;

        private float HalfCamHeight => Cam.orthographicSize;
        private float HalfCamWidth => HalfCamHeight * Cam.aspect;
        private float MapLeft => -0.5f;
        private float MapRight => MapSizeX - 0.5f;

        private float MapWidth => MapSizeX;
        private float MapHeight => MapSizeY;

        private float MaxZoom => CalculateMaxZoom();

        // For mouse dragging
        private Vector3 dragOrigin;
        private bool isDragging;

        private bool cameraLock = true;
        public bool CameraLocked
        {
            get
            {
                return cameraLock;
            }
            set
            {
                cameraLock = value;

                if (cameraLock)
                    InputManager.CameraMovement.Disable();
                else InputManager.CameraMovement.Enable();

                updateUI?.Invoke(cameraLock);
            }
        }

        public delegate void UpdateUI(bool cameraLocked);
        public UpdateUI updateUI;

        private void Awake()
        {
            smoothCameraFollow = GetComponent<SmoothCameraFollow>();
        }

        private void LateUpdate()
        {
            if (cameraLock)
            {
                if (HeroManager.Instance.ActiveHero != null)
                    smoothCameraFollow.FollowTarget(HeroManager.Instance.ActiveHero.transform);
            }

            Clamp();
        }

        private void Update()
        {
            if (!cameraLock)
                HandleMouseDrag();

            MoveCamera();
            ZoomCamera();
        }

        public void ChangeCameraLock()
        {
            CameraLocked = !cameraLock;
        }

        private float CalculateMaxZoom()
        {
            float maxZoomVertical = MapHeight / 2f;
            float maxZoomHorizontal = MapWidth / (2f * Cam.aspect);

            float unitsPerPixel = MapHeight / Screen.height;
            float totalVerticalPadding = (topPaddingPixels + bottomPaddingPixels) * unitsPerPixel;

            return Mathf.Min(maxZoomVertical + totalVerticalPadding, maxZoomHorizontal);
        }

        private float GetVerticalWorldPadding(float pixelPadding)
        {
            float unitsPerPixel = (Cam.orthographicSize * 2f) / (float)Screen.height;
            return pixelPadding * unitsPerPixel;
        }

        private void Clamp()
        {
            // Use the rig's own transform (this object), NOT Cam.transform.
            // Cam is a child that may have a shake offset applied to its
            // local position - we never want to clamp against that.
            Vector3 rigPos = transform.position;

            float MapTop = MapSizeY - 0.5f;
            float MapBottom = -0.5f;

            float CameraYPosMax = MapTop/* - HalfCamHeight + GetVerticalWorldPadding(topPaddingPixels)*/;
            float CameraYPosMin = MapBottom/* + HalfCamHeight - GetVerticalWorldPadding(bottomPaddingPixels)*/;

            float CameraXPosMin = MapLeft/* + HalfCamWidth*/;
            float CameraXPosMax = MapRight/* - HalfCamWidth*/;

            float camX = Mathf.Clamp(rigPos.x, CameraXPosMin, CameraXPosMax);
            float camY = Mathf.Clamp(rigPos.y, CameraYPosMin, CameraYPosMax);
            transform.position = new Vector3(camX, camY, rigPos.z);
        }

        public void FullZoomCamera()
        {
            transform.position = new Vector3(MapSizeX / 2f - 0.5f, MapSizeY / 2f - 0.5f, transform.position.z);
            Cam.orthographicSize = MaxZoom;
        }

        private void MoveCamera()
        {
            Vector3 move = InputManager.CameraMovement.Look.ReadValue<Vector2>() * cameraSpeed * Time.deltaTime;
            transform.position += move;
        }

        private void HandleMouseDrag()
        {
            if (InputManager.UI.Click.WasPressedThisFrame())
            {
                dragOrigin = Cam.ScreenToWorldPoint(PointerManager.Instance.PointerPosition);
                isDragging = true;
            }

            if (InputManager.UI.Click.IsPressed() && isDragging)
            {
                Vector3 currentPos = Cam.ScreenToWorldPoint(PointerManager.Instance.PointerPosition);
                Vector3 difference = dragOrigin - currentPos;
                transform.position += difference;
            }

            if (InputManager.UI.Click.WasReleasedThisFrame())
            {
                isDragging = false;
            }
        }

        private void ZoomCamera()
        {
            if (Mouse.current.scroll.value.y == 0)
                return;

            if (PointerManager.Instance.PointerOverUI)
                return;

            float newSize = Cam.orthographicSize - InputManager.CameraMovement.Zoom.ReadValue<Vector2>().y * zoomSpeed * Time.deltaTime;
            newSize = Mathf.Clamp(newSize, zoomMin, MaxZoom);
            Cam.orthographicSize = newSize;
        }
    }
}