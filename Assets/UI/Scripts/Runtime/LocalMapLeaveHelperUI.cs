using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Monsters;
using System.Collections;
using GameManagerCalls;
using LocalMap;
using System;

public class LocalMapLeaveHelperUI : MonoBehaviour, IOnAfterCreateLocalMap, IOnCompleteLocalMapLevel
{
    [Header("Helper Arrow")]
    [SerializeField]
    private RectTransform arrowMovementArea;
    [SerializeField]
    private Image arrowHelper;
    [SerializeField]
    private Vector4 arrowPadding; // (left, top, right, bottom)

    [Header("World Target")]
    [SerializeField]
    private Camera worldCamera; // defaults to Camera.main if left null
    [SerializeField]
    private Canvas parentCanvas; // needed if the canvas isn't Screen Space - Overlay

    [Header("Text")]
    [SerializeField]
    private TextMeshProUGUI leaveHintText;
    [SerializeField]
    private TextMeshProUGUI countdownText;

    private Coroutine countDownCoroutine;
    private Coroutine hintTextCoroutine;
    private Coroutine hideTextCoroutine;

    private bool isHordeActive;

    [SerializeField]
    private float timeToHideHintMessage = 4f;
    [SerializeField]
    private float hideTimeLength = 0.2f;

    private bool hideTriggered;

    [SerializeField]
    private float textTypeSpeedPerSecond = 20f; // characters per second

    [SerializeField]
    private GameObject scytheImage;


    private void Start()
    {
        MonsterSpawnManager.WhenReady(Subscribe);
    }

    private void OnDestroy()
    {
        if (MonsterSpawnManager.Instance != null)
        {
            MonsterSpawnManager.Instance.OnHordeStart -= OnHordeStart;
            MonsterSpawnManager.Instance.OnHordeFinished -= OnHordeFinished;
        }
    }

    private void Update()
    {
        if (!isHordeActive)
        {
            if (arrowHelper.isActiveAndEnabled)
            {
                arrowHelper.gameObject.SetActive(false);
            }
            return;
        }

        if (!arrowHelper.isActiveAndEnabled)
        {
            arrowHelper.gameObject.SetActive(true);
        }

        Vector2 targetWorldPos = GetTargetOutPosition();
        UpdateArrow(targetWorldPos);
    }

    private void UpdateArrow(Vector2 targetWorldPos2D)
    {
        Camera cam = worldCamera != null ? worldCamera : Camera.main;
        if (cam == null || arrowMovementArea == null || arrowHelper == null)
        {
            return;
        }

        Vector3 targetWorldPos = new Vector3(targetWorldPos2D.x, targetWorldPos2D.y, 0f);

        // Detect if the point is behind the camera (relevant for perspective cams)
        Vector3 viewportPoint = cam.WorldToViewportPoint(targetWorldPos);
        Vector3 screenPoint = cam.WorldToScreenPoint(targetWorldPos);

        if (viewportPoint.z < 0f)
        {
            // Flip to the opposite side so the arrow still points the right way
            screenPoint.x = Screen.width - screenPoint.x;
            screenPoint.y = Screen.height - screenPoint.y;
        }

        Camera uiCamera = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? parentCanvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            arrowMovementArea, screenPoint, uiCamera, out Vector2 localTargetPoint);

        Rect area = arrowMovementArea.rect;
        Vector2 center = area.center;

        float xMin = area.xMin + arrowPadding.x;
        float xMax = area.xMax - arrowPadding.z;
        float yMin = area.yMin + arrowPadding.w;
        float yMax = area.yMax - arrowPadding.y;

        Vector2 dir = localTargetPoint - center;

        Vector2 clampedPos = ClampDirectionToBounds(dir, center, xMin, xMax, yMin, yMax);

        RectTransform arrowRect = arrowHelper.rectTransform;
        arrowRect.anchoredPosition = clampedPos;

