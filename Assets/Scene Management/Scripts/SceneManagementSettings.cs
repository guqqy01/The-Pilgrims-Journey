using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scene/Scene Management Settings")]
public class SceneManagementSettings : ScriptableObject
{
    [SerializeField]
    private SceneLoadItem InGameAsset;
    [SerializeField]
    private SceneLoadItem PhaseOneLoad;
    [SerializeField]
    private SceneLoadItem PhaseTwoLoad;
    [SerializeField]
    private SceneLoadItem ShopSceneLoad;
    [SerializeField]
    private SceneLoadItem MainMenuSceneLoad;
    [SerializeField]
    private SceneLoadItem RandomEventsLoad;
    [SerializeField]
    private SceneLoadItem LoadingSceneLoad;
    [SerializeField]
    private SceneLoadItem GlobalMapLoad;

    public SceneLoadItem GetScenLoadItem(SceneLoad sceneLoad)
    {
        switch (sceneLoad)
        {
            case SceneLoad.InGame:
                return InGameAsset;
            case SceneLoad.CardGame:
                return PhaseOneLoad;
            case SceneLoad.LocalMap:
                return PhaseTwoLoad;
            case SceneLoad.Shop:
                return ShopSceneLoad;
            case SceneLoad.MainMenu:
                return MainMenuSceneLoad;
            case SceneLoad.RandomEvent:
                return RandomEventsLoad;
            case SceneLoad.LoadingScreen:
                return LoadingSceneLoad;
            case SceneLoad.GlobalMap:
                return GlobalMapLoad;
            default:
                throw new NotImplementedException("Unhandled scene load type.");
        }
    }
}