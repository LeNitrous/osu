// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online.Broadcasts
{
    public sealed class WebSocketServer
    {
        public event Action<int, IWebSocketConnection>? OnConnectionStart;
        public event Action<int, IWebSocketConnection>? OnConnectionClose;

        private int nextClientId = -1;
        private readonly HttpListener http;
        private readonly ConcurrentDictionary<int, Connection> connections = new ConcurrentDictionary<int, Connection>();

        public WebSocketServer(string url)
        {
            http = new HttpListener();
            http.Prefixes.Add(url);
        }

        public void Send(ReadOnlySpan<char> message)
        {
            foreach (var connection in connections.Values)
            {
                connection.Send(message);
            }
        }

        public void Send(ReadOnlySpan<byte> message)
        {
            foreach (var connection in connections.Values)
            {
                connection.Send(message);
            }
        }

        public async Task RunAsync(CancellationToken token)
        {
            if (http.IsListening)
            {
                throw new InvalidOperationException();
            }

            http.Start();

            await run(token).ConfigureAwait(false);

            http.Stop();
        }

        private async Task run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var context = await http.GetContextAsync().ConfigureAwait(false);

                if (context.Request.IsWebSocketRequest)
                {
                    await handleRequest(context, token).ConfigureAwait(false);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task handleRequest(HttpListenerContext httpContext, CancellationToken token)
        {
            WebSocketContext? sockContext;

            try
            {
                sockContext = await httpContext.AcceptWebSocketAsync(null).ConfigureAwait(false);
            }
            catch (Exception)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                return;
            }

            int id = Interlocked.Increment(ref nextClientId);
            var connection = new Connection(sockContext.WebSocket);

            if (!connections.TryAdd(id, connection))
            {
                return;
            }

            OnConnectionStart?.Invoke(id, connection);

            try
            {
                await connection.Start(token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Dispose();
                OnConnectionClose?.Invoke(id, connection);
                connections.TryRemove(id, out _);
            }
        }

        private sealed class Connection : IWebSocketConnection
        {
            private const int heartbeat_timeout = 15000;

            private bool isDisposed;
            private readonly WebSocket socket;
            private readonly ConcurrentQueue<Message> rx = new ConcurrentQueue<Message>();
            private readonly ConcurrentQueue<Message> tx = new ConcurrentQueue<Message>();

            public Connection(WebSocket socket)
            {
                this.socket = socket;
            }

            public Task Start(CancellationToken token)
            {
                return Task.WhenAll(recv(token), send(token));
            }

            public async Task Close(CancellationToken token)
            {
                await close(true, token).ConfigureAwait(false);
            }

            public void Send(ReadOnlySpan<char> message)
            {
                Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(message)];
                Encoding.UTF8.GetBytes(message, buffer);

                var stream = new MemoryStream(buffer.Length);

                stream.Write(buffer);
                stream.Position = 0;

                tx.Enqueue(new Message(stream, WebSocketMessageType.Text));
            }

            public void Send(ReadOnlySpan<byte> message)
            {
                var stream = new MemoryStream(message.Length);

                stream.Write(message);
                stream.Position = 0;

                tx.Enqueue(new Message(stream, WebSocketMessageType.Binary));
            }

            public void Dispose()
            {
                dispose();
            }

            private async Task send(CancellationToken token)
            {
                var buffer = MemoryPool<byte>.Shared.Rent(8192);

                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                    {
                        break;
                    }

                    if (socket.State != WebSocketState.Open)
                    {
                        continue;
                    }

                    while (tx.TryDequeue(out var send))
                    {
                        int count = (int)send.Stream.Length;

                        do
                        {
                            int read = send.Stream.Read(buffer.Memory.Span);

                            await socket.SendAsync(buffer.Memory.Slice(0, read), send.Type, (count -= read) <= 0, token).ConfigureAwait(false);
                        }
                        while (count > 0);

                        send.Stream.Dispose();
                    }
                }

                buffer.Dispose();
            }

            private async Task recv(CancellationToken token)
            {
                var buffer = MemoryPool<byte>.Shared.Rent(8192);

                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                    {
                        break;
                    }

                    var resp = default(ValueWebSocketReceiveResult);
                    var recv = new MemoryStream();

                    do
                    {
                        resp = await socket.ReceiveAsync(buffer.Memory, token).ConfigureAwait(false);
                        recv.Write(buffer.Memory.Span);
                    }
                    while (!resp.EndOfMessage);

                    recv.Position = 0;

                    if (resp.MessageType == WebSocketMessageType.Close && socket.State == WebSocketState.CloseReceived)
                    {
                        await close(false, token).ConfigureAwait(false);
                    }
                    else
                    {
                        rx.Enqueue(new Message(recv, resp.MessageType));
                    }
                }

                buffer.Dispose();
            }

            private async Task close(bool requested, CancellationToken token)
            {
                if (requested)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token).ConfigureAwait(false);
                }
                else
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token).ConfigureAwait(false);
                }

                dispose();
            }

            private void dispose()
            {
                if (isDisposed)
                {
                    return;
                }

                while (tx.TryDequeue(out var send))
                {
                    send.Stream.Dispose();
                }

                while (rx.TryDequeue(out var recv))
                {
                    recv.Stream.Dispose();
                }

                socket.Dispose();

                isDisposed = true;
            }

            private static readonly byte[] heartbeat_data = [0x7, 0x2, 0x7, 0x0];

            private readonly record struct Message(Stream Stream, WebSocketMessageType Type);
        }
    }

    public interface IWebSocketConnection
    {
        void Send(ReadOnlySpan<char> message);
        void Send(ReadOnlySpan<byte> message);
        Task Close(CancellationToken token);
    }
}
