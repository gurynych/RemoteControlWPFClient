using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.BusinessLogic.Services
{
	public class CurrentUserServices:ISingleton
	{
		public User CurrentUser { get; private set; }

		public List<Device> UserDevices { get; private set; }

		public void Enter(User u)
		{
			CurrentUser = u;
		}

		public void Exit()
		{
			CurrentUser = null;
			UserDevices = null;
		}

		public void SetDevices(List<Device> devices)
		{
			UserDevices = devices;
		}
	}
}
