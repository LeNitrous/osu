// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace osu.Game.Online.WebSockets
{
    public abstract class WebSocketHub
    {
        private readonly Dictionary<string, Func<dynamic[], Task>> handlers = new();

        public abstract void Start(string uri, IDictionary<string, string>? headers = null);

        public abstract Task StartAsync(string uri, IDictionary<string, string>? headers = null);

        public abstract void Stop(int timeout = 10000);

        public abstract Task StopAsync(int timeout = 10000);

        public void On(string methodName, Func<dynamic[], Task> handler)
        {
            if (handlers.ContainsKey(methodName))
                return;

            handlers.Add(methodName, handler);
        }

        public void On<T>(string methodName, Func<T, Task> handler)
        {
            On(methodName, args => handler((T)args[0]));
        }

        public void On<T1, T2>(string methodName, Func<T1, T2, Task> handler)
        {
            On(methodName, args => handler((T1)args[0], (T2)args[1]));
        }

        public Task Invoke(string methodName, params dynamic[] args)
        {
            return HandleOutgoingMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new WebSocketHubMessage { Method = methodName, Args = args }, settings)));
        }

        internal Task HandleIncomingMessage(ReadOnlyMemory<byte> data)
        {
            var msg = JsonConvert.DeserializeObject<WebSocketHubMessage>(Encoding.UTF8.GetString(data.Span), settings);

            if (string.IsNullOrEmpty(msg?.Method) || !handlers.TryGetValue(msg.Method, out var handler))
                return Task.CompletedTask;

            return handler(msg.Args);
        }

        internal abstract Task HandleOutgoingMessage(byte[] message);

        private static readonly JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.Auto };
    }
}
