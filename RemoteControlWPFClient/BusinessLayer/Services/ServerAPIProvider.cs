using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NetworkMessage.CommandFactory;
using NetworkMessage.CommandsResults.ConcreteCommandResults;
using NetworkMessage.Models;
using Newtonsoft.Json;
using RemoteControlWPFClient.BusinessLayer.DTO;
using RemoteControlWPFClient.WpfLayer.IoC;

namespace RemoteControlWPFClient.BusinessLayer.Services
{
    public class ServerAPIProvider : ITransient
    {
        public const string ServerAddress = "192.168.1.162";
        public const string ServerPort = "5000";
        private const string AuthtorizeAPIUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/AuthorizeFromDevice";
        private const string RegisterAPIUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/RegisterFromDevice";
        private const string AuthorizeWithTokenUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/AuthorizeWithToken";
		private const string GetNestedFilesInfoInDirectoryUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/GetNestedFilesInfoInDirectory";
        private const string GetConnectedDeviceUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/GetConnectedDevices";
		private const string GetUserByTokenUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/GetUserByToken";
		private const string DownloadFileUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/DownloadFile";
		private const string GetDeviceStatusesUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/GetDeviceStatuses";
		private const string GetScreenshotUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/GetScreenshot";
        private readonly ICommandFactory factory;	

        public ServerAPIProvider(ICommandFactory factory)
        {
            this.factory = factory;
        }

		/// <summary>
		/// Асинхронный метод для авторизации пользователя с API сервера
		/// </summary>
		/// <param name="user"></param>
		/// <returns>Открытый ключ сервера</returns>
		/// <exception cref="WebException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="FormatException"/>
		/// <exception cref="SEHException"/>
		public async Task<byte[]> UserAuthorizationUseAPIAsync(UserDTO user, CancellationToken token = default)
		{
			using HttpClient client = new HttpClient();
			try
			{
				FormUrlEncodedContent parameters = await GetAuthParametersAsync(user, token);
				HttpResponseMessage response = await client.PostAsync(AuthtorizeAPIUri, parameters, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					return Convert.FromBase64String(responseContent[1..^1]);
				}

				Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
				return default;
			}
			catch (Exception ex)
			{
				return default;
			}
		}
		
		public async Task<List<DeviceDTO>> GetConnectedDeviceAsync(UserDTO user, CancellationToken token = default)
		{
			using HttpClient client = new HttpClient();
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(user.AuthToken));
				string requestUri = GetConnectedDeviceUri +
					$"?userToken={encodedToken}";

