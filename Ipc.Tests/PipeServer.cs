using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FiftyOne.Mobile.Detection.Provider.Interop;
using NLog;

namespace Ipc.Tests
{
	public class PipeServer
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		bool running;
		Thread runningThread;
		EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

		private string _pipeName = "testpipe";
		private static TrieWrapper _trieWrapper;
		private PatternWrapper _patternWrapper;

		protected TrieWrapper TrieWrapper
		{
			get
			{
				return
					_trieWrapper ?? (_trieWrapper =
					new TrieWrapper(@"C:\websites\online\DeviceRedirectionData\51Degrees.mobi-Lite-2013.01.02.trie.dat"));
				;
			}
		}

		public PatternWrapper PatternWrapper
		{
			get { return _patternWrapper ?? (_patternWrapper = new PatternWrapper("IsMobile")); }
		}

		public string PipeName
		{
			get { return _pipeName; }
			set { _pipeName = value; }
		}

		void ServerLoop()
		{
			while (running)
			{
				ProcessNextClient();
			}

			terminateHandle.Set();
		}

		public void Run()
		{
			running = true;
			runningThread = new Thread(ServerLoop);
			runningThread.Start();
		}

		public void Stop()
		{
			running = false;
			terminateHandle.WaitOne();
			runningThread.Abort();
		}

		public void ProcessClientThread(object o)
		{
			var pipeStream = (NamedPipeServerStream)o;

			using (var streamReader = new NonClosingStreamReader(pipeStream))
			using (var streamWriter = new NonClosingStreamWriter(pipeStream))
			{
				_logger.Trace("[Server] Pipe connection established");
				{
					string userAgent;
					//wait for message to arrive from the pipe
					while ((userAgent = streamReader.ReadLine()) != null)
					{
						_logger.Trace("[Server] Rcvd User Agent: {1}", DateTime.Now, userAgent);

						var isMobile = IsMobilePattern(userAgent);

						streamWriter.AutoFlush = true;
						streamWriter.WriteLine(isMobile);
						_logger.Trace("[Server] sent message back to client IsMobile:{0}", isMobile);
					}
				}
			}

			pipeStream.Dispose();
			_logger.Trace("[Server] Pipe connection ended");
		}

		public void ProcessNextClient()
		{
			try
			{
				var pipeStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 254);
				pipeStream.WaitForConnection();
				_logger.Trace("[Server] NamedPipeServerStream created and waiting for connection");

				//Spawn a new thread for each request and continue waiting
				ThreadPool.QueueUserWorkItem(ProcessClientThread, pipeStream);
				_logger.Trace("[Server] Thread started for NamedPipeServerStream");
			}
			catch
			{//If there are no more avail connections (254 is in use already) then just keep looping until one is avail
			}
		}

		/// <summary>
		/// Uses a faster but more memory intensive version (vastly so) of the agent matcher. Also needs a specific version of the dat file
		/// </summary>
		/// <param name="userAgent"></param>
		/// <returns></returns>
		private bool IsMobileTrie(string userAgent)
		{
			bool isMobile;
			var properties = TrieWrapper.GetProperties(userAgent);
			var isMobilePropertyList = properties["IsMobile"];
			bool.TryParse(isMobilePropertyList.First(), out isMobile);
			return isMobile;
		}

		/// <summary>
		/// Uses a fast pattern based match. The data is compiled within the code, so no need for the dat file... 
		/// it does however mean recompiling any time we want to update the data
		/// </summary>
		/// <param name="userAgent"></param>
		/// <returns></returns>
		private bool IsMobilePattern(string userAgent)
		{
			bool isMobile;
			
			var properties = PatternWrapper.GetProperties(userAgent);
			var isMobilePropertyList = properties["IsMobile"];
			bool.TryParse(isMobilePropertyList.First(), out isMobile);

			return isMobile;
		}
	}
}