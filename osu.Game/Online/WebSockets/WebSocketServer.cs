// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;

namespace osu.Game.Online.WebSockets
{
    public class WebSocketServer
    {
        public IEnumerable<WebSocketClient> Clients => connections.Values.Cast<WebSocketClient>();
        public int Connected => connections.Count;

        private int connectionId = -1;
        private HttpListener? listener;
        private Task? handleRequestTask;
        private CancellationTokenSource? cts;
        private readonly ConcurrentDictionary<int, WebSocketConnection> connections = new();

        public void Start(string uri)
        {
            if (listener != null || cts != null || handleRequestTask != null)
                return;

            cts = new CancellationTokenSource();

            listener = new();
            listener.Prefixes.Add(uri);

            listener.Start();
            handleRequestTask = Task.Run(() => handleRequest(cts.Token), cts.Token);
        }

        public void Stop(int timeout = 10000)
        {
            StopAsync(timeout).WaitSafely();
        }

        public async Task StopAsync(int timeout = 10000)
        {
            if (listener == null || cts == null || handleRequestTask == null)
                return;

            try
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                await Task.WhenAll(connections.Values.Select(c => c.StopAsync(timeout)));
            }
            catch (OperationCanceledException)
            {
            }

            cts.Cancel();
            await handleRequestTask;

            listener.Stop();
            listener = null;

            cts.Dispose();
            cts = null;

            handleRequestTask = null;
        }

        public void Send(ReadOnlyMemory<byte> data)
        {
            SendAsync(data, CancellationToken.None).WaitSafely();
        }

        public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
        {
            await Task.WhenAll(connections.Values.Select(c => c.SendAsync(data, token)));
        }

        protected virtual Task OnMessageReceived(WebSocketConnection connection, ReadOnlyMemory<byte> data, CancellationToken token) => Task.CompletedTask;

        private async Task handleRequest(CancellationToken token)
        {
            if (listener == null)
                return;

            while (!token.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();

                if (!await handleWebSocketRequest(context, token))
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
        }

        private async Task<bool> handleWebSocketRequest(HttpListenerContext context, CancellationToken token)
        {
            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null!);

                int nextId = Interlocked.Increment(ref connectionId);

                var connection = new WebSocketConnection(this, context, nextId);
                connections.TryAdd(nextId, connection);
                connection.Start(webSocketContext.WebSocket);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected class WebSocketConnection : WebSocketClient
        {
            public readonly HttpListenerContext Context;

            private readonly int id;
            private readonly WebSocketServer server;

            public WebSocketConnection(WebSocketServer server, HttpListenerContext context, int id)
            {
                Context = context;
                this.id = id;
                this.server = server;
            }

            protected override Task OnMessageReceived(ReadOnlyMemory<byte> data, CancellationToken token) => server.OnMessageReceived(this, data, token);

            protected override async Task Close(WebSocket socket, CancellationToken token)
            {
                // When the initiator is the server, CloseAsync must be called to notify the client
                // that the socket is being closed from the server side.
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);

                server.connections.TryRemove(id, out _);
            }
        }
    }
}
