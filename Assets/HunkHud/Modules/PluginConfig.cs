using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace HunkHud.Modules
{
    public static class PluginConfig
    {
        private static ConfigFile cfg;

        public static ConfigEntry<bool> customHUD;
        public static ConfigEntry<bool> dynamicCustomHUD;
        public static ConfigEntry<bool> disableChat;

        public static ConfigEntry<bool> dynamicCrosshair;
        public static ConfigEntry<bool> fancyShield;
        public static ConfigEntry<bool> shieldBubble;
        public static ConfigEntry<float> vignetteStrength;

        public static ConfigEntry<bool> overTheShoulderCamera;
        public static ConfigEntry<bool> overTheShoulderCameraGlobal;
        public static ConfigEntry<float> cameraSmoothSpeed;

        internal static void Init(ConfigFile config)
        {
            cfg = config;

            #region 01 - General
            customHUD = cfg.BindAndOptions("01 - General",
                "Custom HUD",
                defaultValue: true,
                "Set to true to enable a custom HUD tailored just for HUNK.");

            dynamicCustomHUD = cfg.BindAndOptions("01 - General",
                "Dynamic Custom HUD",
                defaultValue: true,
                "Set to false to keep HUNK's custom HUD on the screen at all times.");

            /*disableChat = cfg.BindAndOptions("01 - General",
                "Disable Chat In SinglePlayer",
                defaultValue: false,
                "Set to true to hide the chatbox in singleplayer.");*/
            #endregion

            #region 02 - Misc
            /*
            dynamicCrosshair = cfg.BindAndOptions("02 - Misc",
                "Dynamic Crosshair",
                defaultValue: true,
                "If set to false, will no longer highlight HUNK's crosshair when hovering over entities.");

            fancyShield = cfg.BindAndOptions("02 - Misc",
                "Fancy Shield",
                defaultValue: true,
                "Set to false to disable the custom shield overlay and use the ugly vanilla overlay on HUNK only.",
                restartRequired: true);

            shieldBubble = cfg.BindAndOptions("02 - Misc",
                "Shield Bubble",
                defaultValue: false,
                "Set to true to enable a custom shield bubble for HUNK. Only works with Fancy Shield enabled!",
                restartRequired: true);*/
            #endregion

            #region 03 - Camera
            vignetteStrength = cfg.BindAndOptionsSlider("03 - Camera",
                "Vignette Strength",
                defaultValue: 0f,
                "Adds a subtle vignette to the screen to put more focus on the center.",
                min: 0f, max: 1f);
            /*
            overTheShoulderCamera = cfg.BindAndOptions("03 - Camera",
                "Enable Over The Shoulder Camera",
                defaultValue: false,
                "Set to true to position the camera in a traditional over the shoulder view.");

            overTheShoulderCameraGlobal = cfg.BindAndOptions("03 - Camera",
                "Enable Over The Shoulder Camera (Global)",
                defaultValue: false,
                "Set to true to apply HUNK's camera controls to all survivors. Expect jank.");

            cameraSmoothSpeed = cfg.BindAndOptionsSlider("03 - Camera",
                "Camera Smoothing Speed",
                defaultValue: 24f,
                "Controls the sensitivity of HUNK's custom camera interpolation - higher values are faster. Set to 0 to disable interpolation and camera bobbing.",
                min: 0f, max: 80f);*/
            #endregion
        }
        
        public static void InitROO(Sprite modSprite, string modDescription)
        {
            ModSettingsManager.SetModIcon(modSprite);
            ModSettingsManager.SetModDescription(modDescription);
        }

        public static ConfigEntry<T> BindAndOptions<T>(this ConfigFile config, string section, string name, T defaultValue, string description = "", bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += $" (Default: {defaultValue})";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = config.Bind(section, name, defaultValue, description);

            if (HunkHudMain.ROOInstalled)
                TryRegisterOption(configEntry, restartRequired);

            return configEntry;
        }

        public static ConfigEntry<float> BindAndOptionsSlider(this ConfigFile config, string section, string name, float defaultValue, string description = "", float min = 0f, float max = 20f, bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += $" (Default: {defaultValue})";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = config.Bind(section, name, defaultValue, new ConfigDescription(description, new AcceptableValueRange<float>(min, max)));

            if (HunkHudMain.ROOInstalled)
                TryRegisterOptionSlider(configEntry, min, max, restartRequired);

            return configEntry;
        }

        public static ConfigEntry<int> BindAndOptionsSlider(this ConfigFile config, string section, string name, int defaultValue, string description = "", int min = 0, int max = 20, bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += $" (Default: {defaultValue})";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = config.Bind(section, name, defaultValue, new ConfigDescription(description, new AcceptableValueRange<int>(min, max)));

            if (HunkHudMain.ROOInstalled)
                TryRegisterOptionSlider(configEntry, min, max, restartRequired);

            return configEntry;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void TryRegisterOption<T>(ConfigEntry<T> entry, bool restartRequired)
        {
            if (entry is ConfigEntry<float>)
            {
                ModSettingsManager.AddOption(new SliderOption(entry as ConfigEntry<float>, new SliderConfig
                {
                    min = 0f,
                    max = 20f,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), HunkHudMain.GUID, HunkHudMain.MODNAME);
            }
            if (entry is ConfigEntry<int>)
                ModSettingsManager.AddOption(new IntSliderOption(entry as ConfigEntry<int>, restartRequired), HunkHudMain.GUID, HunkHudMain.MODNAME);
            if (entry is ConfigEntry<bool>)
                ModSettingsManager.AddOption(new CheckBoxOption(entry as ConfigEntry<bool>, restartRequired), HunkHudMain.GUID, HunkHudMain.MODNAME);
            if (entry is ConfigEntry<KeyboardShortcut>)
                ModSettingsManager.AddOption(new KeyBindOption(entry as ConfigEntry<KeyboardShortcut>, restartRequired), HunkHudMain.GUID, HunkHudMain.MODNAME);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void TryRegisterOptionSlider(ConfigEntry<int> entry, int min, int max, bool restartRequired)
        {
            ModSettingsManager.AddOption(new IntSliderOption(entry, new IntSliderConfig
            {
                min = min,
                max = max,
                formatString = "{0:0.00}",
                restartRequired = restartRequired
            }), HunkHudMain.GUID, HunkHudMain.MODNAME);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void TryRegisterOptionSlider(ConfigEntry<float> entry, float min, float max, bool restartRequired)
        {
            ModSettingsManager.AddOption(new SliderOption(entry, new SliderConfig
            {
                min = min,
                max = max,
                FormatString = "{0:0.00}",
                restartRequired = restartRequired
            }), HunkHudMain.GUID, HunkHudMain.MODNAME);
        }
    }
}
