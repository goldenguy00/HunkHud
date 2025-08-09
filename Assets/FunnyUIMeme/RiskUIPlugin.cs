using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using RoR2.Scripts.GameBehaviors.UI;
using RoR2.UI;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
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
	[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("bubbet.zioriskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInPlugin("bubbet.riskui", "Risk UI", "1.4.0")]
	public class RiskUIPlugin : BaseUnityPlugin
	{
		internal static GameObject _newHud;
		internal static GameObject _newClassicRunHud;
		internal static GameObject _newSimulacrumHud;
		internal static GameObject _allyCard;
		internal static GameObject BaseWaveUI;
		internal static GameObject EnemyInfoPanel;

		public static readonly Dictionary<string, Sprite> DifficultyIconMap = new();
		public static ZioConfigEntry<Color> VoidColor;
		public static ZioConfigEntry<Color> InfusionColor;
		public static ZioConfigEntry<Color> VoidShieldColor;
		public static ZioConfigFile.ZioConfigFile ConfigFile;
		private string description;
		private Sprite icon;
		public static ZioConfigEntry<bool> Enabled;
		private Harmony harm;
		private PatchClassProcessor patcher;
		public static readonly List<ZioConfigFile.ZioConfigFile> ConfigFiles = new();
		public static bool NeverBeforeInitialized;

		public static bool RiskOfOptionsEnabled => Chainloader.PluginInfos.ContainsKey("bubbet.zioriskofoptions"); //"com.rune580.riskofoptions");
		public void Awake()
		{
            Log.Init(Logger);
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

			harm = new Harmony(Info.Metadata.GUID);
			patcher = new PatchClassProcessor(harm, typeof(NewHarmonyPatches));

			RoR2Application.onLoad += OnLoad;

            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(path, "riskui"));

            EditShit(assetBundle);
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
        /*
        public static void EditShit()
        {
            _newHud.GetComponent<HUD>().contextManager.inspectGlyphTMP = LegacyResourcesAPI.Load<GameObject>("Prefabs/HUDSimple").GetComponent<HUD>().contextManager.inspectGlyphTMP;
            			this.alphaStartPercent = base.AnimParams.GetParam("enterAlphaStartPercent", 0.25f);
			this.alphaEndPercent = base.AnimParams.GetParam("enterAlphaEndPercent", 0.5f);
			this.heightStartPercent = base.AnimParams.GetParam("enterHeightStartPercent", 0f);
			this.heightEndPercent = base.AnimParams.GetParam("enterHeightEndPercent", 0.25f);

            var hud = RiskUIPlugin._newHud.GetComponent<HUD>();

            var skillIconRoot = assetBundle.LoadAsset<GameObject>("SkillIconRoot");
            var hpBarSub = assetBundle.LoadAsset<GameObject>("HealthBarSub");
            var chatbox = assetBundle.LoadAsset<GameObject>("MaterialChatbox");
            var artifactIcon = assetBundle.LoadAsset<GameObject>("MaterialArtifactStatusIcon");
            var objectivePanel = assetBundle.LoadAsset<GameObject>("MaterialObjectiveStrip");
            var skillIcon = hud.skillIcons;
            var buffIcon = hud.buffDisplay.buffIconPrefab;
            var itemDisplay = hud.itemInventoryDisplay.gameObject;
            var equipIcon = hud.equipmentIcons;

            objectivePanel.TryAddComponent<UIInputPassthrough>();
            objectivePanel.TryAddComponent<MPEventSystemLocator>();
            objectivePanel.TryAddComponent<ObjectiveStripAnimationParams>().Params =
            [
                new() { Param = "enterAlphaStartPercent", Value = 0.25f },
                new() { Param = "enterAlphaEndPercent", Value = 0.5f },
                new() { Param = "enterHeightStartPercent", Value = 0f },
                new() { Param = "enterHeightEndPercent", Value = 0.25f },
            ];

            if (objectivePanel.transform.Find("Checkbox"))
                objectivePanel.transform.Find("Checkbox").SetParent(objectivePanel.transform.Find("Label"));

            buffIcon.TryAddComponent<UIInputPassthrough>();
            buffIcon.TryAddComponent<MPEventSystemLocator>();
            var b = buffIcon.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            //equip icons
            equipIcon[0].gameObject.TryAddComponent<MPEventSystemLocator>();
            equipIcon[0].gameObject.TryAddComponent<UIInputPassthrough>();
            b = equipIcon[0].gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            equipIcon[1].gameObject.TryAddComponent<MPEventSystemLocator>();
            equipIcon[1].gameObject.TryAddComponent<UIInputPassthrough>();
            b = equipIcon[1].gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            // skill icons
            skillIcon[0].gameObject.TryAddComponent<MPEventSystemLocator>();
            skillIcon[0].gameObject.TryAddComponent<UIInputPassthrough>();
            b = skillIcon[0].gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            skillIcon[1].gameObject.TryAddComponent<MPEventSystemLocator>();
            skillIcon[1].gameObject.TryAddComponent<UIInputPassthrough>();
            b = skillIcon[1].gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            skillIcon[2].gameObject.TryAddComponent<MPEventSystemLocator>();
            skillIcon[2].gameObject.TryAddComponent<UIInputPassthrough>();
            b = skillIcon[2].gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            skillIcon[3].gameObject.TryAddComponent<MPEventSystemLocator>();
            skillIcon[3].gameObject.TryAddComponent<UIInputPassthrough>();
            b = skillIcon[3].gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            itemDisplay.TryAddComponent<UIInputPassthrough>();
            itemDisplay.TryAddComponent<MPEventSystemLocator>();
            b = itemDisplay.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            hud.scoreboardPanel.TryAddComponent<UIInputPassthrough>();
            hud.scoreboardPanel.TryAddComponent<MPEventSystemLocator>();

            var scoreboard = hud.scoreboardPanel.GetComponent<ScoreboardController>();
            scoreboard.stripPrefab.TryAddComponent<UIInputPassthrough>();

            var strip = scoreboard.stripPrefab.GetComponent<ScoreboardStrip>();
            strip.eventSystemLocator = scoreboard.stripPrefab.TryAddComponent<MPEventSystemLocator>();

            strip.equipmentIcon.gameObject.TryAddComponent<UIInputPassthrough>();
            strip.equipmentIcon.gameObject.TryAddComponent<MPEventSystemLocator>();
            b = strip.gameObject.TryAddComponent<MPButton>();
            b.disableGamepadClick = true;
            b.disablePointerClick = true;

            chatbox.TryAddComponent<UIInputPassthrough>();
            chatbox.TryAddComponent<MPEventSystemLocator>();

            artifactIcon.TryAddComponent<UIInputPassthrough>();
            artifactIcon.TryAddComponent<MPEventSystemLocator>();

            hpBarSub.TryAddComponent<UIInputPassthrough>();
            hpBarSub.TryAddComponent<MPEventSystemLocator>();

            skillIconRoot.TryAddComponent<UIInputPassthrough>();
            skillIconRoot.TryAddComponent<MPEventSystemLocator>();

            RiskUIPlugin.EnemyInfoPanel.TryAddComponent<UIInputPassthrough>();
            RiskUIPlugin.EnemyInfoPanel.TryAddComponent<MPEventSystemLocator>();

            RiskUIPlugin._allyCard.TryAddComponent<UIInputPassthrough>();
            RiskUIPlugin._allyCard.TryAddComponent<MPEventSystemLocator>();

            RiskUIPlugin._newClassicRunHud.TryAddComponent<UIInputPassthrough>();
            RiskUIPlugin._newClassicRunHud.TryAddComponent<MPEventSystemLocator>();

            RiskUIPlugin._newSimulacrumHud.TryAddComponent<UIInputPassthrough>();
            RiskUIPlugin._newSimulacrumHud.TryAddComponent<MPEventSystemLocator>();

            RiskUIPlugin.BaseWaveUI.TryAddComponent<UIInputPassthrough>();
            RiskUIPlugin.BaseWaveUI.TryAddComponent<MPEventSystemLocator>();
        }*/

        private void EditShit(AssetBundle assetBundle)
        {
            _newHud = assetBundle.LoadAsset<GameObject>("RiskUI");
            var hud = _newHud.GetComponent<HUD>();

            _newClassicRunHud = assetBundle.LoadAsset<GameObject>("MaterialClassicRunInfoHudPanel");
            _allyCard = assetBundle.LoadAsset<GameObject>("MaterialAllyCard");
            EnemyInfoPanel = assetBundle.LoadAsset<GameObject>("MaterialMonsterItemInventory");
            _newSimulacrumHud = assetBundle.LoadAsset<GameObject>("MaterialSimulacrum");
            BaseWaveUI = assetBundle.LoadAsset<GameObject>("MaterialDefaultWaveUI");
            /*
            var skillIconRoot = assetBundle.LoadAsset<GameObject>("SkillIconRoot");
            var hpBarSub = assetBundle.LoadAsset<GameObject>("HealthBarSub");
            var chatbox = assetBundle.LoadAsset<GameObject>("MaterialChatbox");
            var artifactIcon = assetBundle.LoadAsset<GameObject>("MaterialArtifactStatusIcon");
            var objectivePanel = assetBundle.LoadAsset<GameObject>("MaterialObjectiveStrip");
            var skillIcon = hud.skillIcons;
            var buffIcon = hud.buffDisplay.buffIconPrefab;
            var itemDisplay = hud.itemInventoryDisplay.gameObject;
            var equipIcon = hud.equipmentIcons;*/

            var skillIconRoot = assetBundle.LoadAsset<GameObject>("SkillIconRoot");
            var hpBarSub = assetBundle.LoadAsset<GameObject>("HealthBarSub");
            var chatbox = assetBundle.LoadAsset<GameObject>("MaterialChatbox");
            var artifactIcon = assetBundle.LoadAsset<GameObject>("MaterialArtifactStatusIcon");
            var buffIcon = assetBundle.LoadAsset<GameObject>("MaterialBuffIcon");
            var itemIcon = assetBundle.LoadAsset<GameObject>("ItemIcon");
            var equipIcon = assetBundle.LoadAsset<GameObject>("EquipIcon");
            var stripContainer = assetBundle.LoadAsset<GameObject>("StripContainer");
            var objectivePanel = assetBundle.LoadAsset<GameObject>("MaterialObjectiveStrip");

            objectivePanel.transform.Find("Checkbox").SetParent(objectivePanel.transform.Find("Label"));
            var param = objectivePanel.gameObject.AddComponent<ObjectiveStripAnimationParams>();
            param.Params = new List<ObjectiveStripAnimationParam>
            {
                new() { Param = "enterAlphaStartPercent", Value = 0.25f },
                new() { Param = "enterAlphaEndPercent", Value = 0.5f },
                new() { Param = "enterHeightStartPercent", Value = 0f },
                new() { Param = "enterHeightEndPercent", Value = 0.25f },
            };

            equipIcon.TryAddComponent<MPEventSystemLocator>();
            equipIcon.TryAddComponent<UIInputPassthrough>();

            itemIcon.TryAddComponent<UIInputPassthrough>();
            itemIcon.TryAddComponent<MPEventSystemLocator>();
            itemIcon.AddComponent<InspectPanelLocator>();

            buffIcon.TryAddComponent<UIInputPassthrough>();
            buffIcon.TryAddComponent<MPEventSystemLocator>();

            //stripContainer.GetComponentInChildren<>.TryAddComponent<UIInputPassthrough>();
            //stripContainer.TryAddComponent<MPEventSystemLocator>();
            var scoreboard = hud.scoreboardPanel.GetComponent<ScoreboardController>();
            scoreboard.stripPrefab.AddComponent<UIInputPassthrough>();
            scoreboard.stripPrefab.TryAddComponent<MPEventSystemLocator>();


            objectivePanel.TryAddComponent<UIInputPassthrough>();
            objectivePanel.TryAddComponent<MPEventSystemLocator>();

            chatbox.TryAddComponent<UIInputPassthrough>();
            chatbox.TryAddComponent<MPEventSystemLocator>();

            artifactIcon.TryAddComponent<UIInputPassthrough>();
            artifactIcon.TryAddComponent<MPEventSystemLocator>();

            hpBarSub.TryAddComponent<UIInputPassthrough>();
            hpBarSub.TryAddComponent<MPEventSystemLocator>();

            skillIconRoot.TryAddComponent<UIInputPassthrough>();
            skillIconRoot.TryAddComponent<MPEventSystemLocator>();

            EnemyInfoPanel.TryAddComponent<UIInputPassthrough>();
            EnemyInfoPanel.TryAddComponent<MPEventSystemLocator>();

            _allyCard.TryAddComponent<UIInputPassthrough>();
            _allyCard.TryAddComponent<MPEventSystemLocator>();

            _newClassicRunHud.TryAddComponent<UIInputPassthrough>();
            _newClassicRunHud.TryAddComponent<MPEventSystemLocator>();

            _newSimulacrumHud.TryAddComponent<UIInputPassthrough>();
            _newSimulacrumHud.TryAddComponent<MPEventSystemLocator>();

            BaseWaveUI.TryAddComponent<UIInputPassthrough>();
            BaseWaveUI.TryAddComponent<MPEventSystemLocator>();
        }

		private void OnLoad()
		{
			ConfigFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, "/RiskUI.cfg", true, this);
			
			foreach (var path in RoR2Application.cloudStorage.EnumeratePaths(new UPath("/RiskUI/")))
			{
				ConfigFiles.Add(new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, path, true, this));
			}

			if (ConfigFiles.Count < 1)
			{
				NeverBeforeInitialized = true;
				ConfigFiles.Add(new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, $"/RiskUI/{new Guid()}",
					true, this));
			}

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