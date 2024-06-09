using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkMessage.CommandFactory;
using NetworkMessage.CommandsResults;
using NetworkMessage.Communicator;
using NetworkMessage.Exceptions;
using NetworkMessage.Intents;
using RemoteControlWPFClient.WpfLayer.IoC;

namespace RemoteControlWPFClient.BusinessLayer.Services
{
	public class CommandsRecipientService : ISingleton, IDisposable
	{
		private readonly Task task;
		private readonly CancellationTokenSource tokenSource;
		private readonly TcpCryptoClientCommunicator communicator;
		private readonly ICommandFactory factory;
		private readonly ServerConectionService conectionService;
		private readonly Queue<BaseIntent> receivedIntents;

		public CommandsRecipientService(TcpCryptoClientCommunicator communicator, ICommandFactory factory, ServerConectionService conectionService)
		{
			this.communicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			this.conectionService = conectionService ?? throw new ArgumentNullException(nameof(conectionService));
			receivedIntents = new Queue<BaseIntent>();
			tokenSource = new CancellationTokenSource();
			task = new Task(async () => await ActionAsync(communicator, factory));
		}

		public void Start()
		{	
			if (task.Status == TaskStatus.Running || task.Status == TaskStatus.RanToCompletion) return;
			task.Start();	
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
			//Progress<long> progress = new Progress<long>(i=>Debug.WriteLine(i));

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

					await communicator.SendObjectAsync(result, token: token).ConfigureAwait(false);
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
					MessageBox.Show(ex.Message);
					Debug.WriteLine(ex.Message);
					return;
					//await conectionService.ReconnectAsync().ConfigureAwait(false);
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				task?.Dispose();
				tokenSource?.Dispose();
				communicator?.Dispose();
			}
		}
	}
}
