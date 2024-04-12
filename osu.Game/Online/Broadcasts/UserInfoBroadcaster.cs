// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class UserInfoBroadcaster : StateBroadcasterWithBindable<APIUser>
    {
        public UserInfoBroadcaster(IAPIProvider api)
            : base(BroadcastID.USER_INFO, api.LocalUser)
        {
        }
    }
}
