// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamWin.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public class TeamWinScreen : TournamentMatchScreen
    {
        private Container mainContainer;

        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        private TourneyVideo blueWinVideo;
        private TourneyVideo redWinVideo;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                blueWinVideo = new TourneyVideo("teamwin-blue")
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                redWinVideo = new TourneyVideo("teamwin-red")
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            currentCompleted.BindValueChanged(_ => update());
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            base.CurrentMatchChanged(match);

            currentCompleted.UnbindBindings();

            if (match.NewValue == null)
                return;

            currentCompleted.BindTo(match.NewValue.Completed);
            update();
        }

        private bool firstDisplay = true;

        private void update() => Scheduler.AddOnce(() =>
        {
            var match = CurrentMatch.Value;

            if (match.Winner == null)
            {
                mainContainer.Clear();
                return;
            }

            redWinVideo.Alpha = match.WinnerColour == TeamColour.Red ? 1 : 0;
            blueWinVideo.Alpha = match.WinnerColour == TeamColour.Blue ? 1 : 0;

            if (firstDisplay)
            {
                if (match.WinnerColour == TeamColour.Red)
                    redWinVideo.Reset();
                else
                    blueWinVideo.Reset();
                firstDisplay = false;
            }

            mainContainer.Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Y = 125,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = match.Round.Value?.Name.Value ?? "Unknown Round",
                            Font = OsuFont.Torus.With(size: 36)
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Winner",
                            Font = OsuFont.Torus.With(size: 36)
                        },
                    }
                },
                new PlayerWin(match.Winner)
                {
                    Y = 25,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
            mainContainer.FadeOut();
            mainContainer.Delay(2000).FadeIn(1600, Easing.OutQuint);
        });
    }
}
