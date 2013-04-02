using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipc.Tests
{
	/// <summary>
	/// Encapsulates a stream reader which does not close the underlying stream.
	/// Inspiration from http://stackoverflow.com/a/6784157
	/// </summary>
	public class NonClosingStreamReader : StreamReader
	{
		/// <summary>
		/// Creates a new stream reader object.
		/// </summary>
		/// <param name="stream">The underlying stream to read from.</param>
		/// <param name="encoding">The encoding for the stream.</param>
		public NonClosingStreamReader(Stream stream, Encoding encoding)
			: base(stream, encoding)
		{
		}

		/// <summary>
		/// Creates a new stream reader object using default encoding.
		/// </summary>
		/// <param name="stream">The underlying stream to read from.</param>
		/// <param name="encoding">The encoding for the stream.</param>
		public NonClosingStreamReader(Stream stream)
			: base(stream)
		{
		}

		/// <summary>
		/// Disposes of the stream reader.
		/// </summary>
		/// <param name="disposing">True to dispose managed objects.</param>
		protected override void Dispose(bool disposeManaged)
		{
			// Dispose the stream reader but pass false to the dispose
			// method to stop it from closing the underlying stream
			base.Dispose(false);
		}
	}
}