				HttpResponseMessage response = await client.GetAsync(requestUri, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					return JsonConvert.DeserializeObject<List<DeviceDTO>>(responseContent);
				}

				Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
				return default;
			}
			catch (Exception ex)
			{
				return default;
			}
		}
		
		public async Task GetScreenshot(UserDTO user, DeviceDTO device, Stream streamToWrite, CancellationToken token = default)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (device == null) throw new ArgumentNullException(nameof(device));
			if (streamToWrite == null) throw new ArgumentNullException(nameof(streamToWrite));

			using HttpClient client = new HttpClient();
			HttpResponseMessage response = null;
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(user.AuthToken));
				string requestUri = GetScreenshotUri +
				                    $"?userToken={encodedToken}" +
				                    $"&deviceId={device.Id}";

				response = await client.GetAsync(requestUri, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					byte[] bytes = Convert.FromBase64String(responseContent[1..^1]);
					await streamToWrite.WriteAsync(bytes, token);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Debug.WriteLine("Ошибка при выполнении запроса: " + response?.StatusCode);
				throw;
			}
		}

		public async Task<DeviceStatusesDTO> GetDeviceStatuses(UserDTO user, DeviceDTO device, CancellationToken token = default)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (device == null) throw new ArgumentNullException(nameof(device));
			
			using HttpClient client = new HttpClient();
			HttpResponseMessage response = null;
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(user.AuthToken));
				string requestUri = GetDeviceStatusesUri +
				                    $"?userToken={encodedToken}" +
				                    $"&deviceId={device.Id}";

				response = await client.GetAsync(requestUri, token);
				if (response.IsSuccessStatusCode)
				{
					return JsonConvert.DeserializeObject<DeviceStatusesDTO>(await response.Content.ReadAsStringAsync(token).ConfigureAwait(false));
				}

				throw new InvalidOperationException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Debug.WriteLine("Ошибка при выполнении запроса: " + response?.StatusCode);
				throw;
			}	
		}
		
		public async IAsyncEnumerable<FileInfoDTO> GetNestedFilesInfoInDirectoryAsync(UserDTO user, DeviceDTO device, string path, CancellationToken token = default)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (device == null) throw new ArgumentNullException(nameof(device));

			NestedFileInfoInDirectories allFiles = null;
			using HttpClient client = new HttpClient();
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(user.AuthToken));
				string requestUri = GetNestedFilesInfoInDirectoryUri +
				                    $"?userToken={encodedToken}&" +
				                    $"deviceId={device.Id}&" +
				                    $"&path={path}";

				HttpResponseMessage response = await client.GetAsync(requestUri, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					allFiles = JsonConvert.DeserializeObject<NestedFileInfoInDirectories>(responseContent);
				}
				else
				{
					Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			
			if (allFiles?.NestedDirectoriesInfo != null)
			{
				foreach (MyDirectoryInfo dir in allFiles.NestedDirectoriesInfo)
				{
					yield return new FileInfoDTO(dir);
				}
			}

			if (allFiles?.NestedFilesInfo != null)
			{
				foreach (MyFileInfo file in allFiles.NestedFilesInfo)
				{
					yield return new FileInfoDTO(file);
				}
			}
		}
		
		public async Task DownloadFileAsync(UserDTO user, DeviceDTO device, string path, Stream streamToWrite, CancellationToken token = default)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (device == null) throw new ArgumentNullException(nameof(device));
			
			using HttpClient client = new HttpClient();
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(user.AuthToken));
				string requestUri = DownloadFileUri +
				                    $"?userToken={encodedToken}&" +
				                    $"deviceId={device.Id}&" +
				                    $"&path={path}";

				HttpResponseMessage response = await client.GetAsync(requestUri, token);
				if (response.IsSuccessStatusCode)
				{
					Stream stream = await response.Content.ReadAsStreamAsync(token);
					await stream.CopyToAsync(streamToWrite);
				}
				else
				{
					string errorResponse = await response.Content.ReadAsStringAsync(token);
					Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}
		
		public async Task<UserDTO> GetUserByToken(byte[] usertoken, CancellationToken token = default)
		{
			if (usertoken == null) throw new ArgumentNullException(nameof(usertoken));
			if (usertoken.Length == 0) throw new ArgumentException("Value cannot be an empty collection", nameof(usertoken));
			
			using HttpClient client = new HttpClient();
			HttpResponseMessage response = null;
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(usertoken));
				string requestUri = GetUserByTokenUri +
					$"?userToken={encodedToken}";

				 response = await client.GetAsync(requestUri, token);

				//ByteArrayContent bytesContent = new ByteArrayContent(userToken);
				//HttpResponseMessage response = await client.PostAsync(GetConnectedDeviceUri, bytesContent, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					UserDTO u = JsonConvert.DeserializeObject<UserDTO>(responseContent);
					u.AuthToken = usertoken;
					return JsonConvert.DeserializeObject<UserDTO>(responseContent);
				}
				
				throw new InvalidOperationException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Debug.WriteLine("Ошибка при выполнении запроса: " + response?.StatusCode);
				throw;
			}
		}
		/// <summary>
		/// Асинхронный метод для авторизации пользователя через токен
		/// </summary>
		/// <param name="user"></param>
		/// <returns>Открытый ключ сервера</returns>
		/// <exception cref="WebException"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="FormatException"/>
		/// <exception cref="SEHException"/>
		public async Task<byte[]> UserAuthorizationWithTokenUseAPIAsync(byte[] userToken, CancellationToken token = default)
        {
            using HttpClient client = new HttpClient();
            try
            {
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(userToken));
				FormUrlEncodedContent parameters = await GetAuthWithTokenParametersAsync(encodedToken, token);
                HttpResponseMessage response = await client.PostAsync(AuthorizeWithTokenUri, parameters, token);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync(token);
                    return Convert.FromBase64String(responseContent[1..^1]);
                }

                Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        /// <summary>
        /// Асинхронный метод для регистрации пользователя через API сервера
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Открытый ключ сервера</returns>
        /// <exception cref="WebException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>      
        /// <exception cref="SEHException"/>
        public async Task<byte[]> UserRegistrationUseAPIAsync(UserDTO user, CancellationToken token = default)
        {
            using HttpClient client = new HttpClient();
            try
            {
                FormUrlEncodedContent parameters = await GetRegParametersAsync(user, token);
                HttpResponseMessage response = await client.PostAsync(RegisterAPIUri, parameters, token);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync(token);
                    return Convert.FromBase64String(responseContent[1..^1]);
                }

                Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

		/// <summary>
		/// Асинхронный метод для получения параметров пользователя для авторизации через токен
		/// </summary>        
		/// <returns>FormUrlEncodedContent с параметрами пользователя</returns>
		/// <exception cref="SEHException"/>
		private async Task<FormUrlEncodedContent> GetAuthWithTokenParametersAsync(string userToken, CancellationToken token = default)
		{
			DeviceGuidResult guidResult = await factory.CreateGuidCommand().ExecuteAsync(token).ConfigureAwait(false) as DeviceGuidResult;
			var parameters = new Dictionary<string, string>
			{
				{ "userToken", userToken },
				{ "deviceGuid", guidResult.Guid },
				{ "deviceName", Environment.MachineName },
				{ "deviceType", "PC" },
				{ "devicePlatform", "Windows" },
				{ "devicePlatformVersion", "11" },
				{ "deviceManufacturer", "" }
			};

			return new FormUrlEncodedContent(parameters);
		}

		/// <summary>
		/// Асинхронный метод для получения параметров пользователя для авторизации
		/// </summary>        
		/// <returns>FormUrlEncodedContent с параметрами пользователя</returns>
		/// <exception cref="SEHException"/>
		private async Task<FormUrlEncodedContent> GetAuthParametersAsync(UserDTO user, CancellationToken token = default)
        {
            DeviceGuidResult guidResult = await factory.CreateGuidCommand().ExecuteAsync(token).ConfigureAwait(false) as DeviceGuidResult;
            var parameters = new Dictionary<string, string>
            {
                { "email", user.Email },
                { "password", user.Password },
                { "deviceGuid", guidResult.Guid },
                { "deviceName", Environment.MachineName },
                { "deviceType", "PC" },
                { "devicePlatform", "Windows" },
                { "devicePlatformVersion", "11" },
                { "deviceManufacturer", "" }
            };

            return new FormUrlEncodedContent(parameters);
        }

        /// <summary>
        /// Асинхронный метод для получения параметров пользователя для регистрации
        /// </summary>
        /// <param name="user"></param>
        /// <returns>NameValuecollection с параметрами нового пользователя</returns>
        /// <exception cref="SEHException"/>
        private async Task<FormUrlEncodedContent> GetRegParametersAsync(UserDTO user, CancellationToken token = default)
        {
            DeviceGuidResult guidResult = await factory.CreateGuidCommand().ExecuteAsync(token).ConfigureAwait(false) as DeviceGuidResult;
            var parameters = new Dictionary<string, string>
            {
                { "login",user.Login },
                { "email", user.Email },
                { "password", user.Password },
                { "deviceGuid", guidResult.Guid },
                { "deviceName", Environment.MachineName },
                { "deviceType", "PC" },
                { "devicePlatform", "Windows" },
                { "devicePlatformVersion", "11" },
                { "deviceManufacturer", "" }
            };

            return new FormUrlEncodedContent(parameters);
        }
    }
    
	internal sealed class NestedFileInfoInDirectories
    {
	    public List<MyFileInfo> NestedFilesInfo { get; set; }
	    
	    public List<MyDirectoryInfo> NestedDirectoriesInfo { get; set; }
    }
}