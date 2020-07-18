// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModAlternate : ModAlternate<TaikoHitObject, TaikoAction>
    {
        private TaikoAction? lastRimAction;
        private TaikoAction? lastCentreAction;

        protected override void OnBreakEnd()
        {
            resetStates();
        }

        protected override void OnInterceptorLoadComplete()
        {
            Drawable interceptor = Interceptor as Drawable;

            using(Interceptor.BeginAbsoluteSequence(0))
            {
                foreach (TaikoHitObject hitObject in HitObjects)
                {
                    if (!hitObject.IsStrong) continue;

                    var window = hitObject.HitWindows.WindowFor(HitResult.Miss);
                    interceptor.Delay(hitObject.StartTime - window).Schedule(() => resetStates());
                    interceptor.Delay(hitObject.StartTime + window).Schedule(() => resetStates());

                    foreach (TaikoHitObject h in hitObject.NestedHitObjects)
                    {
                        interceptor.Delay(h.StartTime - 50).Schedule(() => resetStates());
                        interceptor.Delay(h.StartTime + 50).Schedule(() => resetStates());
                    }
                }
            }
        }

        protected override bool OnPressed(TaikoAction action)
        {
            if (action == TaikoAction.LeftRim || action == TaikoAction.RightRim)
            {
                if (lastRimAction != null && lastRimAction == action)
                    return true;

                lastRimAction = action;
                return false;
            }

            if (action == TaikoAction.LeftCentre || action == TaikoAction.RightCentre)
            {
                if (lastCentreAction != null && lastCentreAction == action)
                    return true;

                lastCentreAction = action;
                return false;
            }

            return false;
        }

        protected override bool OnReleased(TaikoAction action)
        {
            return false;
        }

        private void resetStates()
        {
            lastCentreAction = lastRimAction = null;
        }
    }
}
