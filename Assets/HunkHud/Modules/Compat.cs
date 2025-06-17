using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using DanteMod;
using DanteMod.Content.Components;
using DanteMod.Content.Survivors;
using DanteMod.Modules;
using HunkHud.Components.UI;
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
        }
    }
}
