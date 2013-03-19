using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiftyOne.Mobile.Detection.Provider.Interop;

namespace Ipc.Tests
{
	public class PipeServer
	{
		public void Start()
		{
			//Create a named pipe
			using (var pipeStream = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 50))
			using (var streamReader = new StreamReader(pipeStream))
			using (var streamWriter = new StreamWriter(pipeStream))
			{
				//wait for a connection from another process
				pipeStream.WaitForConnection();
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
