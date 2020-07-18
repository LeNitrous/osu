// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAlternate : Mod
    {
        public override string Name => "Alternate";
        public override string Acronym => "AL";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override string Description => "Never use the same key twice!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModAutoplay)).ToArray();
    }

    public abstract class ModAlternate<THitObject, TAction> : ModAlternate, IApplicableToDrawableRuleset<THitObject>
        where THitObject : HitObject
        where TAction : struct
    {
        public List<BreakPeriod> Breaks;
        public List<THitObject> HitObjects;
        public InputInterceptor Interceptor;

        public void ApplyToDrawableRuleset(DrawableRuleset<THitObject> drawableRuleset)
        {
            Breaks = drawableRuleset.Beatmap.Breaks;
            HitObjects = drawableRuleset.Beatmap.HitObjects;
            drawableRuleset.KeyBindingInputManager.Add(Interceptor = new InputInterceptor(this));
        }

        protected abstract bool OnPressed(TAction action);

        protected abstract bool OnReleased(TAction action);

        protected abstract void OnBreakEnd();

        protected virtual void OnInterceptorLoadComplete()
        {
        }

        public class InputInterceptor : Drawable, IKeyBindingHandler<TAction>
        {
            private readonly ModAlternate<THitObject, TAction> mod;

            public InputInterceptor(ModAlternate<THitObject, TAction> mod)
            {
                this.mod = mod;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                using(BeginAbsoluteSequence(0))
                {
                    foreach(BreakPeriod breakPeriod in mod.Breaks)
                    {
                        var hitObject = mod.HitObjects.First((h) => h.StartTime > breakPeriod.EndTime);
                        var window = hitObject.HitWindows.WindowFor(HitResult.Miss);
                        this.Delay(hitObject.StartTime - window).Schedule(() => mod.OnBreakEnd());
                    }
                }

                mod.OnInterceptorLoadComplete();
            }

            public bool OnPressed(TAction action) => mod.OnPressed(action);

            public void OnReleased(TAction action) => mod.OnReleased(action);
        }
    }
}