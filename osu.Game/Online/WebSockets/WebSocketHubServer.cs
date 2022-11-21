// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online.WebSockets
{
    public class WebSocketHubServer : WebSocketHub
    {
        protected HttpListenerContext? Context { get; private set; }

        private readonly WebSocketServer server;

        public WebSocketHubServer()
        {
            server = new HubServer(this);
        }

        public override Task StartAsync(string uri, IDictionary<string, string>? headers = null)
        {
            server.Start(uri);
            return Task.CompletedTask;
        }

        public override void Start(string uri, IDictionary<string, string>? headers = null) => server.Start(uri);
        public override void Stop(int timeout = 10000) => server.Stop(timeout);
        public override Task StopAsync(int timeout = 10000) => server.StopAsync(timeout);
        internal override Task HandleOutgoingMessage(byte[] message) => server.SendAsync(message);

        private class HubServer : WebSocketServer
        {
            private readonly WebSocketHubServer hub;

            public HubServer(WebSocketHubServer hub)
            {
                this.hub = hub;
            }

            protected override Task OnMessageReceived(WebSocketConnection connection, ReadOnlyMemory<byte> data, CancellationToken token)
            {
                hub.Context = connection.Context;
                return hub.HandleIncomingMessage(data);
            }
        }
    }
}
