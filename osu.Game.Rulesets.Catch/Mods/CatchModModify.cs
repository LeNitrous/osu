// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModModify : ModModify, IModHasSettings
    {
        public Drawable[] CreateControls()
        {
            BindableFloat csControl = new BindableFloat();
            BindableFloat hpControl = new BindableFloat();
            BindableFloat arControl = new BindableFloat();

            csControl.BindTo(cs);
            hpControl.BindTo(hp);
            arControl.BindTo(ar);
            od.BindTo(ar);

            return new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = "Drain Rate",
                    Bindable = hpControl
                },
                new SettingsSlider<float>
                {
                    LabelText = "Fruit Size",
                    Bindable = csControl
                },
                new SettingsSlider<float>
                {
                    LabelText = "Approach Rate",
                    Bindable = arControl
                },
            };
        } 
    }
}