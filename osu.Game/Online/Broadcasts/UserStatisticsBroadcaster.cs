// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class UserStatisticsBroadcaster : StateBroadcasterWithBindable<UserStatistics?>
    {
        public UserStatisticsBroadcaster(IAPIProvider api)
            : base(BroadcastID.USER_STATISTIC, api.Statistics)
        {
        }
    }
}
