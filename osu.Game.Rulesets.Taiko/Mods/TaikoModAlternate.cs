// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModAlternate : ModAlternate<TaikoHitObject, TaikoAction>
    {
        private TaikoAction? lastRimAction;
        private TaikoAction? lastCentreAction;
        private TaikoHitObject nextHitObject;
        private bool requestNextHitObject = true;

        protected override void ResetActionStates()
        {
            lastCentreAction = lastRimAction = null;
        }

        protected override void OnInterceptorUpdate(double time)
        {
            if (requestNextHitObject)
            {
                nextHitObject = HitObjects.FirstOrDefault((h) => h.StartTime > time);
                requestNextHitObject = false;
            }

            base.OnInterceptorUpdate(time);
        }

        protected override bool OnPressed(TaikoAction action)
        {
            if (nextHitObject?.IsStrong ?? false)
            {
                ResetActionStates();
                return false;
            }

            if (action == TaikoAction.LeftRim || action == TaikoAction.RightRim)
            {
                if (lastRimAction == action)
                    return true;

                lastRimAction = action;
                return false;
            }

            if (action == TaikoAction.LeftCentre || action == TaikoAction.RightCentre)
            {
                if (lastCentreAction == action)
                    return true;

                lastCentreAction = action;
                return false;
            }

            requestNextHitObject = true;
            return false;
        }

        protected override bool OnReleased(TaikoAction action) => false;
    }
}
