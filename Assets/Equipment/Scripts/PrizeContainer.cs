public class PrizeContainer : EquipmentContainer
{
    public EquipmentSO AllowedEquipment;

    private void Awake()
    {
        onEquipmentChange = OnEquipmentChanged;
    }

    private void OnEquipmentChanged(EquipmentSO fromEquipment, EquipmentSO toEquipment)
    {
        if (toEquipment == null) AllowedEquipment = fromEquipment;
    }
}