// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModModify : ModModify, IModHasSettings
    {
        public Drawable[] CreateControls()
        {
            BindableFloat odControl = new BindableFloat();
            BindableFloat hpControl = new BindableFloat();

            odControl.BindTo(od);
            hpControl.BindTo(hp);

            return new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = "Drain Rate",
                    Bindable = hpControl
                },
                new SettingsSlider<float>
                {
                    LabelText = "Overall Difficulty",
                    Bindable = odControl
                },
            };
        } 
    }
}