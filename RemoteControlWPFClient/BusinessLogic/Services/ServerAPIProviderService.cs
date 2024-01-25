﻿using NetworkMessage.CommandFactory;
using NetworkMessage.CommandsResults;
using NetworkMessage.Windows;
using Newtonsoft.Json;
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
        public const string ServerAddress = "192.168.232.254";
        private const string AuthtorizeAPIUri = $"http://{ServerAddress}:5170/api/AuthentificationAPI/AuthorizeFromDevice";
        private const string RegisterAPIUri = $"http://{ServerAddress}:5170/api/AuthentificationAPI/RegisterFromDevice";
        private const string GetNestedFilesInfoInDirectoryUri = $"http://{ServerAddress}:5170/api/DeviceAPI/GetNestedFilesInfoInDirectory";
        private const string GetConnectedDeviceUri = $"http://{ServerAddress}:5170/api/DeviceAPI/GetConnectedDevice";
        private const string DownloadFileUri = $"http://{ServerAddress}:5170/api/DeviceAPI/DownloadFile";
        private readonly ICommandFactory factory;

        public ServerAPIProviderService(ICommandFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Асинхронный метод для авторизации пользователя через API сервера
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