using System.Collections;
using UnityEngine;

public class AltarSpawnController : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField]
    private ParticleSystem dirtKickupPS;
    [SerializeField]
    private Transform pSTransform;

    [Header("Altar Setup")]
    [SerializeField]
    private Altar altar;
    [SerializeField]
    private SpriteRenderer altarSpriteRenderer;
    [SerializeField]
    private Transform altarVisualTransform;
    [SerializeField]
    private float riseAnimationTime = 1.5f;

    [Header("Canvas Setup")]
    [SerializeField]
    private Canvas altarCanvas;
    [SerializeField]
    private CanvasGroup altarCanvasGroup;

    [SerializeField]
    private AltarUI altarUI;

    [SerializeField]
    private float showCanvasTime = 0.5f;

    [Header("Mask Setup")]
    [SerializeField]
    private SpriteMask mask;
    [SerializeField]
    private Transform maskTransform;

    private Coroutine _riseCoroutine;
    private Coroutine _showCanvasCoroutine;
    private void Setup(int layer)
    {
        altarSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        ParticleSystemRenderer particleSystemRenderer = dirtKickupPS.GetComponent<ParticleSystemRenderer>();


        altarSpriteRenderer.sortingOrder = layer;
        particleSystemRenderer.sortingOrder = layer + 1;
        altarCanvas.sortingOrder = layer + 2;

        altarCanvasGroup.alpha = 0f;

        Sprite altarSprite = altar.AltarSO.altarSprite;

        float pixelsPerUnit = altarSprite.pixelsPerUnit;
        Vector2 spritePixelSize = altarSprite.textureRect.size;

        Vector2 spriteUnitSize = spritePixelSize / pixelsPerUnit;
        maskTransform.localScale = spriteUnitSize;

        altarUI.SetupUIPlacement(spriteUnitSize.y);

        pSTransform.localPosition = new Vector3(0, - spriteUnitSize.y / 2f, 0);

        mask.frontSortingOrder = layer;
        mask.backSortingOrder = layer - 1;

        if (_riseCoroutine != null)
            StopCoroutine(_riseCoroutine);

        if (_showCanvasCoroutine != null)
            StopCoroutine(_showCanvasCoroutine);

        altarVisualTransform.localPosition = new Vector3(0, -spriteUnitSize.y, 0);

        if(dirtKickupPS.isPlaying)
            dirtKickupPS.Stop();
    }

    public void StartSpawnAnimation(int layer)
    {
        Setup(layer);

        _riseCoroutine = StartCoroutine(RiseAnimation());
    }

    private IEnumerator RiseAnimation()
    {
        Sprite altarSprite = altar.AltarSO.altarSprite;
        float pixelsPerUnit = altarSprite.pixelsPerUnit;
        Vector2 spritePixelSize = altarSprite.textureRect.size;
        Vector2 spriteUnitSize = spritePixelSize / pixelsPerUnit;

        float riseSpeed = spriteUnitSize.y / riseAnimationTime;

        dirtKickupPS.Play();

        while (altarVisualTransform.localPosition.y < 0)
        {
            float yPos = Mathf.Min(0, altarVisualTransform.localPosition.y + riseSpeed * Time.deltaTime);
            altarVisualTransform.localPosition = new Vector3(0, yPos, 0);
            yield return null;
        }

        dirtKickupPS.Stop();

        if (_showCanvasCoroutine != null)
            StopCoroutine(_showCanvasCoroutine);

        _showCanvasCoroutine = StartCoroutine(ShowCanvas());

        AnimationCompleteRemoveMask();
    }

    private IEnumerator ShowCanvas()
    {
        float elapsedTime = 0f;
        while (elapsedTime < showCanvasTime)
        {
            elapsedTime += Time.deltaTime;
            altarCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / showCanvasTime);
            yield return null;
        }
        altarCanvasGroup.alpha = 1f;
    }

    private void AnimationCompleteRemoveMask()
    {
        altarSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;
    }
}
