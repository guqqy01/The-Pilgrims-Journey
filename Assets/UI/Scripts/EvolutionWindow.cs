using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using UnityEngine.UI;

public class EvolutionWindow : MonoBehaviour
{
    [SerializeField]
    private Image unitImage;

    [SerializeField]
    private EquipmentContainer equipmentContainer;


    [SerializeField]
    private Transform unitPerEvoHolder;
    

    private Tile tile;

    private UnitManager unitManager;


    private void Start()
    {
        unitManager = UnitManager.Instance;
    }

    public void Init(Tile tile)
    {
        unitImage.sprite = tile.Unit.sprite;
        this.tile = tile;
    }

    public void Evolve()
    {
        //tile.Unit = tile.Unit.unitEquipmentRequirements

        equipmentContainer.TryChangeEquipment(null);
        
    }
}
