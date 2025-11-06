using Map;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocalCameraMovementManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static LocalCameraMovementManager Instance { get; private set; }
    private Camera cam;
    private Camera Cam
    {
        get
        {
            if(cam == null)
                cam = Camera.main;

            return cam;
        }
    }


    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float zoomMin = 2f;

    [Header("Movement")]
    [SerializeField] private float cameraSpeed = 2f;

    [Header("Padding (in pixels)")]
    [SerializeField] private float topPaddingPixels;
    [SerializeField] private float bottomPaddingPixels;

    private KeyCode cameraMovementLeft = KeyCode.LeftArrow;
    private KeyCode cameraMovementRight = KeyCode.RightArrow;
    private KeyCode cameraMovementUp = KeyCode.UpArrow;
    private KeyCode cameraMovementDown = KeyCode.DownArrow;

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

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        HandleMouseDrag();
        MoveCamera();
        ZoomCamera();
        Clamp();
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
        Vector3 camPos = Cam.transform.position;

        float MapTop = MapSizeY - 0.5f;
        float MapBottom = -0.5f;

        float CameraYPosMax = MapTop - HalfCamHeight + GetVerticalWorldPadding(topPaddingPixels);
        float CameraYPosMin = MapBottom + HalfCamHeight - GetVerticalWorldPadding(bottomPaddingPixels);

        float CameraXPosMin = MapLeft + HalfCamWidth;
        float CameraXPosMax = MapRight - HalfCamWidth;

        float camX = Mathf.Clamp(camPos.x, CameraXPosMin, CameraXPosMax);
        float camY = Mathf.Clamp(camPos.y, CameraYPosMin, CameraYPosMax);
        Cam.transform.position = new Vector3(camX, camY, camPos.z);
    }

    public void FullZoomCamera()
    {
        Cam.transform.position = new Vector3(MapSizeX / 2f - 0.5f, MapSizeY / 2f - 0.5f, -10f);
        Cam.orthographicSize = MaxZoom;
    }

    private void MoveCamera()
    {
        float moveX = (Input.GetKey(cameraMovementRight) ? 1 : 0) - (Input.GetKey(cameraMovementLeft) ? 1 : 0);
        float moveY = (Input.GetKey(cameraMovementUp) ? 1 : 0) - (Input.GetKey(cameraMovementDown) ? 1 : 0);

        Vector3 move = new Vector3(moveX, moveY, 0f).normalized * cameraSpeed * Time.deltaTime;
        Cam.transform.position += move;
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0) && !InputManager.Instance.MouseOverUI)
        {
            dragOrigin = Cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentPos = Cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPos;
            Cam.transform.position += difference;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void ZoomCamera()
    {
        if(Input.mouseScrollDelta.y == 0)
            return;

        if (InputManager.Instance.MouseOverUI)
            return;

        float newSize = Cam.orthographicSize - Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
        newSize = Mathf.Clamp(newSize, zoomMin, MaxZoom);
        Cam.orthographicSize = newSize;
    }
}