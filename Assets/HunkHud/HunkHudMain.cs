using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HG;
using HunkHud.Components.UI;
using HunkHud.Modules;
using MaterialHud;
using RoR2;
using UnityEngine;

[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]

#pragma warning disable CS0618 // Type or member is obsolete
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
//[assembly: HG.Reflection.SearchableAttribute.OptIn]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HunkHud
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("bubbet.riskui", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class HunkHudMain : BaseUnityPlugin
    {
        public const string GUID = "com." + MODAUTHOR + "." + MODNAME;
        public const string MODAUTHOR = "public_ParticleSystem";
        public const string MODNAME = "HunkHud";
        public const string VERSION = "1.0.4";

        public static AssetBundle mainAssetBundle;

        public static ConfigFile activeConfig;

        private static Harmony harm;
        private static PatchClassProcessor patcher;

        public static HunkHudMain instance { get; private set; }
        public void Awake()
        {
            instance = this;

            Log.Init(this.Logger);

            LoadAssets();
            SetHooks();
        }

        private void SetHooks()
        {
            harm = new Harmony(GUID);
            patcher = harm.CreateClassProcessor(typeof(Hooks));
            patcher.Patch();
        }

        private void LoadAssets()
        {
            mainAssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "hunkui"));

            RiskUIPlugin._newHud = mainAssetBundle.LoadAsset<GameObject>("HunkUI");
            RiskUIPlugin._newClassicRunHud = mainAssetBundle.LoadAsset<GameObject>("HunkClassicRunInfoHudPanel");
            RiskUIPlugin._newSimulacrumHud = mainAssetBundle.LoadAsset<GameObject>("HunkSimulacrum");
            RiskUIPlugin._allyCard = mainAssetBundle.LoadAsset<GameObject>("HunkAllyCard");

            ItemCatalog.availability.CallWhenAvailable(CreateBandMap);
        }

        private void CreateBandMap()
        {
            CreateEntry("BandDisplay", "FireRing", "IceRing");
            CreateEntry("BandDisplayVoid", "ElementalRingVoid");
            CreateEntry("BandDisplayHealing", "ITEM_HEALING_BAND", "ITEM_BARRIER_BAND");
            CreateEntry("BandDisplayNova", "ITEM_NOVA_BAND");
            CreateEntry("BandDisplaySacrificial", "ITEM_SANDSWEPT_SACRIFICIAL_BAND");
        }

        private void CreateEntry(string prefabName, params string[] itemNames)
        {
            ItemIndex[] items = new ItemIndex[0];
            foreach (var name in itemNames)
            {
                var index = ItemCatalog.FindItemIndex(name);
                if (index != ItemIndex.None)
                {
                    HG.ArrayUtils.ArrayAppend(ref items, index);
                }
            }

            if (items.Length == 0)
                return;

            var displayPrefab = mainAssetBundle.LoadAsset<GameObject>(prefabName);
            if (displayPrefab == null)
                return;

            ArrayUtils.ArrayAppend(ref BandDisplayController.bandInfos, new BandDisplayController.BandDisplayInfo
            {
                prefabName = prefabName,
                displayPrefab = displayPrefab,
                itemsToDisplay = items
            });
        }

        //public static GameObject CreateHud() => _newHud;
        //public static GameObject CreateClassicRunHud() => _newClassicRunHud;
        //public static GameObject CreateSimulcrum() => _newSimulacrumHud;
        //public static GameObject CreateAllyCard() => _allyCard;
    }
}
