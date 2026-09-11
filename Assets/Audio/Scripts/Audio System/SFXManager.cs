using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    [SerializeField]
    private SFXUISettings uISFX;
    public void PlayButtonClick() => uISFX.PlayButtonClick();
    public void PlayButtonHover() => uISFX.PlayButtonHover();
    public void PlayOpenWindow() => uISFX.PlayOpenWindow();
    public void PlayCloseWindow() => uISFX.PlayCloseWindow();
    public void PlaySelectTile() => uISFX.PlaySelectTile();
    public void PlayDeselectTile() => uISFX.PlayDeselectTile();
    public void PlayPickupTile() => uISFX.PlayPickupTile();
    public void PlayMoveTile() => uISFX.PlayMoveTile();
    public void PlayCancelMoveTile() => uISFX.PlayCancelMoveTile();
    public void PlayPlaceTile() => uISFX.PlayPlaceTile();
    public void PlayRotateTile() => uISFX.PlayRotateTile();
    public void PlaySelectEquipment() => uISFX.PlaySelectEquipment();

    private void Awake()
    {
        InitiateFromRootAtAwake();
    }
}
