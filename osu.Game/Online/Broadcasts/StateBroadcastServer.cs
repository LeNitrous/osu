// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.IO.Serialization;

namespace osu.Game.Online.Broadcasts
{
    public interface IStateBroadcastServer
    {
        void Start(IStateBroadcasterWithCurrent broadcaster);

        void Close(IStateBroadcasterWithCurrent broadcaster);

        void Broadcast<T>(string type, T data);
    }

    [Cached(typeof(IStateBroadcastServer))]
    public sealed partial class StateBroadcastServer : Component, IStateBroadcastServer
    {
        private CancellationTokenSource? source;
        private readonly WebSocketServer server;
        private readonly ConcurrentDictionary<Type, IStateBroadcasterWithCurrent> broadcasters = new ConcurrentDictionary<Type, IStateBroadcasterWithCurrent>();

        public StateBroadcastServer()
        {
            server = new WebSocketServer(@"http://+:7270/");
            server.OnConnectionStart += onConnectionStart;
        }

        protected override void LoadComplete()
        {
            source = new CancellationTokenSource();
            Task.Run(() => server.RunAsync(source.Token), source.Token);
            base.LoadComplete();
        }

        protected override void Dispose(bool isDisposing)
        {
            source?.Cancel();
            base.Dispose(isDisposing);
        }

        private void onConnectionStart(int id, IWebSocketConnection connection)
        {
            foreach (var broadcaster in broadcasters.Values)
            {
                broadcaster.Broadcast();
            }
        }

        void IStateBroadcastServer.Start(IStateBroadcasterWithCurrent broadcaster)
        {
            broadcasters.TryAdd(broadcaster.GetType(), broadcaster);
        }

        void IStateBroadcastServer.Close(IStateBroadcasterWithCurrent broadcaster)
        {
            broadcasters.TryRemove(broadcaster.GetType(), out _);
        }

        void IStateBroadcastServer.Broadcast<T>(string type, T data)
        {
            server.Send(new EventData<T>(type, data).Serialize());
        }

        [JsonObject(MemberSerialization.OptIn)]
        private record struct EventData<T>
        (
            [property: JsonProperty("type")]
            string Type,

            [property: JsonProperty("data")]
            T Data
        );
    }
}
