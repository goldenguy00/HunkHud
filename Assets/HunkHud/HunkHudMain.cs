using BepInEx;
using BepInEx.Bootstrap;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using HunkHud.Modules;

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
        public const string VERSION = "0.1.7";

        public static HunkHudMain instance { get; private set; }

        public static bool ROOInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        public static bool HunkInstalled => Chainloader.PluginInfos.ContainsKey("com.rob.Hunk");

        public static bool RiskUIEnabled
        {
            get
            {
                if (Chainloader.PluginInfos.ContainsKey("bubbet.riskui"))
                    return GetRiskUIEnabled();
                return false;
            }
        }

        public void Awake()
        {
            instance = this;

            Log.Init(this.Logger);
            PluginConfig.Init(this.Config);

            StartCoroutine(nameof(Load));
        }

        private IEnumerator Load()
        {
            var request = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(this.Info.Location), "hunkhudassets"));
            yield return request;
            HudAssets.mainAssetBundle = request.assetBundle;
            HudAssets.Init();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool GetRiskUIEnabled()
        {
            return MaterialHud.RiskUIPlugin.Enabled.Value;
        }
    }
}
