namespace RemoteControlWPFClient.BusinessLayer.DTO;

public class DeviceStatusesDTO
{
    public float AmountOfRAM { get; set; }

    public float AmountOfOccupiedRAM { get; set; }

    public byte ButteryChargePercent { get; set; }

    public byte PercentageOfCPUUsage { get; set; }
}