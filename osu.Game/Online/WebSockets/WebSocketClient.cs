// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;

namespace osu.Game.Online.WebSockets
{
    public class WebSocketClient
    {
        private WebSocket? socket;
        private Task? processIncomingTask;
        private CancellationTokenSource? cts;

        internal void Start(WebSocket socket)
        {
            if (this.socket != null || cts != null || processIncomingTask != null)
                return;

            this.socket = socket;

            cts = new CancellationTokenSource();
            processIncomingTask = Task.Run(() => processIncomingMessages(cts.Token), cts.Token);
        }

        public void Start(Uri uri, IDictionary<string, string>? headers = null)
        {
            StartAsync(uri, headers).WaitSafely();
        }

        public async Task StartAsync(Uri uri, IDictionary<string, string>? headers = null)
        {
            var client = new ClientWebSocket();

            if (headers != null)
            {
                foreach (var pair in headers)
                    client.Options.SetRequestHeader(pair.Key, pair.Value);
            }

            await client.ConnectAsync(uri, CancellationToken.None);
            Start(client);
        }

        public void Stop(int timeout = 10000)
        {
            StopAsync(timeout).WaitSafely();
        }

        public async Task StopAsync(int timeout = 10000)
        {
            if (socket == null || cts == null || processIncomingTask == null)
                return;

            try
            {
                // ReceiveAsync throws when we cancel its token and the close handshake has not been completed.
                // As we're initiating the handshake, we first tell the server that we're closing. Then we wait
                // until the server acknowledges our handshake.
                // See also: https://mcguirev10.com/2019/08/17/how-to-close-websocket-correctly.html
                using (var timeoutCts = new CancellationTokenSource(timeout))
                {
                    await Close(socket, timeoutCts.Token);
                    while (socket.State != WebSocketState.Closed && !timeoutCts.IsCancellationRequested) ;
                }
            }
            catch (OperationCanceledException)
            {
            }

            cts.Cancel();
            await processIncomingTask;

            socket.Dispose();
            socket = null;

            cts.Dispose();
            cts = null;

            processIncomingTask = null;
        }

        public void Send(ReadOnlyMemory<byte> data)
        {
            SendAsync(data, CancellationToken.None).WaitSafely();
        }

        public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
        {
            if (cts != null && cts.IsCancellationRequested)
                return;

            if (socket != null && socket.State != WebSocketState.Open)
                return;

            if (cts == null || socket == null || processIncomingTask == null)
                return;

            await socket.SendAsync(data, WebSocketMessageType.Binary, true, token);
        }

        protected virtual Task OnMessageReceived(ReadOnlyMemory<byte> data, CancellationToken token) => Task.CompletedTask;

        protected virtual Task Close(WebSocket socket, CancellationToken token)
        {
            // When the initiator is the client, CloseOutputAsync must be called so the websocket can transition to the
            // CloseSent state which is required to transition to the Closed state when the server acknowledges our close message.
            return socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }

        private async Task processIncomingMessages(CancellationToken token)
        {
            if (socket == null)
                return;

            int bytesRead = 0;
            var buffer = MemoryPool<byte>.Shared.Rent(65536);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed)
                        continue;

                    var result = await socket.ReceiveAsync(buffer.Memory.Slice(bytesRead), token);
                    bytesRead += result.Count;

                    // If the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted.
                    if (socket.State == WebSocketState.Aborted)
                        continue;

                    // The server is notifying us that the connection will close.
                    if (socket.State == WebSocketState.CloseReceived && result.MessageType == WebSocketMessageType.Close)
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                    // Data is sent in chunks. Only call when an "end of message" packet has been sent.
                    if (socket.State == WebSocketState.Open && result.MessageType != WebSocketMessageType.Close && result.EndOfMessage)
                    {
                        await OnMessageReceived(buffer.Memory.Slice(0, bytesRead), token);
                        bytesRead = 0;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (WebSocketException)
            {
            }
            finally
            {
                buffer.Dispose();
            }
        }
    }
}
