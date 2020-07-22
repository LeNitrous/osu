// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModAlternate : ModAlternate<TaikoHitObject, TaikoAction>
    {
        [SettingSource("Playstyle", "Preferred Alternate Playstyle")]
        public Bindable<Playstyle> Style { get; } = new Bindable<Playstyle>();

        private TaikoAction? lastAction1;
        private TaikoAction? lastAction2;
        private TaikoAction lastAction;
        private bool lastActionState;
        private double lastActionTime = 0;
        private const double strong_hit_window = 30;

        protected override void ResetActionStates()
        {
            lastAction1 = lastAction2 = null;
        }

        protected override bool OnPressed(TaikoAction action)
        {
            var blockInput = false;

            if (Style.Value == Playstyle.KDDK)
            {
                switch (action)
                {
                    case TaikoAction.LeftRim:
                    case TaikoAction.LeftCentre:
                        blockInput = action == lastAction1;
                        lastAction1 = action;
                        break;
                    case TaikoAction.RightRim:
                    case TaikoAction.RightCentre:
                        blockInput = action == lastAction2;
                        lastAction2 = action;
                        break;
                }
            }
            else
            {
                switch (action)
                {
                    case TaikoAction.LeftRim:
                    case TaikoAction.RightRim:
                        blockInput = action == lastAction1;
                        lastAction1 = action;
                        break;
                    case TaikoAction.LeftCentre:
                    case TaikoAction.RightCentre:
                        blockInput = action == lastAction2;
                        lastAction2 = action;
                        break;
                }
            }

            if (Interceptor.Time.Current - lastActionTime <= strong_hit_window)
            {
                if (lastAction == TaikoAction.LeftRim && action == TaikoAction.RightRim ||
                    lastAction == TaikoAction.RightRim && action == TaikoAction.LeftRim ||
                    lastAction == TaikoAction.LeftCentre && action == TaikoAction.RightCentre ||
                    lastAction == TaikoAction.RightCentre && action == TaikoAction.LeftCentre)
                    ResetActionStates();
            }

            lastAction = action;
            lastActionTime = Interceptor.Time.Current;
            lastActionState = blockInput;

            return blockInput;
        }

        protected override bool OnReleased(TaikoAction action) => false;

        public enum Playstyle
        {
            KDDK,
            KKDD
        }
    }
}
