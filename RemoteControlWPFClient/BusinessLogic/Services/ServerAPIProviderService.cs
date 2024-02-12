using NetworkMessage.CommandFactory;
using NetworkMessage.CommandsResults;
using NetworkMessage.CommandsResults.ConcreteCommandResults;
using NetworkMessage.Windows;
using Newtonsoft.Json;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


namespace RemoteControlWPFClient.BusinessLogic.Services
{
    public class ServerAPIProviderService : ITransient
    {
        public const string ServerAddress = "192.168.60.254";
        public const string ServerPort = "5000";
        private const string AuthtorizeAPIUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/AuthorizeFromDevice";
        private const string RegisterAPIUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/RegisterFromDevice";
        private const string AuthorizeWithTokenUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/AuthorizeWithToken";
		private const string GetNestedFilesInfoInDirectoryUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/GetNestedFilesInfoInDirectory";
        private const string GetConnectedDeviceUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/GetConnectedDevice";
		private const string GetUserByTokenUri = $"http://{ServerAddress}:{ServerPort}/api/AuthentificationAPI/GetUserByToken";
		private const string DownloadFileUri = $"http://{ServerAddress}:{ServerPort}/api/DeviceAPI/DownloadFile";
        private readonly ICommandFactory factory;	

        public ServerAPIProviderService(ICommandFactory factory)
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
		public async Task<byte[]> UserAuthorizationUseAPIAsync(User user, CancellationToken token = default)
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
		public async Task<List<Device>> GetConnectedDeviceAsync(User user, CancellationToken token = default)
		{
			using HttpClient client = new HttpClient();
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(user.AuthToken));
				string requestUri = GetConnectedDeviceUri +
					$"?userToken={encodedToken}";

				HttpResponseMessage response = await client.GetAsync(requestUri, token);

				//ByteArrayContent bytesContent = new ByteArrayContent(userToken);
				//HttpResponseMessage response = await client.PostAsync(GetConnectedDeviceUri, bytesContent, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					return JsonConvert.DeserializeObject<List<Device>>(responseContent);
				}

				Debug.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
				return default;
			}
			catch (Exception ex)
			{
				return default;
			}
		}
		public async Task<User> GetUserByToken(byte[] usertoken, CancellationToken token = default)
		{
			using HttpClient client = new HttpClient();
			try
			{
				string encodedToken = WebUtility.UrlEncode(Convert.ToBase64String(usertoken));
				string requestUri = GetUserByTokenUri +
					$"?userToken={encodedToken}";

				HttpResponseMessage response = await client.GetAsync(requestUri, token);

				//ByteArrayContent bytesContent = new ByteArrayContent(userToken);
				//HttpResponseMessage response = await client.PostAsync(GetConnectedDeviceUri, bytesContent, token);
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync(token);
					User u = JsonConvert.DeserializeObject<User>(responseContent);
					return JsonConvert.DeserializeObject<User>(responseContent);
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
        public async Task<byte[]> UserRegistrationUseAPIAsync(User user, CancellationToken token = default)
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
		private async Task<FormUrlEncodedContent> GetAuthParametersAsync(User user, CancellationToken token = default)
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
        private async Task<FormUrlEncodedContent> GetRegParametersAsync(User user, CancellationToken token = default)
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
}