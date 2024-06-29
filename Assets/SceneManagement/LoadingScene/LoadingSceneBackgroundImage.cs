using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class LoadingSceneBackgroundImage : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static LoadingSceneBackgroundImage Instance;

    [SerializeField]
    private Image image;

    [SerializeField]
    private List<Sprite> backgroundImages;

    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetRandomLoadingImage()
    {
        image.sprite = backgroundImages[Random.Range(0, backgroundImages.Count)];
    }
}
