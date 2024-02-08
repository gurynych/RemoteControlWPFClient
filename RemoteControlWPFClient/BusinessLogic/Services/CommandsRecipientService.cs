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
        private readonly Thread thread;
        private readonly Queue<BaseIntent> receivedIntents;

        public CommandsRecipientService(TcpCryptoClientCommunicator communicator, ICommandFactory factory)
        {                      
            receivedIntents = new Queue<BaseIntent>();
            thread = new Thread(async () => await ActionAsync(communicator, factory))
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
            if (thread.ThreadState == System.Threading.ThreadState.Running)
            {
                thread.Join();
            }
        }

        private async Task ActionAsync(TcpCryptoClientCommunicator communicator, ICommandFactory factory)
        {          
            while (true)
            {
                try
                {                   
                    BaseIntent intent = await communicator.ReceiveAsync().ConfigureAwait(false);
                    if (intent == null)
                    {
                        continue;
                    }

                    receivedIntents.Enqueue(intent);
                    BaseNetworkCommandResult result = await receivedIntents.Dequeue()
                        .CreateCommand(factory)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                    
                    await communicator.SendObjectAsync(result).ConfigureAwait(false);               
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
