using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;

public class LocalMapPlayButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        if(TilePlayerManager.Instance != null)
            TilePlayerManager.Instance.Play();
    }
}
