// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class NowPlayingInfo : CompositeDrawable
    {
        public TournamentBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                update();
            }
        }

        public LegacyMods Mods
        {
            get => mods;
            set
            {
                mods = value;
                update();
            }
        }

        [Resolved]
        private IBindable<RulesetInfo> rulesetInfo { get; set; }

        private TournamentBeatmap beatmap;
        private LegacyMods mods;
        private FillFlowContainer modsFlow;
        private bool showMods = true;

        public bool ShowMods
        {
            get => showMods;
            set
            {
                if (value == showMods)
                    return;

                showMods = true;

                if (IsLoaded)
                    modsFlow.FadeTo(showMods ? 1 : 0, 100);
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(800, 75);
            Masking = true;
        }

        private void update()
        {
            if (beatmap == null)
                return;

            double bpm = beatmap.BPM;
            double length = beatmap.Length;
            string hardRockExtra = string.Empty;
            float ar = beatmap.Difficulty.ApproachRate;

            if ((Mods & LegacyMods.HardRock) > 0)
            {
                hardRockExtra = "*";
            }

            if ((Mods & LegacyMods.DoubleTime) > 0)
            {
                // temporary local calculation (taken from OsuDifficultyCalculator)
                double preempt = (int)IBeatmapDifficultyInfo.DifficultyRange(ar, 1800, 1200, 450) / 1.5;
                ar = (float)(preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5);

                bpm *= 1.5f;
                length /= 1.5f;
            }

            (string heading, string content)[] stats;

            switch (rulesetInfo.Value.OnlineID)
            {
                default:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{beatmap.Difficulty.CircleSize:0.#}{hardRockExtra}"),
                        ("AR", $"{ar:0.#}{hardRockExtra}"),
                        ("OD", $"{beatmap.Difficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                    };
                    break;

                case 1:
                case 3:
                    stats = new (string heading, string content)[]
                    {
                        ("OD", $"{beatmap.Difficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                        ("HP", $"{beatmap.Difficulty.DrainRate:0.#}{hardRockExtra}")
                    };
                    break;

                case 2:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{beatmap.Difficulty.CircleSize:0.#}{hardRockExtra}"),
                        ("AR", $"{ar:0.#}"),
                    };
                    break;
            }

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                new UpdateableOnlineBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.5f),
                    OnlineInfo = Beatmap,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding(15),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = Beatmap.GetDisplayTitleRomanisable(false, false),
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = "mapper",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14)
                                },
                                new TournamentSpriteText
                                {
                                    Text = Beatmap.Metadata.Author.Username,
                                    Padding = new MarginPadding { Right = 20 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14)
                                },
                                new TournamentSpriteText
                                {
                                    Text = "difficulty",
                                    Padding = new MarginPadding { Right = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14)
                                },
                                new TournamentSpriteText
                                {
                                    Text = Beatmap.DifficultyName,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 14)
                                },
                            }
                        }
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Padding = new MarginPadding(15),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new StarRatingDisplay(new StarDifficulty(Beatmap.StarRating, 0))
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        },
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Right = 10 },
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new DiffPiece(stats),
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new DiffPiece(("Length", length.ToFormattedDuration().ToString())),
                                        new DiffPiece(("BPM", $"{bpm:0.#}")),
                                    }
                                }
                            }
                        },
                        modsFlow = new FillFlowContainer
                        {
                            Alpha = showMods ? 1 : 0,
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children = getModAcronyms(Mods).Select(a => new TournamentModIcon(a)
                            {
                                Width = 50,
                                RelativeSizeAxes = Axes.Y,
                            }).ToArray(),
                        }
                    }
                },
            });
        }

        private static IEnumerable<string> getModAcronyms(LegacyMods mods)
        {
            if (mods.HasFlagFast(LegacyMods.Easy))
                yield return "EZ";

            if (mods.HasFlagFast(LegacyMods.Hidden))
                yield return "HD";

            if (mods.HasFlagFast(LegacyMods.HardRock))
                yield return "HR";

            if (mods.HasFlagFast(LegacyMods.SuddenDeath))
                yield return "SD";

            if (mods.HasFlagFast(LegacyMods.DoubleTime))
                yield return "DT";

            if (mods.HasFlagFast(LegacyMods.Relax))
                yield return "RX";

            if (mods.HasFlagFast(LegacyMods.HalfTime))
                yield return "HT";

            if (mods.HasFlagFast(LegacyMods.Nightcore))
                yield return "NC";

            if (mods.HasFlagFast(LegacyMods.Flashlight))
                yield return "FL";

            if (mods.HasFlagFast(LegacyMods.Autoplay))
                yield return "AU";

            if (mods.HasFlagFast(LegacyMods.SpunOut))
                yield return "SO";

            if (mods.HasFlagFast(LegacyMods.Autopilot))
                yield return "AP";

            if (mods.HasFlagFast(LegacyMods.Perfect))
                yield return "PF";

            if (mods.HasFlagFast(LegacyMods.Key4))
                yield return "4K";

            if (mods.HasFlagFast(LegacyMods.Key5))
                yield return "5K";

            if (mods.HasFlagFast(LegacyMods.Key6))
                yield return "6K";

            if (mods.HasFlagFast(LegacyMods.Key7))
                yield return "7K";

            if (mods.HasFlagFast(LegacyMods.Key8))
                yield return "8K";

            if (mods.HasFlagFast(LegacyMods.FadeIn))
                yield return "FI";

            if (mods.HasFlagFast(LegacyMods.Random))
                yield return "RD";

            if (mods.HasFlagFast(LegacyMods.Cinema))
                yield return "CN";

            if (mods.HasFlagFast(LegacyMods.Target))
                yield return "TP";

            if (mods.HasFlagFast(LegacyMods.Key9))
                yield return "9K";

            if (mods.HasFlagFast(LegacyMods.Key1))
                yield return "1K";

            if (mods.HasFlagFast(LegacyMods.Key3))
                yield return "3K";

            if (mods.HasFlagFast(LegacyMods.Key2))
                yield return "2K";

            if (mods.HasFlagFast(LegacyMods.Mirror))
                yield return "MR";
        }

        public class DiffPiece : TextFlowContainer
        {
            public DiffPiece(params (string heading, string content)[] tuples)
            {
                Margin = new MarginPadding { Horizontal = 15, Vertical = 1 };
                AutoSizeAxes = Axes.Both;

                static void cp(SpriteText s, bool bold)
                {
                    s.Font = OsuFont.Torus.With(weight: bold ? FontWeight.Bold : FontWeight.Regular, size: 15);
                }

                for (int i = 0; i < tuples.Length; i++)
                {
                    (string heading, string content) = tuples[i];

                    if (i > 0)
                    {
                        AddText(" / ", s =>
                        {
                            cp(s, false);
                            s.Spacing = new Vector2(-2, 0);
                        });
                    }

                    AddText(new TournamentSpriteText { Text = heading }, s => cp(s, false));
                    AddText(" ", s => cp(s, false));
                    AddText(new TournamentSpriteText { Text = content }, s => cp(s, true));
                }
            }
        }
    }
}
