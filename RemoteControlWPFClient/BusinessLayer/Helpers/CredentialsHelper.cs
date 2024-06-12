using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using RemoteControlWPFClient.BusinessLayer.DTO;

namespace RemoteControlWPFClient.BusinessLayer.Helpers
{
    public static class CredentialsHelper
    {
        private static readonly string PathToAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), (Application.Current as App)!.ApplicationName);

        private static readonly string userDataPath =
            Path.Combine(PathToAppData, "user.txt");

        static CredentialsHelper()
        {
            Directory.CreateDirectory(PathToAppData);
        }
        
        /// <summary>
        /// Асинхронное чтение данных пользователя из файла
        /// </summary>                
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="OperationCanceledException"/>        
        public static async Task<UserDTO> ReadUserTokenFromFileAsync(CancellationToken token = default)
        {
			return JsonConvert.DeserializeObject<UserDTO>((await File.ReadAllTextAsync(userDataPath, token).ConfigureAwait(false)));        
        }

        /// <summary>
        /// Асинхронная записть данных пользователя в файл
        /// </summary>
        /// <param name="user">Текущий пользователь</param>               
        /// <exception cref="OperationCanceledException"/>
        public static async Task WriteUserToFileAsync(UserDTO user, CancellationToken token = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            await File.WriteAllTextAsync(userDataPath, JsonConvert.SerializeObject(user), token).ConfigureAwait(false);
        }

    }
}
