namespace Week2.Models
{
    public enum EquipmentType
    {
        ElectricalPanel,
        Transformer,
        Generator,
        UPS,
        SwitchGear,
        MotorControlCenter
    }

    public class EquipmentRequest
    {
        public EquipmentType Type { get; set; }
        public string Name { get; set; }
    }

    public class EquipmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ElementId { get; set; }
    }
}