        if (dir.sqrMagnitude > 0.0001f)
        {
            // Assumes the arrow sprite points "up" by default
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // Clamps a point (center + dir) to a rectangle, preserving direction.
    // If the point is already inside the bounds, it's returned unclamped.
    private Vector2 ClampDirectionToBounds(Vector2 dir, Vector2 center, float xMin, float xMax, float yMin, float yMax)
    {
        if (dir.sqrMagnitude <= 0.0001f)
        {
            return center;
        }

        float relXMax = xMax - center.x;
        float relXMin = xMin - center.x;
        float relYMax = yMax - center.y;
        float relYMin = yMin - center.y;

        float tx = float.PositiveInfinity;
        if (dir.x > 0f) tx = relXMax / dir.x;
        else if (dir.x < 0f) tx = relXMin / dir.x;

        float ty = float.PositiveInfinity;
        if (dir.y > 0f) ty = relYMax / dir.y;
        else if (dir.y < 0f) ty = relYMin / dir.y;

        float t = Mathf.Min(tx, ty);
        t = Mathf.Clamp(t, 0f, 1f); // 1 = target already inside bounds

        return center + dir * t;
    }

    private Vector2 GetTargetOutPosition()
    {
        if (PathPicker.Instance == null)
        {
            return Vector2.zero;
        }
        if (LocalMapGenerator.Instance == null || LocalMapGenerator.Instance.pathway == null)
        {
            return Vector2.zero;
        }
        if (!PathPicker.Instance.PickedDirection.HasValue)
        {
            return Vector2.zero;
        }

        Direction pickedDirection = PathPicker.Instance.PickedDirection.Value;
        return LocalMapGenerator.Instance.pathway.ToEndPoints[pickedDirection];
    }

    private void Subscribe(MonsterSpawnManager monsterSpawnManager)
    {
        monsterSpawnManager.OnHordeStart += OnHordeStart;
        monsterSpawnManager.OnHordeFinished += OnHordeFinished;
    }

    private void OnHordeStart(float hoardTime)
    {
        isHordeActive = true;
        scytheImage.SetActive(true);
        if (hintTextCoroutine != null) StopCoroutine(hintTextCoroutine);
        if (hideTextCoroutine != null) StopCoroutine(hideTextCoroutine);
        hintTextCoroutine = StartCoroutine(ShowHintText("Death approaches!\r\n" + "Leave!"));

        if (countDownCoroutine != null)
            StopCoroutine(countDownCoroutine);
        countDownCoroutine = StartCoroutine(CountdownCoroutine(hoardTime));
    }


    private IEnumerator CountdownCoroutine(float countdownTime)
    {
        float timeToHide = Time.time + timeToHideHintMessage;
        float remainingTime = countdownTime;
        hideTriggered = false;

        while (remainingTime > 0)
        {
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString() + " Sec!";

            if (!hideTriggered && Time.time > timeToHide)
            {
                hideTriggered = true;
                hideTextCoroutine = StartCoroutine(HideText());
            }

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }
        countdownText.text = "";
    }

    private void OnHordeFinished()
    {
        if (countDownCoroutine != null) StopCoroutine(countDownCoroutine);
        if (hintTextCoroutine != null) StopCoroutine(hintTextCoroutine);
        if (hideTextCoroutine != null) StopCoroutine(hideTextCoroutine);
        scytheImage.SetActive(false);
        isHordeActive = false;
        hintTextCoroutine = StartCoroutine(ShowHintText("Death is Here!\r\n" + "Run!"));
        countdownText.text = "";
        hideTextCoroutine = StartCoroutine(HideText());
    }

    private IEnumerator ShowHintText(string text)
    {
        leaveHintText.alpha = 1f;
        leaveHintText.text = "";
        while (leaveHintText.text.Length < text.Length)
        {
            leaveHintText.text = text.Substring(0, leaveHintText.text.Length + 1);
            yield return new WaitForSeconds(1f / textTypeSpeedPerSecond);
        }
    }

    private IEnumerator HideText()
    {
        yield return new WaitForSeconds(timeToHideHintMessage);

        float startAlpha = leaveHintText.alpha;

        float alphaT = Mathf.InverseLerp(1f, 0f, startAlpha);

        float duration = Mathf.Lerp(hideTimeLength, 0f, alphaT);

        float startTime = Time.time;
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = duration > 0f ? (Time.time - startTime) / duration : 1f;
            leaveHintText.alpha = Mathf.Lerp(startAlpha, 0, t);
            yield return null;
        }

        leaveHintText.text = "";
        leaveHintText.alpha = 1f;
    }

    private void CleanUp()
    {
        isHordeActive = false;
        scytheImage.SetActive(false);
        leaveHintText.text = "";
        countdownText.text = "";
    }

    public void AfterCreateLocalMap()
    {
        CleanUp();
    }

    public void OnCompleteLocalMapLevel()
    {
        CleanUp();
    }
}