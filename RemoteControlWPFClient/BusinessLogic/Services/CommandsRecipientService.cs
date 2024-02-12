using NetworkMessage.CommandFactory;
using NetworkMessage.CommandsResults;
using NetworkMessage.Communicator;
using NetworkMessage.Exceptions;
using NetworkMessage.Intents;
using RemoteControlWPFClient.MVVM.IoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteControlWPFClient.BusinessLogic.Services
{
	public class CommandsRecipientService : ISingleton
	{
		private readonly Thread thread;
		private readonly Queue<BaseIntent> receivedIntents;
		private readonly CancellationTokenSource tokenSource;

		public CommandsRecipientService(TcpCryptoClientCommunicator communicator, ICommandFactory factory)
		{
			receivedIntents = new Queue<BaseIntent>();
			tokenSource = new CancellationTokenSource();
			thread = new Thread(async () => await ActionAsync(communicator, factory))
			{ IsBackground = true };
		}

		public void Start()
		{	
			thread.Start();	
		}

		public void Stop()
		{
			tokenSource.Cancel();		
		}

		public void Restart()
		{
			Stop();
			Start();
		}

		private async Task ActionAsync(TcpCryptoClientCommunicator communicator, ICommandFactory factory, CancellationToken token = default)
		{
			Progress<long> progress = new Progress<long>(i=>Debug.WriteLine(i));

			while (!token.IsCancellationRequested)
			{
				try
				{
					BaseIntent intent = await communicator.ReceiveAsync(token: token).ConfigureAwait(false);
					token.ThrowIfCancellationRequested();
					if (intent == null)
					{
						continue;
					}

					receivedIntents.Enqueue(intent);
					BaseNetworkCommandResult result = await receivedIntents.Dequeue()
						.CreateCommand(factory)
						.ExecuteAsync()
						.ConfigureAwait(false);

					await communicator.SendObjectAsync(result, progress, token: token).ConfigureAwait(false);
				}
				catch (DeviceNotConnectedException notConnEx)
				{
					MessageBox.Show("Устройство отключено");
					Debug.WriteLine(notConnEx.Message);
					return;
				}
				catch (OperationCanceledException operationCanncelEx)
				{
					MessageBox.Show("Время ожидания превышено");
					Debug.WriteLine(operationCanncelEx.Message);
					return;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}
	}
}
