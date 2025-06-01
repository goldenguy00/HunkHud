using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DanteMod.Content.Components;
using HunkHud.Modules;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud
{
    internal static class Compat
    {
        internal static bool ROOInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        internal static bool HunkInstalled => Chainloader.PluginInfos.ContainsKey("com.rob.Hunk");
        internal static bool DanteInstalled => Chainloader.PluginInfos.ContainsKey("com.rob.Dante");

        internal static bool RiskUIEnabled
        {
            get
            {
                if (Chainloader.PluginInfos.ContainsKey("bubbet.riskui"))
                    return GetRiskUIEnabled();
                return false;
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool GetRiskUIEnabled()
        {
            return MaterialHud.RiskUIPlugin.Enabled.Value;
        }

        internal static void Init()
        {
            HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void HUD_onHudTargetChangedGlobal(HUD hud)
        {
            if (!hud.targetMaster || hud.targetMaster.backupBodyIndex == RoR2.BodyIndex.None || !hud.targetMaster.hasAuthority)
                return;

            if (hud.targetMaster.backupBodyIndex == RoR2.BodyCatalog.FindBodyIndex("RobDanteBody"))
            {
                AddDanteCompat(hud);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddDanteCompat(HUD hud)
        {
            var childLoc = hud.GetComponent<ChildLocator>();
            Transform cluster = childLoc.FindChild("BottomLeftCluster");
            if (!cluster.Find("DevilTriggerGauge"))
            {
                GameObject gameObject = GameObject.Instantiate(HudAssets.mainAssetBundle.LoadAsset<GameObject>("DevilTriggerGauge"), cluster);
                gameObject.name = "DevilTriggerGauge";
                Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
                Object.Destroy(gameObject.GetComponentInChildren<RoR2.UI.LevelText>());
                Object.Destroy(gameObject.GetComponentInChildren<RoR2.UI.ExpBar>());
                DevilTriggerGauge devilTriggerGauge = gameObject.AddComponent<DevilTriggerGauge>();
                devilTriggerGauge.targetHUD = hud;
                devilTriggerGauge.fillRectTransform = gameObject.transform.Find("ExpBarRoot").GetChild(0).GetChild(0).GetComponent<RectTransform>();
                devilTriggerGauge.transform.Find("LevelDisplayRoot/ValueText").gameObject.SetActive(value: false);
                devilTriggerGauge.transform.Find("LevelDisplayRoot/PrefixText").GetComponent<RoR2.UI.LanguageTextMeshController>().token = "";
                devilTriggerGauge.transform.Find("ExpBarRoot").GetChild(0).GetComponent<Image>().enabled = true;
                devilTriggerGauge.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

                RectTransform component = devilTriggerGauge.GetComponent<RectTransform>();
                component.anchorMax = new Vector2(0.01f, 1f);
                component.anchoredPosition = new Vector2(0f, -310f);
            }
        }
    }
}
