using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using NUnit.Framework;

namespace Ipc.Tests
{
	[TestFixture]
	public class NamedPipes
	{
		[TestCase("Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_2_1 like Mac OS X; da-dk) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8C148 Safari/6533.18.5", "True")]
		[TestCase("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/22.0.1207.1 Safari/537.1", "False")]
		public void NamedPipeServerDetectAgent(string agent, string expectedResult)
		{
			var pipeServer = new PipeServer();

			var serverThread = new Thread(pipeServer.Start);
			serverThread.Start();

			string response = "";

			using (var pipeStream = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut))
			using (var streamReader = new StreamReader(pipeStream))
			using (var streamWriter = new StreamWriter(pipeStream))
			{
				pipeStream.Connect();

				if (pipeStream.IsConnected)
				{
					streamWriter.AutoFlush = true;
					streamWriter.WriteLine(agent);

					response = streamReader.ReadLine();
				}
				else
				{
					Assert.Fail("pipe was not connected");
				}
			}

			Assert.AreEqual(expectedResult, response);
		}
	}
}