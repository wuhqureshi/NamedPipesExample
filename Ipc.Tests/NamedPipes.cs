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
		private PipeServer _pipeServer;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_pipeServer = new PipeServer();
			_pipeServer.Run();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_pipeServer.Stop();
		}

		[TestCase("Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_2_1 like Mac OS X; da-dk) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8C148 Safari/6533.18.5", "True")]
		[TestCase("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/22.0.1207.1 Safari/537.1", "False")]
		public void NamedPipeServerDetectAgent(string agent, string expectedResult)
		{
			string response = "";
			//Thread.Sleep(2000);
			using (var pipeStream = new NamedPipeClientStream(".", _pipeServer.PipeName, PipeDirection.InOut))
			using (var streamReader = new NonClosingStreamReader(pipeStream))
			using (var streamWriter = new NonClosingStreamWriter(pipeStream))
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