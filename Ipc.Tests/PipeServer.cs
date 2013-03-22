using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FiftyOne.Mobile.Detection.Provider.Interop;

namespace Ipc.Tests
{
	public class PipeServer
	{
        bool running;
        Thread runningThread;
        EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
		private string _pipeName = "testpipe";
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
        }

        public void ProcessClientThread(object o)
        {
			using (var pipeStream = (NamedPipeServerStream)o)
			using (var streamReader = new StreamReader(pipeStream))
			using (var streamWriter = new StreamWriter(pipeStream))
			{
				Console.WriteLine("[Server] Pipe connection established");
				{
					string userAgent;
					//wait for message to arrive from the pipe
					while ((userAgent = streamReader.ReadLine()) != null)
					{
						Console.WriteLine("[Server Rcvd User Agent] {0}: {1}", DateTime.Now, userAgent);

						var isMobile = IsMobilePattern(userAgent);

						streamWriter.AutoFlush = true;
						streamWriter.WriteLine(isMobile);
						Console.WriteLine("[Server] sent message back to client IsMobile:{0}", isMobile);
					}
				}
			}
			Console.WriteLine("Connection lost");
        }

        public void ProcessNextClient()
        {
            try
            {
                var pipeStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 254);
                pipeStream.WaitForConnection();

                //Spawn a new thread for each request and continue waiting
                var t = new Thread(ProcessClientThread);
                t.Start(pipeStream);
            }
            catch
            {//If there are no more avail connections (254 is in use already) then just keep looping until one is avail
            }
        }

		private static bool IsMobileTrie(string userAgent)
		{
			var isMobile = false;

			var trieWrapper = new TrieWrapper(@"C:\websites\online\DeviceRedirectionData\51Degrees.mobi-Lite-2013.01.02.trie.dat");

			var properties = trieWrapper.GetProperties(userAgent);
			var isMobilePropertyList = properties["IsMobile"];
			bool.TryParse(isMobilePropertyList.First(), out isMobile);
			return isMobile;
		}

		private static bool IsMobilePattern(string userAgent)
		{
			var isMobile = false;

			var wrapper = new PatternWrapper(new[] { "IsMobile" });

			var properties = wrapper.GetProperties(userAgent);
			var isMobilePropertyList = properties["IsMobile"];
			bool.TryParse(isMobilePropertyList.First(), out isMobile);
			return isMobile;
		}
	}
}