using Newtonsoft.Json;
using RemoteControlWPFClient.MVVM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.BusinessLogic.Helpers
{
    public static class FileHelper
    {
        private static readonly string userDataPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "user.txt");

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
