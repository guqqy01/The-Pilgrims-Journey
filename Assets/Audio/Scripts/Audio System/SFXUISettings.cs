using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SFX/UI")]
public class SFXUISettings : ScriptableObject
{
    [Foldout("Buttons")]
    public AudioClipAndSettings ButtonClick, ButtonHover;

    [Foldout("Window")]
    public AudioClipAndSettings OpenWindow, CloseWindow;

    [Foldout("Tiles")]
    public AudioClipAndSettings
        selectTile,
        deselectTile,
        pickupTile,
        moveTile,
        cancelMoveTile,
        placeTile,
        rotateTile;
    [Foldout("Equipment")]

    public AudioClipAndSettings selectEquipment;

    public void PlayButtonClick() => ButtonClick.Play(AudioType.UI);
    public void PlayButtonHover() => ButtonHover.Play(AudioType.UI);
    public void PlayOpenWindow() => OpenWindow.Play(AudioType.UI);
    public void PlayCloseWindow() => CloseWindow.Play(AudioType.UI);
    public void PlaySelectTile() => selectTile.Play(AudioType.UI);
    public void PlayDeselectTile() => deselectTile.Play(AudioType.UI);
    public void PlayPickupTile() => pickupTile.Play(AudioType.UI);
    public void PlayMoveTile() => moveTile.Play(AudioType.UI);
    public void PlayCancelMoveTile() => cancelMoveTile.Play(AudioType.UI);
    public void PlayPlaceTile() => placeTile.Play(AudioType.UI);
    public void PlayRotateTile() => rotateTile.Play(AudioType.UI);
    public void PlaySelectEquipment() => selectEquipment.Play(AudioType.UI);
}