using MaterialDesignThemes.Wpf;

namespace RemoteControlWPFClient.BusinessLayer.DTO
{
    public sealed class DeviceDTO
    {
        public int Id { get; set; }

        public string DeviceName { get; set; }

        public string DeviceType { get; set; }

        public string DevicePlatform { get; set; }

        public string DevicePlatformVersion { get; set; }

        public string DeviceManufacturer { get; set; }

        public string DeviceGuid { get; set; }

        public int UserId { get; set; }

        public UserDTO User { get; set; }

        private PackIconKind? deviceTypeIcon;
        public PackIconKind DeviceTypeIcon
        {
            get
            {
                return deviceTypeIcon ??= DeviceType?.ToLower() switch
                {
                    "pc" => PackIconKind.Monitor,
                    "phone" => PackIconKind.Cellphone,
                    _ => PackIconKind.Help
                };
            }
        }

        private PackIconKind? devicePlatformIcon;
        public PackIconKind DevicePlatformIcon
        {
            get
            {
                return devicePlatformIcon ??= DevicePlatform?.ToLower() switch
                {
                    "android" => PackIconKind.Android,
                    "ios" or "macos" or "maccatalyst" => PackIconKind.Apple,
                    "windows" => PackIconKind.MicrosoftWindows,
                    _ => PackIconKind.RhombusSplit
                };
            }
        }

        public bool IsConnected { get; set; }

        public string IsConnectedText => IsConnected ? "подключено" : "не подключено";

        public DeviceDTO()
        {
        }

        public DeviceDTO(string deviceGuid, string deviceName, UserDTO user, string deviceType = null, string devicePlatform = null, string devicePlatformVersion = null, string deviceManufacturer = null)
        {
            User = user;
            DeviceGuid = deviceGuid;
            DeviceName = deviceName;
            DeviceType = deviceType;
            DevicePlatform = devicePlatform;
            DevicePlatformVersion = devicePlatformVersion;
            DeviceManufacturer = deviceManufacturer;
        }
        
        private byte[] GetImageByteArray()
        {
            byte[] bytes = Resources.Icons.ResourceManager.GetObject(DeviceType?.ToLower() ?? "") as byte[];
            if (bytes == null)
            {
                bytes = Resources.Icons.unknown;
            }

            return bytes;
        }
    }
}
