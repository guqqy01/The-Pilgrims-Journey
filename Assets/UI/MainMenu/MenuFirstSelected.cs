using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Put this on your menu panel (or a manager object) and assign the button
/// that should be highlighted by default when the menu opens. Mouse users
/// don't need this, but keyboard/gamepad users have nothing selected until
/// something sets it explicitly.
/// </summary>
public class MenuFirstSelected : MonoBehaviour
{
    [SerializeField] private GameObject firstSelectedButton;

    private void OnEnable()
    {
        // Clearing first avoids a Unity quirk where re-selecting the same
        // object that was already selected doesn't fire OnSelect again.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
