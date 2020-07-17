using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAlternate : Mod
    {
        public override string Name => "Alternate";

        public override string Acronym => "AL";

        public override double ScoreMultiplier => 1;
    }

    public abstract class ModAlternate<THitObject, TAction> : ModAlternate, IApplicableToDrawableRuleset<THitObject>
        where THitObject : HitObject
        where TAction : struct
    {

        public void ApplyToDrawableRuleset(DrawableRuleset<THitObject> drawableRuleset)
        {
            drawableRuleset.Cursor.Add(new InputInterceptor(this));
        }

        protected abstract bool OnPressed(TAction action);

        protected abstract bool OnReleased(TAction action);

        private class InputInterceptor : Drawable, IKeyBindingHandler<TAction>
        {
            private readonly ModAlternate<THitObject, TAction> mod;

            public InputInterceptor(ModAlternate<THitObject, TAction> mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(TAction action) => mod.OnPressed(action);

            public void OnReleased(TAction action) => mod.OnReleased(action);
        }
    }
}