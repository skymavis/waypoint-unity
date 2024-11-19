using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkyMavis.Utils
{
    /// <summary> 
    /// This class is intended only for asynchronous send over WebSocket.
    /// Synchronous operation is not supported.
    /// </summary>
    internal class WebSocketMessageStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        private readonly WebSocket _ws;
        private long _length;

        public WebSocketMessageStream(WebSocket ws)
        {
            _ws = ws;
        }

        public override void Flush() { }

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _ws.SendAsync(new ArraySegment<byte>(Array.Empty<byte>()), WebSocketMessageType.Text, true, cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _ws.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Text, false, cancellationToken);
            _length += count;
        }
    }
}
