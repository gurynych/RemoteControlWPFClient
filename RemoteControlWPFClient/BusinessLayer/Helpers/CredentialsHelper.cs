using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        public static async Task<byte[]> ReadUserTokenFromFileAsync(CancellationToken token = default)
        {
			return await File.ReadAllBytesAsync(userDataPath, token).ConfigureAwait(false);        
        }

        /// <summary>
        /// Асинхронная записть данных пользователя в файл
        /// </summary>
        /// <param name="user">Текущий пользователь</param>               
        /// <exception cref="OperationCanceledException"/>
        public static async Task WriteUserTokenToFileAsync(byte[] userToken, CancellationToken token = default)
        {        
            await File.WriteAllBytesAsync(userDataPath, userToken, token).ConfigureAwait(false);                       
        }

    }
}
