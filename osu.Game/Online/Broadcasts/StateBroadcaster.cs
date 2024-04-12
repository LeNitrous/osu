// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Online.Broadcasts
{
    public partial class StateBroadcaster<T> : Component
    {
        [Resolved]
        private IStateBroadcastServer server { get; set; } = null!;

        private readonly string type;

        public StateBroadcaster(string type)
        {
            this.type = type;
        }

        protected void Broadcast(T data)
        {
            if (server is null)
            {
                return;
            }

            server.Broadcast(type, data);
        }
    }
}
