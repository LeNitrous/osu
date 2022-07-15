// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamWin.Components
{
    public class PlayerWin : DrawablePlayer
    {
        private Container avatarContainer;

        public PlayerWin(TournamentTeam team)
            : base(team)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Size = new Vector2(36),
                        Icon = FontAwesome.Solid.Crown,
                        Colour = Colour4.FromHex("#F4B206"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Bottom = 10 },
                    },
                    avatarContainer = new Container
                    {
                        Size = new Vector2(160),
                        Masking = true,
                        CornerRadius = 10,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Bottom = 10 },
                    },
                    new TournamentSpriteText
                    {
                        Text = User.Username,
                        Font = OsuFont.Torus.With(size: 48, weight: FontWeight.Bold),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new TournamentSpriteText
                    {
                        Text = $"Country Ranking #{(User.CountryRank.HasValue ? $"{User.CountryRank.Value:0.#}" : "Unknown")}",
                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.Bold),
                        Alpha = 0.6f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                },
            };

            LoadComponentAsync(new DrawableAvatar(User.ToAPIUser())
            {
                RelativeSizeAxes = Axes.Both,
            }, avatarContainer.Add);
        }
    }
}
