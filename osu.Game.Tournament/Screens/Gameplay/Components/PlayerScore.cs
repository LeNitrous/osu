// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class PlayerScore : DrawablePlayer
    {
        private readonly int pointsToWin;
        private readonly TeamColour colour;
        private readonly Bindable<int?> currentTeamScore;
        private TournamentSpriteText name;
        private TournamentSpriteText rank;
        private Container avatarContainer;
        private Container coverContainer;
        private PlayerScoreCounter counter;
        private bool showScore;

        public bool ShowScore
        {
            get => showScore;
            set
            {
                if (showScore == value)
                    return;

                showScore = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        public PlayerScore(TournamentTeam team, TeamColour colour, Bindable<int?> currentTeamScore, int pointsToWin)
            : base(team)
        {
            this.colour = colour;
            this.pointsToWin = pointsToWin;
            this.currentTeamScore = currentTeamScore.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = 500;
            RelativeSizeAxes = Axes.Y;

            bool flip = colour == TeamColour.Blue;
            var anchor = flip ? Anchor.CentreRight : Anchor.CentreLeft;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Height = 5,
                    Anchor = flip ? Anchor.BottomRight : Anchor.BottomLeft,
                    Origin = flip ? Anchor.TopRight : Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    Colour = flip
                        ? ColourInfo.GradientHorizontal(((Color4)TournamentGame.GetTeamColour(colour)).Opacity(0), TournamentGame.GetTeamColour(colour))
                        : ColourInfo.GradientHorizontal(TournamentGame.GetTeamColour(colour), ((Color4)TournamentGame.GetTeamColour(colour)).Opacity(0))
                },
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(15, 0),
                    Anchor = anchor,
                    Origin = anchor,
                    AutoSizeAxes = Axes.Both,
                    Margin = flip ? new MarginPadding { Right = 20 } : new MarginPadding { Left = 20 },
                    Children = new Drawable[]
                    {
                        avatarContainer = new Container
                        {
                            Size = new Vector2(60),
                            Masking = true,
                            CornerRadius = 10,
                            Anchor = anchor,
                            Origin = anchor,
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Anchor = anchor,
                            Origin = anchor,
                            Children = new Drawable[]
                            {
                                name = new TournamentSpriteText
                                {
                                    Anchor = anchor,
                                    Origin = anchor,
                                },
                                rank = new TournamentSpriteText
                                {
                                    Anchor = anchor,
                                    Origin = anchor,
                                },
                                counter = new PlayerScoreCounter(pointsToWin)
                                {
                                    Anchor = anchor,
                                    Margin = new MarginPadding { Top = 5 },
                                    Scale = flip ? new Vector2(-1, 1) : Vector2.One,
                                }
                            }
                        }
                    }
                }
            };

            LoadComponentAsync(new DrawableAvatar(User.ToAPIUser())
            {
                RelativeSizeAxes = Axes.Both,
            }, avatarContainer.Add);

            LoadComponentAsync(new UserCoverBackground
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = User.ToAPIUser(),
                Colour = flip
                    ? ColourInfo.GradientHorizontal(Colour4.Transparent, Colour4.White.Opacity(0.25f))
                    : ColourInfo.GradientHorizontal(Colour4.White.Opacity(0.25f), Colour4.Transparent),
            }, coverContainer.Add);

            name.Text = User.Username;
            name.Font = name.Font.With(size: 24, weight: FontWeight.Bold);
            rank.Text = $"COUNTRY RANK {(User.CountryRank.HasValue ? $"#{User.CountryRank.Value:n0}" : "Unknown")}";
            rank.Font = rank.Font.With(size: 14, weight: FontWeight.Bold);
            rank.Alpha = 0.3f;

            currentTeamScore.BindValueChanged(scoreChanged);

            updateDisplay();
        }

        private void scoreChanged(ValueChangedEvent<int?> score) => counter.Current = score.NewValue ?? 0;
        private void updateDisplay() => counter.FadeTo(ShowScore ? 1 : 0, 200);

        private class PlayerScoreCounter : StarCounter
        {
            public PlayerScoreCounter(int count)
                : base(count)
            {
            }

            public override Star CreateStar() => new Light();

            public class Light : Star
            {
                public Light()
                {
                    Size = new Vector2(20, 6);
                    Masking = true;
                    CornerRadius = 5;

                    InternalChild = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.FromHex("#2E2E2E")
                    };
                }

                public override void DisplayAt(float scale)
                {
                    InternalChild.Colour = scale > 0 ? Colour4.White : Colour4.FromHex("#2E2E2E");
                }
            }
        }
    }
}
