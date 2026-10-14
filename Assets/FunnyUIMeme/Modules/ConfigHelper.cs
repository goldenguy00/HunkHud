using BepInEx.Configuration;
using JetBrains.Annotations;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Zio;
using ZioConfigFile;
using ZioRiskOfOptions;
using Color = UnityEngine.Color;

namespace MaterialHud
{
	public static class ConfigHelper
    {
        public static List<ZioConfigFile.ZioConfigFile> ConfigFiles = new();
        public static ZioConfigFile.ZioConfigFile ActiveConfigFile;
		public static Dictionary<string, ZioConfigEntryBase> Entries = new();
		public static ZioConfigEntry<int> _selectedProfile;
		private static IntSliderConfig option;
		public static ZioConfigFile.ZioConfigFile LoadedProfile;
        public static bool NeverBeforeInitialized;

        public static void Init()
		{
			ActiveConfigFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, "/RiskUI.cfg", true, RiskUIPlugin.instance);

            _selectedProfile = ActiveConfigFile.Bind("Profiles", "Selected Profile", 0, "Currently selected profile.");
			//_useZio = ActiveConfigFile.Bind("Profiles", "Use Zio", true, "Disable this setting to use local profile saving instead of cloud saves.");

            try
            {
                foreach (var path in RoR2Application.cloudStorage.EnumerateFiles(new UPath("/RiskUI/")))
                {
                    if (!path.IsNull && !path.Equals(UPath.Root) && !path.Equals(UPath.Empty))
                    {
						Log.Debug("Found file " + path.ToString());
                        var pathName = path.FullName.Replace("/RiskUI/", string.Empty);
                        if (Guid.TryParse(pathName, out var guid))
                        {
                            Log.Debug("Found zio config file " + guid.ToString());
                            ConfigFiles.Add(new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, $"/RiskUI/{guid}", true, RiskUIPlugin.instance));
                        }
                        else
                        {
                            Log.Error("Failed to parse " + pathName);
                        }
                    }
                    else
                    {
                        Log.Error("Invalid path " + path.ToString());
                    }
                }
            }
            catch (Exception e) { Log.Error(e); }

            if (ConfigFiles.Count < 1)
            {
                Log.Debug("Performing first time Zio init.");

                NeverBeforeInitialized = true;
                ConfigFiles.Add(new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, $"/RiskUI/{new Guid()}", true, RiskUIPlugin.instance));
            }

            _selectedProfile.Value = Math.Clamp(_selectedProfile.Value, 0, ConfigFiles.Count - 1);
			LoadedProfile = ConfigFiles[_selectedProfile.Value];

            if (RiskUIPlugin.RiskOfOptionsEnabled)
                MakeSelectedProfileOptions();

        }

		[MethodImpl(MethodImplOptions.NoInlining)]
        public static void MakeSelectedProfileOptions()
		{
			option = new IntSliderConfig
			{
				max = ConfigFiles.Count - 1,
				restartRequired = true
			};
			ModSettingsManager.AddOption(new ZioIntSliderOption(_selectedProfile, option));
			ModSettingsManager.AddOption(new GenericButtonOption("Add Profile", "Profiles", "Adds new profile. Select and restart game to use.", "Add", () =>
            {
                ConfigFiles.Add(new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, new UPath($"/RiskUI/{Guid.NewGuid()}"), true, RiskUIPlugin.instance));
                option.max = ConfigFiles.Count - 1;
				_selectedProfile.Value = ConfigFiles.Count - 1;
            }));
			ModSettingsManager.AddOption(new GenericButtonOption("Delete Profile", "Profiles", "Delete Selected Profile. Cannot delete active profile.", "Delete", () =>
			{
				var configToDelete = ConfigFiles.ElementAtOrDefault(_selectedProfile.Value);
				if (ConfigFiles.Count > 1 && configToDelete != null && configToDelete != LoadedProfile)
                {
                    RoR2Application.cloudStorage.DeleteFile(configToDelete.FilePath);
                    ConfigFiles.Remove(configToDelete);
					option.max = ConfigFiles.Count - 1;
					_selectedProfile.Value = ConfigFiles.IndexOf(LoadedProfile);
                }
            }));
		}

		public static ZioConfigEntry<T> Bind<T>(string category, string key, T defaultValue, string desc, [CanBeNull] Action<ZioConfigEntry<T>> firstSetupCallback = null, object riskOfOptionsExtra = null)
		{
			var entryKey = category + key;
			if (!Entries.ContainsKey(entryKey))
			{
				var _configEntry = ActiveConfigFile.Bind(category, key, defaultValue, desc);
				_configEntry.SettingChanged += UpdateSelectedProfile;
				
				foreach (var configFile in ConfigFiles) 
					configFile.Bind(category, key, defaultValue, desc);
				
				ApplyConfigProfile(_configEntry);
				
				if(RiskUIPlugin.RiskOfOptionsEnabled)
					FillRiskOfOptions(_configEntry, riskOfOptionsExtra);
				Entries.Add(entryKey, _configEntry);
				firstSetupCallback?.Invoke(_configEntry);
			}
			return (ZioConfigEntry<T>) Entries[entryKey];
		}

        private static void UpdateSelectedProfile(ZioConfigEntryBase configEntry, object _, bool __)
        {
            if (LoadedProfile == null)
            {
                Log.Error($"UpdateSelectedProfile: Profile num {_selectedProfile.Value} does not exist");
                return;
            }
            if (!LoadedProfile.TryGetValue(configEntry.Definition, out var zioEntry))
            {
                Log.Error($"UpdateSelectedProfile: Config entry {configEntry.Definition.Key} does not exist in profile num {_selectedProfile.Value}");
                return;
            }

            zioEntry.BoxedValue = configEntry.BoxedValue;
		}

		private static void ApplyConfigProfile(ZioConfigEntryBase configEntry)
        {
            if (LoadedProfile == null)
            {
                Log.Error($"ApplyConfigProfile: Profile num {_selectedProfile.Value} does not exist");
                return;
            }
            if (!LoadedProfile.TryGetValue(configEntry.Definition, out var zioEntry))
            {
                Log.Error($"ApplyConfigProfile: Config entry {configEntry.Definition.Key} does not exist in profile num {_selectedProfile.Value}");
                return;
            }

            if (NeverBeforeInitialized)
				zioEntry.BoxedValue = configEntry.BoxedValue;
			else
				configEntry.BoxedValue = zioEntry.BoxedValue;
		}

		private static void FillRiskOfOptions<T>(ZioConfigEntry<T> configEntry, object riskOfOptionsExtra = null)
		{
			switch (configEntry)
			{
				case ZioConfigEntry<string> entry:
					ModSettingsManager.AddOption(new ZioStringInputFieldOption(entry));
					break;
				case ZioConfigEntry<bool> entry:
					ModSettingsManager.AddOption(new ZioCheckBoxOption(entry));
					break;
				case ZioConfigEntry<int> entry:
					ModSettingsManager.AddOption(new ZioIntSliderOption(entry, new IntSliderConfig
					{
						max = (int) riskOfOptionsExtra
					}));
					break;
				case ZioConfigEntry<float> entry:
					ModSettingsManager.AddOption(new ZioSliderOption(entry, new SliderConfig
					{
						max = (float) riskOfOptionsExtra
					}));
					break;
				case ZioConfigEntry<Color> entry:
					ModSettingsManager.AddOption(new ZioColorOption(entry));
					break;
			}
		}
	}
}