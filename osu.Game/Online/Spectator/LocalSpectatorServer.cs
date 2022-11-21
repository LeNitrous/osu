// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Threading.Tasks;
using osu.Game.Online.WebSockets;

namespace osu.Game.Online.Spectator
{
    public class LocalSpectatorServer : WebSocketHubServer, ISpectatorServer
    {
        protected int CurrentContextUserId => Context != null ? int.Parse(Context.Request.Headers[@"X-osu-UserId"]) : 0;

        private readonly ConcurrentDictionary<int, SpectatorState> states = new();

        public LocalSpectatorServer()
        {
            On<SpectatorState>(nameof(ISpectatorServer.BeginPlaySession), ((ISpectatorServer)this).BeginPlaySession);
            On<SpectatorState>(nameof(ISpectatorServer.EndPlaySession), ((ISpectatorServer)this).EndPlaySession);
            On<int>(nameof(ISpectatorServer.StartWatchingUser), ((ISpectatorServer)this).StartWatchingUser);
            On<int>(nameof(ISpectatorServer.EndWatchingUser), ((ISpectatorServer)this).EndWatchingUser);
            On<FrameDataBundle>(nameof(ISpectatorServer.SendFrameData), ((ISpectatorServer)this).SendFrameData);
        }

        Task ISpectatorServer.BeginPlaySession(SpectatorState state)
        {
            var current = states.GetOrAdd(CurrentContextUserId, _ => state);
            return Invoke(nameof(ISpectatorClient.UserBeganPlaying), new dynamic[] { CurrentContextUserId, state });
        }

        Task ISpectatorServer.EndPlaySession(SpectatorState state)
        {
            states.TryRemove(CurrentContextUserId, out _);
            return Invoke(nameof(ISpectatorClient.UserFinishedPlaying), new dynamic[] { CurrentContextUserId, state });
        }

        Task ISpectatorServer.EndWatchingUser(int userId)
        {
            return Task.CompletedTask;
        }

        Task ISpectatorServer.SendFrameData(FrameDataBundle data)
        {
            return Invoke(nameof(ISpectatorClient.UserSentFrames), new dynamic[] { CurrentContextUserId, data });
        }

        Task ISpectatorServer.StartWatchingUser(int userId)
        {
            if (states.TryGetValue(userId, out var state))
                return Invoke(nameof(ISpectatorClient.UserBeganPlaying), new dynamic[] { userId, state });

            return Task.CompletedTask;
        }
    }
}
