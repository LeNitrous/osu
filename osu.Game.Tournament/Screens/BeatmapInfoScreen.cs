// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    public abstract class BeatmapInfoScreen : TournamentMatchScreen
    {
        protected readonly NowPlayingInfo NowPlayingInfo;

        protected BeatmapInfoScreen()
        {
            AddInternal(NowPlayingInfo = new NowPlayingInfo
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Depth = float.MinValue,

            });
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            ipc.Beatmap.BindValueChanged(beatmapChanged, true);
            ipc.Mods.BindValueChanged(modsChanged, true);
        }

        private void modsChanged(ValueChangedEvent<LegacyMods> mods)
        {
            NowPlayingInfo.Mods = mods.NewValue;
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap> beatmap)
        {
            NowPlayingInfo.FadeInFromZero(300, Easing.OutQuint);
            NowPlayingInfo.Beatmap = beatmap.NewValue;
        }
    }
}
