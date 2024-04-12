// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class PlayerClockBroadcaster : StateBroadcaster<double>
    {
        private const double time_between_broadcasts = 500;

        private double prevBroadcastTime;
        private readonly GameplayClockContainer clock;

        public PlayerClockBroadcaster(GameplayClockContainer clock)
            : base(BroadcastID.PLAY_CLOCK)
        {
            this.clock = clock;
        }

        protected override void Update()
        {
            if (!clock.IsRunning)
            {
                return;
            }

            if ((Time.Current - prevBroadcastTime) < time_between_broadcasts)
            {
                return;
            }

            Broadcast(clock.CurrentTime);

            prevBroadcastTime = Time.Current;
        }
    }
}
