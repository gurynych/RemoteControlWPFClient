using NetworkMessage.Commands;
using NetworkMessage.CommandsResaults;
using RemoteControlWPFClient.MVVM.Models;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.BusinessLogic.ServerAPIProvider
{
    public static class ServerAPIProvider
    {
        private const string AuthtorizeAPIUri = "http://localhost:5170/api/AuthentificationAPI/AuthorizeFromDevice";
        private const string RegisterAPIUri = "http://localhost:5170/api/AuthentificationAPI/RegisterFromDevice";

        /// <summary>
        /// Асинхронный метод для авторизации пользователя через API сервера
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Открытый ключ сервера</returns>
        /// <exception cref="WebException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="SEHException"/>
        public static async Task<byte[]> UserAuthorizationUseAPIAsync(User user)
        {
            using WebClient webClient = new WebClient();
            NameValueCollection parameters = await GetAuthParametersAsync(user);
            byte[] bytes = await webClient.UploadValuesTaskAsync(new Uri(AuthtorizeAPIUri), parameters);
            string base64 = Encoding.UTF8.GetString(bytes);
            base64 = base64[1..^1];
            return Convert.FromBase64String(base64);
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
        public static async Task<byte[]> UserRegistrationUseAPIAsync(User user)
        {
            using WebClient webClient = new WebClient();
            NameValueCollection parameters = await GetRegParametersAsync(user);
            byte[] bytes = await webClient.UploadValuesTaskAsync(new Uri(RegisterAPIUri), parameters);
            string base64 = Encoding.UTF8.GetString(bytes);
            base64 = base64[1..^1];
            return Convert.FromBase64String(base64);           
        }

        /// <summary>
        /// Асинхронный метод для получения параметров пользователя для авторизации
        /// </summary>
        /// <param name="user"></param>
        /// <returns>NameValuecollection с параметрами пользователя</returns>
        /// <exception cref="SEHException"/>
        private static async Task<NameValueCollection> GetAuthParametersAsync(User user)
        {
            HwidCommand hwidCommand = new HwidCommand();
            //HwidResult hwidResult = await hwidCommand.Do() as HwidResult;
            /*NameValueCollection parameters = new NameValueCollection
            {
                { "email", user.Email },
                { "password", user.Password },
                { "hwidHash", hwidResult.Hwid }
            };*/

            NameValueCollection parameters = new NameValueCollection
            {                
                { "email", user.Email },
                { "password", user.Password },
                { "hwidHash", "A64462AFAA94750EEB90B2603B7A4067AFD8A791" }
            };

            return parameters;           
        }

        /// <summary>
        /// Асинхронный метод для получения параметров пользователя для регистрации
        /// </summary>
        /// <param name="user"></param>
        /// <returns>NameValuecollection с параметрами нового пользователя</returns>
        /// <exception cref="System.Runtime.InteropServices.SEHException"/>
        private static async Task<NameValueCollection> GetRegParametersAsync(User user)
        {
            HwidCommand hwidCommand = new HwidCommand();
            /*HwidResult hwidResult = await hwidCommand.Do() as HwidResult;
            NameValueCollection parameters = new NameValueCollection
            {
                { "login",user.Login },
                { "email", user.Email },
                { "password", user.Password },
                { "hwidHash", hwidResult.Hwid }
            };*/

            NameValueCollection parameters = new NameValueCollection
            {
                { "login", user.Login },
                { "email", user.Email },
                { "password", user.Password },
                { "hwidHash", "A64462AFAA94750EEB90B2603B7A4067AFD8A791" }
            };

            return parameters;
        }


    }
}
