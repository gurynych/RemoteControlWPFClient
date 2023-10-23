﻿using NetworkMessage.Commands;
using NetworkMessage.CommandsResaults;
using Newtonsoft.Json;
using RemoteControlWPFClient.MVVM.AttachedProperties;
using RemoteControlWPFClient.MVVM.Command;
using RemoteControlWPFClient.MVVM.IoC;
using RemoteControlWPFClient.MVVM.IoC.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public class MainViewModel : BaseViewModel, ITransient
    {
        private const string AuthAPIUri = "http://localhost:5170/api/Authentification/AuthorizeFromDevice";
        private readonly string userDataPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "user.txt");
        private readonly Client client;

        public string Email { get; set; }

        public string Password { get; set; }

        public MainViewModel(Client client)
        {
            this.client = client;
        }

        public ICommand MainWindowLoadedCommand => new AwaitableCommand(ConnectToServerSocket);

        public ILoadedAction MainWindowLoadedAction => new DelegateLoadedAction(ConnectToServerSocket);

        private async Task ConnectToServerSocket()
        {
            CancellationTokenSource tokenFactory = new CancellationTokenSource(5000);
            try
            {
                User userFromFile = await ReadUserFromFile(tokenFactory.Token);
                NameValueCollection parameters = await GetAuthParametersAsync(userFromFile);
                byte[] serverPublicKey = await GetServerPublicKeyFromAPI(userFromFile, parameters);
                CancellationTokenSource tokenSource = new CancellationTokenSource(10000);
                if (await client.ConnectAsync())
                {
                    client.SetExternalPublicKey(serverPublicKey);
                    client.Handshake(tokenSource.Token);                    
                }
            }
            catch (OperationCanceledException)
            {
                //logger.LogError();
                MessageBox.Show("Время подключения к серверу истекло");
            }
            catch (Exception)
            {
                //logger.LogError();
            }

            //bool connected = false; //await tcpClient.ConnectAsync(cts.Token);
            //_ = await tcpClient.ReceiveAsync();
            //using var webClient = new WebClient();
            /*if (connected)
            {
                //await tcpClient.SendAsync(new MacAddressCommand());
                //считывание файла, какого-нибудь
                Current.Shutdown();
                StartupUri = new Uri("/RemoteControlWPFClient;component/Window1.xaml", UriKind.Relative);
            }
            else
            {
                Current.Shutdown();
                StartupUri = new Uri("/RemoteControlWPFClient;component/MainWindow.xaml", UriKind.Relative);
            }*/
        }

        private async Task<byte[]> GetServerPublicKeyFromAPI(User user, NameValueCollection parameters)
        {
            try
            {
                using WebClient webClient = new WebClient();
                byte[] bytes = await webClient.UploadValuesTaskAsync(new Uri(AuthAPIUri), parameters);
                string base64 = Encoding.UTF8.GetString(bytes);
                base64 = base64[1..^1];
                return Convert.FromBase64String(base64);
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    MessageBox.Show("Время подключения к серверу истекло");
                }
                else if (webEx.Response is HttpWebResponse response)
                {
                    _ = response.StatusCode;
                }
                else if (webEx.Response == null)
                {

                }
                else
                {
                    //logger.LogError();
                    MessageBox.Show("Ошибка подключения к серверу");
                }

                return default;
            }
        }

        private async Task<NameValueCollection> GetAuthParametersAsync(User user)
        {
            HwidCommand hwidCommand = new HwidCommand();
            HwidResult hwidResult = await hwidCommand.Do() as HwidResult;
            /*NameValueCollection parameters = new NameValueCollection
            {
                { "email", user.Email },
                { "password", user.Password },
                { "hwidHash", hwidResult.Hwid }
            };*/

            var parameters = new NameValueCollection
            {
                { "email", "test" },
                { "password", "test" },
                { "hwidHash", hwidResult.Hwid }
            };

            return parameters;
        }

        private async Task<User> ReadUserFromFile(CancellationToken token)
        {
            Memory<byte> bytes = new Memory<byte>();
            try
            {
                using (FileStream stream = File.OpenRead(userDataPath))
                {
                    await stream.ReadAsync(bytes, token);
                }

                string userJson = Encoding.UTF8.GetString(bytes.ToArray());
                User user = JsonConvert.DeserializeObject<User>(userJson);
                return user;
            }
            catch (OperationCanceledException ex) { throw ex; }
            catch (Exception) { return default; }
        }

        private async Task WriteUserToFile(User user, CancellationToken token)
        {
            string userJson = JsonConvert.SerializeObject(user);
            byte[] bytes = Encoding.UTF8.GetBytes(userJson);
            try
            {
                using FileStream stream = File.OpenWrite(userDataPath);
                await stream.WriteAsync(bytes, token);
            }
            catch (OperationCanceledException ex) { throw ex; }
            catch (Exception) { }
        }
    }

    record class User(string Login, string Email, string Password);
}