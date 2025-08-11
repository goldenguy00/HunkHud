using BepInEx;
using System.Collections;
using System.IO;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using HunkHud.Modules;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]

#pragma warning disable CS0618 // Type or member is obsolete
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
//[assembly: HG.Reflection.SearchableAttribute.OptIn]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HunkHud
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("bubbet.riskui", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class HunkHudMain : BaseUnityPlugin
    {
        public const string GUID = "com." + MODAUTHOR + "." + MODNAME;
        public const string MODAUTHOR = "public_ParticleSystem";
        public const string MODNAME = "HunkHud";
        public const string VERSION = "0.2.6";

        public static HunkHudMain instance { get; private set; }
        private static Harmony harm;
        private static PatchClassProcessor patcher;

        public void Awake()
        {
            instance = this;

            Log.Init(this.Logger);

            StartCoroutine(nameof(Load));

            harm = new Harmony(GUID);
            patcher = harm.CreateClassProcessor(typeof(Hooks));
        }

        private IEnumerator Load()
        {
            var request = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(this.Info.Location), "hunkhudassets"));
            yield return request;
            HudAssets.mainAssetBundle = request.assetBundle;
            patcher.Patch();
        }
    }
}
