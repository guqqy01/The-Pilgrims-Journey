using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class Tile : MonoBehaviour
{
    public bool locked;

    public Unit unit;

    public TileShape shape;

    public bool Selected { private set; get; } = false;

    public RectTransform rectTransform { private set; get; }
    private Image image;
    public List<Vector2Int> Shape => shape.shape.v2IntList;



    public Color Color
    {
        set
        {
            image.color = value;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void SelectTile(bool select)
    {
        Selected = select;
    }
}
