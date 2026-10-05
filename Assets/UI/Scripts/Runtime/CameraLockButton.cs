using Helper.Wait;
using LocalMap;
using UnityEngine;
using UnityEngine.UI;


public class CameraLockButton : MonoBehaviour
{
    [SerializeField]
    private Image cameraImage;

    [SerializeField]
    private Sprite lockedCameraSprite;
    [SerializeField]
    private Sprite unlockedCameraSprite;

    private void Awake()
    {
        LocalCameraMovementManager.WhenReady(Subscribe);
    }

    private void OnDestroy()
    {
        LocalCameraMovementManager.IfReady(manager => manager.updateUI -= UpdateUI);
    }

    private void Subscribe(LocalCameraMovementManager manager)
    {
        manager.updateUI += UpdateUI;
        UpdateUI(manager.CameraLocked);
    }

    public void UpdateUI(bool locked)
    {
        if(cameraImage != null)
            cameraImage.sprite = locked ? lockedCameraSprite : unlockedCameraSprite;
    }

    public void OnButtonClick()
    {
        LocalCameraMovementManager.Instance.ChangeCameraLock();
    }
}
