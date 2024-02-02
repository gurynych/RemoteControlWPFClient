using NetworkMessage.CommandFactory;
using NetworkMessage.CommandsResults;
using NetworkMessage.Communicator;
using NetworkMessage.Intents;
using RemoteControlWPFClient.MVVM.IoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.BusinessLogic.Services
{
    public class CommandsRecipientService : ISingleton
    {
        private Thread thread;

        public CommandsRecipientService(TcpCryptoClientCommunicator communicator, ICommandFactory factory)
        {
            thread = new Thread(async () =>
            {
                Progress<long> receiveProgress = new Progress<long>((i) => Debug.WriteLine($"Receive: {i}"));
                Progress<long> sendProgress = new Progress<long>((i) => Debug.WriteLine($"Send: {i}"));               
                while (true)
                {
                    try
                    {
                        //ActualAction = "Получение намерения\n";
                        BaseIntent intent = await communicator.ReceiveIntentAsync(receiveProgress).ConfigureAwait(false);
                        if (intent == null)
                        {
                            continue;
                        }

                        //ActualAction += $"Полученное намерение: {intent.IntentType}\n";
                        //ActualAction += $"Выполнение команды\n";
                        BaseNetworkCommandResult result = await intent.CreateCommand(factory).ExecuteAsync().ConfigureAwait(false);
                        //ActualAction += "Отправка результа команды\n";
                        await communicator.SendObjectAsync(result, sendProgress).ConfigureAwait(false);
                        //ActualAction += "Результат отправлен";
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            })
            { IsBackground = true};
        }

        public void Start()
        {
            if (thread.ThreadState == System.Threading.ThreadState.Unstarted) 
            {
                thread.Start();
            }
        }

        public void Stop()
        {
            if (thread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                thread.Join();
            }
        }
    }
}
