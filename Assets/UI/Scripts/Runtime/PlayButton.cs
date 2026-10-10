using GameManagerCalls;
using LocalMap;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour, IOnCompletedLevel, IOnCreateLocalMap
{
    [SerializeField]
    private Button button;

    private bool completedLevel = false;

    public void OnLevelCompleted()
    {
        completedLevel = true;
    }

    public void OnCreateLocalMap()
    {
        button.interactable = true;
        completedLevel = false;
    }

    public void OnButtonClick()
    {
        if (completedLevel)
            return;

        // The old TilePlayerManager.Play() bailed out here too, but the button was still disabled
        // and the play interfaces were still called. Now nothing starts until a path is picked.
        if (PathPicker.Instance == null || PathPicker.Instance.PickedDirection == null)
            return;

        button.interactable = false;
        InterfaceCaller.CallInterfaces<IOnPlayLocalLevel>(x => x.OnPlayLocalLevel());
        InterfaceCaller.CallInterfaces<IOnSave>(x => x.Save());
    }
}