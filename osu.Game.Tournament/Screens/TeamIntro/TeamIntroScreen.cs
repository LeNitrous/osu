// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamIntro.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamIntro
{
    public class TeamIntroScreen : TournamentMatchScreen
    {
        private Container mainContainer;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("teamintro")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            base.CurrentMatchChanged(match);

            mainContainer.Clear();

            if (match.NewValue == null)
                return;

            const float gap = 75;

            mainContainer.Children = new Drawable[]
            {
                new TournamentSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = 100,
                    Text = match.NewValue.Round.Value?.Name.Value ?? "Unknown Round",
                    Font = OsuFont.Torus.With(size: 26)
                },
                new PlayerIntro(match.NewValue.Team1.Value, TeamColour.Red)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    X = gap,
                },
                new PlayerIntro(match.NewValue.Team2.Value, TeamColour.Blue)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    X = -gap,
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.FistRaised,
                    Size = new Vector2(64),
                },
            };
        }
    }
}
