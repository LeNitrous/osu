// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class BeatmapBroadcaster : StateBroadcasterWithBindable<WorkingBeatmap, BeatmapInfo>
    {
        public BeatmapBroadcaster(IBindable<WorkingBeatmap> bindable)
            : base(BroadcastID.GAME_BEATMAP, bindable)
        {
        }

        protected override BeatmapInfo GetData(WorkingBeatmap working)
        {
            return working.BeatmapInfo;
        }
    }
}
