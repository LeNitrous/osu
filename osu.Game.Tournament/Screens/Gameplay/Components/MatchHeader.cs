// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class MatchHeader : Container
    {
        private PlayerScoreDisplay teamDisplay1;
        private PlayerScoreDisplay teamDisplay2;

        private bool showScores = true;

        public bool ShowScores
        {
            get => showScores;
            set
            {
                if (value == showScores)
                    return;

                showScores = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        private bool showLogo = true;

        public bool ShowLogo
        {
            get => showLogo;
            set
            {
                if (value == showLogo)
                    return;

                showLogo = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 110;
            Children = new Drawable[]
            {
                new MatchRoundDisplay
                {
                    Y = 20,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.5f)
                },
                teamDisplay1 = new PlayerScoreDisplay(TeamColour.Red)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                teamDisplay2 = new PlayerScoreDisplay(TeamColour.Blue)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            teamDisplay1.ShowScore = showScores;
            teamDisplay2.ShowScore = showScores;
        }
    }
}
