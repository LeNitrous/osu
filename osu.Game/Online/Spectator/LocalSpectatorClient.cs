// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.WebSockets;

namespace osu.Game.Online.Spectator
{
    public class LocalSpectatorClient : SpectatorClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private IBindable<APIUser>? user;
        private Task? runTask;
        private readonly WebSocketHubClient client;
        private readonly BindableBool isConnected = new();

        public LocalSpectatorClient()
        {
            client = new WebSocketHubClient();
            client.On<int, SpectatorState>(nameof(ISpectatorClient.UserBeganPlaying), ((ISpectatorClient)this).UserBeganPlaying);
            client.On<int, SpectatorState>(nameof(ISpectatorClient.UserFinishedPlaying), ((ISpectatorClient)this).UserFinishedPlaying);
            client.On<int, FrameDataBundle>(nameof(ISpectatorClient.UserSentFrames), ((ISpectatorClient)this).UserSentFrames);
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            user = api.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                runTask = Task.Run(async () =>
                {
                    await client.StopAsync();
                    await client.StartAsync(@"ws://localhost:7270/spectator/", new Dictionary<string, string>() { { @"X-osu-UserID", u.NewValue.Id.ToString() } });
                });
            }, true);
        }

        protected override Task BeginPlayingInternal(SpectatorState state) => client.Invoke(nameof(ISpectatorServer.BeginPlaySession), state);
        protected override Task EndPlayingInternal(SpectatorState state) => client.Invoke(nameof(ISpectatorServer.EndPlaySession), state);
        protected override Task SendFramesInternal(FrameDataBundle bundle) => client.Invoke(nameof(ISpectatorServer.SendFrameData), bundle);
        protected override Task StopWatchingUserInternal(int userId) => client.Invoke(nameof(ISpectatorServer.EndWatchingUser), userId);
        protected override Task WatchUserInternal(int userId) => client.Invoke(nameof(ISpectatorServer.StartWatchingUser), userId);
    }
}
