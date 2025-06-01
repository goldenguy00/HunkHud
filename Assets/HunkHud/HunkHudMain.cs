using BepInEx;
using System.Collections;
using System.IO;
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
        public const string VERSION = "0.2.5";

        public static HunkHudMain instance { get; private set; }

        public void Awake()
        {
            instance = this;

            Log.Init(this.Logger);
            PluginConfig.Init(this.Config);

            StartCoroutine(nameof(Load));

            Compat.Init();
        }

        private IEnumerator Load()
        {
            var request = AssetBundle.LoadFromFileAsync(Path.Combine(Path.GetDirectoryName(this.Info.Location), "hunkhudassets"));
            yield return request;
            HudAssets.mainAssetBundle = request.assetBundle;
            HudAssets.Init();
        }
    }
}
