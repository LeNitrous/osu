// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public class EditorReplayPlayer : ReplayPlayer
    {
        private readonly Editor editor;
        private readonly EditorState editorState;

        [Resolved]
        private MusicController musicController { get; set; }

        public EditorReplayPlayer(Editor editor, Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore)
            : base(createScore, new PlayerConfiguration { ShowResults = false })
        {
            this.editor = editor;
            editorState = editor.GetState();
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new MasterGameplayClockContainer(beatmap, editorState.Time, true);

        protected override HUDOverlay CreateHUDOverlay(DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods)
            => new EditorHUDOverlay(GameplayClockContainer, drawableRuleset, mods);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ScoreProcessor.HasCompleted.BindValueChanged(completed =>
            {
                if (completed.NewValue)
                    Scheduler.AddDelayed(this.Exit, RESULTS_DISPLAY_DELAY);
            });
        }

        public override bool OnExiting(IScreen next)
        {
            musicController.Stop();

            editorState.Time = GameplayClockContainer.CurrentTime;
            editor.RestoreState(editorState);
            return base.OnExiting(next);
        }
    }
}
