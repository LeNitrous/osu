// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online.WebSockets
{
    public class WebSocketHubClient : WebSocketHub
    {
        private readonly WebSocketClient client;

        public WebSocketHubClient()
        {
            client = new HubClient(this);
        }

        public override void Start(string uri, IDictionary<string, string>? headers = null) => client.Start(new Uri(uri), headers);
        public override Task StartAsync(string uri, IDictionary<string, string>? headers = null) => client.StartAsync(new Uri(uri), headers);
        public override void Stop(int timeout = 10000) => client.Stop(timeout);
        public override Task StopAsync(int timeout = 10000) => client.StopAsync(timeout);
        internal override Task HandleOutgoingMessage(byte[] message) => client.SendAsync(message);

        private class HubClient : WebSocketClient
        {
            private readonly WebSocketHubClient hub;

            public HubClient(WebSocketHubClient hub)
            {
                this.hub = hub;
            }

            protected override Task OnMessageReceived(ReadOnlyMemory<byte> data, CancellationToken token)
            {
                return hub.HandleIncomingMessage(data);
            }
        }
    }
}
