// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLeaderboardScoreV2 : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private FillFlowContainer? fillFlow;
        private OsuSpriteText? drawWidthText;
        private float relativeWidth;

        [BackgroundDependencyLoader]
        private void load()
        {
            // TODO: invalidation seems to be one-off when clicking slider to a certain value, so drag for now
            // doesn't seem to happen in-game (when toggling window mode)
            AddSliderStep("change relative width", 0, 1f, 0.6f, v =>
            {
                relativeWidth = v;
                if (fillFlow != null) fillFlow.Width = v;
            });
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                fillFlow = new FillFlowContainer
                {
                    Width = relativeWidth,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 2f),
                },
                drawWidthText = new OsuSpriteText(),
            };

            int i = 0;

            foreach (var scoreInfo in getTestScores())
            {
                fillFlow.Add(new LeaderboardScoreV2(scoreInfo, scoreInfo.Position, scoreInfo.User.Id == 2)
                {
                    Margin = new MarginPadding { Right = 10f * i, Left = -10f * i++ },
                });
            }

            foreach (var score in fillFlow.Children)
                score.Show();
        });

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (drawWidthText != null) drawWidthText.Text = $"DrawWidth: {fillFlow?.DrawWidth}";
        }

        private static ScoreInfo[] getTestScores()
        {
            var scores = new[]
            {
                new ScoreInfo
                {
                    Position = 999,
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        CountryCode = CountryCode.ES,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    },
                    Date = DateTimeOffset.Now.AddYears(-2),
                },
                new ScoreInfo
                {
                    Position = 22333,
                    Rank = ScoreRank.S,
                    Accuracy = 0.1f,
                    MaxCombo = 32040,
                    TotalScore = 1707827,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        CountryCode = CountryCode.CA,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    },
                    Date = DateTimeOffset.Now.AddMonths(-6),
                },
                new ScoreInfo
                {
                    Position = 110000,
                    Rank = ScoreRank.A,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 17078279,
                    Ruleset = new ManiaRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Username = @"No cover",
                        CountryCode = CountryCode.BR,
                    },
                    Date = DateTimeOffset.Now,
                },
                new ScoreInfo
                {
                    Position = 110000,
                    Rank = ScoreRank.A,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1234567890,
                    Ruleset = new ManiaRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 226597,
                        Username = @"WWWWWWWWWWWWWWWWWWWW",
                        CountryCode = CountryCode.US,
                    },
                    Date = DateTimeOffset.Now,
                },
                TestResources.CreateTestScoreInfo(),
            };

            var halfTime = new OsuModHalfTime
            {
                SpeedChange =
                {
                    Value = 0.99
                }
            };

            scores[1].Mods = new Mod[] { new OsuModHidden(), new OsuModDoubleTime(), new OsuModHardRock(), new OsuModFlashlight() };
            scores[2].Mods = new Mod[] { new OsuModHidden(), new OsuModDoubleTime(), new OsuModHardRock(), new OsuModFlashlight(), new OsuModClassic() };
            scores[3].Mods = new Mod[] { new OsuModHidden(), new OsuModDoubleTime(), new OsuModHardRock(), new OsuModFlashlight(), new OsuModClassic(), new OsuModDifficultyAdjust() };
            scores[4].Mods = scores[4].BeatmapInfo!.Ruleset.CreateInstance().CreateAllMods().ToArray();

            return scores;
        }
    }
}
