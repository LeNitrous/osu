// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamIntro.Components
{
    public class PlayerIntro : DrawablePlayer
    {
        private readonly TeamColour colour;
        private Container avatarContainer;

        public PlayerIntro(TournamentTeam team, TeamColour colour)
            : base(team)
        {
            this.colour = colour;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            bool flip = colour == TeamColour.Blue;
            var anchor = flip ? Anchor.CentreRight : Anchor.CentreLeft;

            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    avatarContainer = new Container
                    {
                        Size = new Vector2(120),
                        Masking = true,
                        CornerRadius = 10,
                        Anchor = anchor,
                        Origin = anchor,
                        Margin = new MarginPadding { Bottom = 10 },
                    },
                    new TournamentSpriteText
                    {
                        Text = User.Username,
                        Font = OsuFont.Torus.With(size: 36, weight: FontWeight.Bold),
                        Anchor = anchor,
                        Origin = anchor,
                    },
                    new TournamentSpriteText
                    {
                        Text = $"Country Ranking #{(User.CountryRank.HasValue ? $"{User.CountryRank.Value:0.#}" : "Unknown")}",
                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.Bold),
                        Alpha = 0.6f,
                        Anchor = anchor,
                        Origin = anchor,
                    }
                }
            };

            LoadComponentAsync(new DrawableAvatar(User.ToAPIUser())
            {
                RelativeSizeAxes = Axes.Both,
            }, avatarContainer.Add);
        }
    }
}
