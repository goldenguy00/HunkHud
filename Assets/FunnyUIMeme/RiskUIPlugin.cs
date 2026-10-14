using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using SimpleJSON;
using UnityEngine;
using Zio;
using ZioConfigFile;
using Path = System.IO.Path;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

[assembly: SearchableAttribute.OptIn]

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]

namespace MaterialHud
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("bubbet.zioriskofoptions", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("bubbet.riskui", "Risk UI", "1.6.0")]
    public class RiskUIPlugin : BaseUnityPlugin
    {
        public static GameObject _newHud;
        public static GameObject _newClassicRunHud;
        public static GameObject _newSimulacrumHud;
        public static GameObject _allyCard;
        public static GameObject BaseWaveUI;
        public static GameObject EnemyInfoPanel;

        public static readonly Dictionary<string, Sprite> DifficultyIconMap = new();
        public static ZioConfigEntry<Color> VoidColor;
        public static ZioConfigEntry<Color> InfusionColor;
        public static ZioConfigEntry<Color> VoidShieldColor;
        private string description;
        private Sprite icon;
        public static ZioConfigEntry<bool> Enabled;
        private Harmony harm;
        private PatchClassProcessor patcher;

        public static bool RiskOfOptionsEnabled => Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"); //"com.rune580.riskofoptions");
        public static RiskUIPlugin instance { get; private set; }
        public void Awake()
        {
            instance = this;
            Log.Init(Logger);
            
            LoadAssets();

            harm = new Harmony(Info.Metadata.GUID);
            patcher = new PatchClassProcessor(harm, typeof(NewHarmonyPatches));

            RoR2Application.onLoad += OnLoad;
        }
        private void LoadAssets()
        {
            var path = Path.GetDirectoryName(Info.Location);
            try
            {
                var jsonNode = JSON.Parse(File.OpenText(Path.Combine(path, "manifest.json")).ReadToEnd());
                description = jsonNode["description"].ToString();
                var iconStream = File.ReadAllBytes(Path.Combine(path, "icon.png"));
                var tex = new Texture2D(256, 256);
                tex.LoadImage(iconStream);
                icon = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
            }
            catch { }

            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(path, "riskui"));
            _newHud = assetBundle.LoadAsset<GameObject>("RiskUI");
            _newClassicRunHud = assetBundle.LoadAsset<GameObject>("MaterialClassicRunInfoHudPanel");
            _newSimulacrumHud = assetBundle.LoadAsset<GameObject>("MaterialSimulacrum");
            _allyCard = assetBundle.LoadAsset<GameObject>("MaterialAllyCard");
            BaseWaveUI = assetBundle.LoadAsset<GameObject>("MaterialDefaultWaveUI");
            EnemyInfoPanel = assetBundle.LoadAsset<GameObject>("MaterialMonsterItemInventory");

            CreateIconMap(assetBundle);
        }

        private void CreateIconMap(AssetBundle assetBundle)
        {
            DifficultyIconMap["SUNNY_NAME"] = assetBundle.LoadAsset<Sprite>("Sunny (More Difficulties)");
            DifficultyIconMap["RAINSOON_NAME"] = assetBundle.LoadAsset<Sprite>("ThunderStorm (More Difficulties)");
            DifficultyIconMap["HIFU_DIFFICULTY_NAME"] = assetBundle.LoadAsset<Sprite>("Inferno (Inferno)");
            DifficultyIconMap["DIFFICULTY_CONFIGURABLEDIFFICULTYMOD_NAME"] = assetBundle.LoadAsset<Sprite>("Pluviculture (ConfigurableDifficulty)");
            DifficultyIconMap["Mico27_DIFFICULTY_TROPICALSTORM_NAME"] = assetBundle.LoadAsset<Sprite>("Tropical Storm (Tropical Storm)");
            DifficultyIconMap["GROOVYDIFFICULTY_4_NAME"] = assetBundle.LoadAsset<Sprite>("Deluge (UntitledDifficultyMod)");
            DifficultyIconMap["CALYPSO_NAME"] = assetBundle.LoadAsset<Sprite>("Calypso (More Difficulties)");
            DifficultyIconMap["GROOVYDIFFICULTY_5_NAME"] = assetBundle.LoadAsset<Sprite>("Charybdis (UntitledDifficultyMod)");
            DifficultyIconMap["TEMPEST_NAME"] = assetBundle.LoadAsset<Sprite>("Tempest (More Difficulties)");
            DifficultyIconMap["SCYLLA_NAME"] = assetBundle.LoadAsset<Sprite>("Armageddon (More Difficulties)");

            DifficultyIconMap["DIFFICULTY_EASY_NAME"] = assetBundle.LoadAsset<Sprite>("Drizzle");
            DifficultyIconMap["DIFFICULTY_NORMAL_NAME"] = assetBundle.LoadAsset<Sprite>("Rainstorm");
            DifficultyIconMap["DIFFICULTY_HARD_NAME"] = assetBundle.LoadAsset<Sprite>("Monsoon");
        }

        private void OnLoad()
        {
            ConfigHelper.Init();

            Enabled = ConfigHelper.Bind("General", "Enabled", true, "Should the hud be replaced. Only updates on hud awake, so stage change and starting new runs.");
            Enabled.SettingChanged += EnabledChanged;
            EnabledChanged();
            
            VoidColor = ConfigHelper.Bind("Recoloring Player HealthBar", "Void Color", (Color) new Color32(181, 100, 189, 255), "Color of void, Void Fiends health bar.");
            InfusionColor = ConfigHelper.Bind("Recoloring Player HealthBar", "Infusion Color", (Color) new Color32(221, 44, 38, 255), "Color of infusion.");
            VoidShieldColor = ConfigHelper.Bind("Recoloring Player HealthBar", "Void Shield Color", (Color) new Color32(229, 127, 240, 255), "Color of void shield.");
            
            if (RiskOfOptionsEnabled)
                MakeRiskofOptions();
            
            foreach (var child in _newHud.GetComponentsInChildren<IConfigHandler>())
            {
                child.Startup();
            }
        }

        private void EnabledChanged()
        {
            if (Enabled.Value)
            {
                patcher.Patch();
                return;
            }
            harm.UnpatchSelf();
        }

        private void EnabledChanged(ZioConfigEntryBase arg1, object arg2, bool arg3) => EnabledChanged();

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void MakeRiskofOptions()
        {
            if(icon)
                ModSettingsManager.SetModIcon(icon);

            if(description != null)
                ModSettingsManager.SetModDescription(description);

            ModSettingsManager.AddOption(new GenericButtonOption("Report An Issue", "General", "If you find a bug in the mod, reporting an issue is the best way to ensure it gets fixed.","Open Link", () =>
            {
                Application.OpenURL("https://github.com/Bubbet/Risk-Of-Rain-Mods/issues/new");
            }));
            ModSettingsManager.AddOption(new GenericButtonOption("Donate to Bubbet", "General", "Donate to the programmer of RiskUI.","Open Link", () =>
            {
                Application.OpenURL("https://ko-fi.com/bubbet");
            }));
        }

        public static GameObject CreateHud() => _newHud;
        public static GameObject CreateClassicRunHud() => _newClassicRunHud;
        public static GameObject CreateSimulcrum() => _newSimulacrumHud;
        public static GameObject CreateAllyCard() => _allyCard;
    }
}