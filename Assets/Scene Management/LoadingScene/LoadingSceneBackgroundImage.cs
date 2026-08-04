using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class LoadingSceneBackgroundImage : Singleton<LoadingSceneBackgroundImage>
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private List<Sprite> backgroundImages;

    public void SetRandomLoadingImage()
    {
        image.sprite = backgroundImages[Random.Range(0, backgroundImages.Count)];
    }
}