// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class ModBroadcaster : StateBroadcasterWithBindable<IReadOnlyList<Mod>>
    {
        public ModBroadcaster(IBindable<IReadOnlyList<Mod>> bindable)
            : base(BroadcastID.GAME_MODS, bindable)
        {
        }
    }
}
