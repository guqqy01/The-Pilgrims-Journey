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
        WaitForObjectHelper.WaitForObjectThenPerformAction(() => LocalCameraMovementManager.Instance, SubscribeToLocalCameraMovementManager);
    }

    private void OnDestroy()
    {
        if(LocalCameraMovementManager.Instance != null)
            LocalCameraMovementManager.Instance.updateUI -= UpdateUI;
    }

    private void SubscribeToLocalCameraMovementManager()
    {
        LocalCameraMovementManager.Instance.updateUI += UpdateUI;
    }

    public void UpdateUI(bool locked)
    {
        cameraImage.sprite = locked ? lockedCameraSprite : unlockedCameraSprite;
    }

    public void OnButtonClick()
    {
        LocalCameraMovementManager.Instance.ChangeCameraLock();
    }
}
