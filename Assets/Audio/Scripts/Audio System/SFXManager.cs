using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    [Header("UI")]
    [SerializeField] private AudioClipAndSettings buttonClick;
    [SerializeField] private AudioClipAndSettings openWindow;
    [SerializeField] private AudioClipAndSettings closeWindow;
    [Header("Tiles")]
    [SerializeField] private AudioClipAndSettings selectTile;
    [SerializeField] private AudioClipAndSettings deselectTile;
    [SerializeField] private AudioClipAndSettings pickupTile;
    [SerializeField] private AudioClipAndSettings moveTile;
    [SerializeField] private AudioClipAndSettings cancelMoveTile;
    [SerializeField] private AudioClipAndSettings placeTile;
    [SerializeField] private AudioClipAndSettings rotateTile;
    [Header("Equipment")]
    [SerializeField] private AudioClipAndSettings selectEquipment;
    public void PlayButtonClick() => buttonClick.Play();
    public void PlayOpenWindow() => openWindow.Play();
    public void PlayCloseWindow() => closeWindow.Play();
    public void PlaySelectTile() => selectTile.Play();
    public void PlayDeselectTile() => deselectTile.Play();
    public void PlayPickupTile() => pickupTile.Play();
    public void PlayMoveTile() => moveTile.Play();
    public void PlayCancelMoveTile() => cancelMoveTile.Play();
    public void PlayPlaceTile() => placeTile.Play();
    public void PlayRotateTile() => rotateTile.Play();
    public void PlaySelectEquipment() => selectEquipment.Play();

    private void Awake()
    {
        InitiateFromRootAtAwake();
    }
}
