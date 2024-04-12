// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class PerformanceBroadcaster : StateBroadcaster<double>
    {
        private readonly GameplayState state;
        private readonly ScoreProcessor processor;
        private readonly PerformanceCalculator? performer;
        private ScoreInfo? score;
        private JudgementResult? lastResult;
        private List<TimedDifficultyAttributes>? attributes;

        public PerformanceBroadcaster(GameplayState state, ScoreProcessor processor, PerformanceCalculator? performer)
            : base(BroadcastID.PLAY_PP)
        {
            this.state = state;
            this.performer = performer;
            this.processor = processor;
            this.processor.NewJudgement += onJudgementChanged;
            this.processor.JudgementReverted += onJudgementChanged;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache cache)
        {
            if (state is null)
            {
                return;
            }

            var mods = state.Mods.Select(m => m.DeepClone()).ToArray();
            var work = new GameplayWorkingBeatmap(state.Beatmap);

            score = new ScoreInfo(state.Score.ScoreInfo.BeatmapInfo, state.Score.ScoreInfo.Ruleset) { Mods = mods };

            cache.GetTimedDifficultyAttributesAsync(work, state.Ruleset, mods, CancellationToken.None)
                .ContinueWith(task => Schedule(() =>
                {
                    attributes = task.GetResultSafely();

                    if (lastResult != null)
                    {
                        onJudgementChanged(lastResult);
                    }

                }), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void onJudgementChanged(JudgementResult result)
        {
            lastResult = result;

            if (performer is null || lastResult is null)
            {
                return;
            }

            var attrib = getAttributeAtTime(lastResult);

            if (attrib is null || score is null)
            {
                return;
            }

            processor.PopulateScore(score);

            Broadcast(performer.Calculate(score, attrib).Total);
        }

        private DifficultyAttributes? getAttributeAtTime(JudgementResult judgement)
        {
            if (attributes is null || attributes.Count == 0)
                return null;

            int attribIndex = attributes.BinarySearch(new TimedDifficultyAttributes(judgement.HitObject.GetEndTime(), null));
            if (attribIndex < 0)
                attribIndex = ~attribIndex - 1;

            return attributes[Math.Clamp(attribIndex, 0, attributes.Count - 1)].Attributes;
        }

        protected override void Dispose(bool isDisposing)
        {
            processor.NewJudgement -= onJudgementChanged;
            processor.JudgementReverted -= onJudgementChanged;
            base.Dispose(isDisposing);
        }

        private class GameplayWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap gameplayBeatmap;

            public GameplayWorkingBeatmap(IBeatmap gameplayBeatmap)
                : base(gameplayBeatmap.BeatmapInfo, null)
            {
                this.gameplayBeatmap = gameplayBeatmap;
            }

            public override IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods, CancellationToken cancellationToken)
                => gameplayBeatmap;

            protected override IBeatmap GetBeatmap() => gameplayBeatmap;

            public override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
