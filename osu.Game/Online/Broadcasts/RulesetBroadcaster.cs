// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class RulesetBroadcaster : StateBroadcasterWithBindable<RulesetInfo, string>
    {
        public RulesetBroadcaster(IBindable<RulesetInfo> bindable)
            : base(BroadcastID.GAME_RULESET, bindable)
        {
        }

        protected override string GetData(RulesetInfo ruleset)
        {
            return ruleset.ShortName;
        }
    }
}
