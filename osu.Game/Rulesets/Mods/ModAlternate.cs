// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModAlternate : Mod
    {
        public override string Name => "Alternate";
        public override string Acronym => "AL";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override string Description => "Never use the same key twice!";
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
    }

    public abstract class ModAlternate<THitObject, TAction> : ModAlternate, IApplicableToDrawableRuleset<THitObject>
        where THitObject : HitObject
        where TAction : struct
    {
        protected readonly List<double> ResetTimestamps = new List<double>();
        protected List<THitObject> HitObjects;
        protected InputInterceptor Interceptor;
        protected PassThroughInputManager InputManager;
        private List<BreakPeriod> breaks;
        private double nextResetTime;
        private bool checkForBreaks = true;

        public void ApplyToDrawableRuleset(DrawableRuleset<THitObject> drawableRuleset)
        {
            breaks = drawableRuleset.Beatmap.Breaks;
            HitObjects = drawableRuleset.Beatmap.HitObjects;
            InputManager = drawableRuleset.KeyBindingInputManager;
            InputManager.Add(Interceptor = new InputInterceptor(this));

            foreach (BreakPeriod period in breaks)
            {
                var hitObject = HitObjects.First((h) => h.StartTime > period.EndTime);
                var window = hitObject.HitWindows.WindowFor(HitResult.Miss);
                ResetTimestamps.Add(hitObject.StartTime - window);
            }

            try
            {
                nextResetTime = ResetTimestamps.First();
            }
            catch
            {
                checkForBreaks = false;
            }
        }

        protected abstract bool OnPressed(TAction action);

        protected abstract bool OnReleased(TAction action);

        protected abstract void ResetActionStates();

        public void CheckBreaks(double time)
        {
            if (checkForBreaks && time > nextResetTime)
            {
                ResetActionStates();

                try
                {
                    nextResetTime = ResetTimestamps.First((stamp) => stamp > time);
                }
                catch
                {
                    checkForBreaks = false;
                }
            }
        }

        public class InputInterceptor : Drawable, IKeyBindingHandler<TAction>
        {
            private readonly ModAlternate<THitObject, TAction> mod;

            public InputInterceptor(ModAlternate<THitObject, TAction> mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(TAction action) => mod.OnPressed(action);

            public void OnReleased(TAction action) => mod.OnReleased(action);

            protected override void Update()
            {
                base.Update();
                mod.CheckBreaks(Time.Current);
            }
        }
    }
}
