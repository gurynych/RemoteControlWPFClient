using System;
using System.Collections.Generic;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.WpfLayer.IoC;

namespace RemoteControlWPFClient.BusinessLayer.Services
{
    public class CurrentUserServices : ISingleton
    {
        public UserDTO CurrentUser { get; private set; }

        public List<DeviceDTO> UserDevices { get; private set; }

        public void Enter(UserDTO u)
        {
            CurrentUser = u ?? throw new ArgumentNullException(nameof(u));
        }

        public void Exit()
        {
            CurrentUser = null;
            UserDevices = null;
        }

        public void SetDevices(List<DeviceDTO> devices)
        {
            UserDevices = devices ?? throw new ArgumentNullException(nameof(devices));
        }
    }
}