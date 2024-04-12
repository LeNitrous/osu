// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class UserActivityBroadcaster : StateBroadcasterWithBindable<UserActivity, string>
    {
        public UserActivityBroadcaster(IAPIProvider api)
            : base(BroadcastID.USER_ACTIVITY, api.Activity)
        {
        }

        protected override string GetData(UserActivity from)
        {
            switch (from)
            {
                case UserActivity.InLobby:
                    return @"lobby";

                case UserActivity.SearchingForLobby:
                    return @"lobby-search";

                case UserActivity.ChoosingBeatmap:
                    return @"select";

                case UserActivity.TestingBeatmap:
                    return @"testing";

                case UserActivity.ModdingBeatmap:
                    return @"modding";

                case UserActivity.EditingBeatmap:
                    return @"editing";

                case UserActivity.InSoloGame:
                    return @"play-solo";

                case UserActivity.InPlaylistGame:
                    return @"play-playlist";

                case UserActivity.InMultiplayerGame:
                    return @"play-multi";

                case UserActivity.SpectatingMultiplayerGame:
                    return @"spectate-multi";

                case UserActivity.SpectatingUser:
                    return @"spectate-user";

                case UserActivity.WatchingReplay:
                    return @"spectate-replay";

                default:
                    return string.Empty;
            }
        }
    }
}
