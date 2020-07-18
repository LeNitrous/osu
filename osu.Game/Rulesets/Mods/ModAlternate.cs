using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
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

        protected abstract void OnInterceptorLoadComplete();

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
                mod.OnInterceptorLoadComplete();
            }

            public bool OnPressed(TAction action) => mod.OnPressed(action);

            public void OnReleased(TAction action) => mod.OnReleased(action);
        }
    }
}