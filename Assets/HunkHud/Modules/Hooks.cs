using HarmonyLib;
using MaterialHud;
using RoR2.UI;
using UnityEngine;
using HunkHud.Components;

namespace HunkHud.Modules
{
    [HarmonyPatch]
    public class Hooks
    {
        [HarmonyPatch(typeof(CrosshairController), nameof(CrosshairController.Awake))]
        [HarmonyPostfix]
        private static void CrosshairController_Awake(CrosshairController __instance)
        {
            __instance.GetOrAddComponent<DynamicCrosshair>();
        }

        [HarmonyPatch(typeof(RiskUIPlugin), nameof(RiskUIPlugin.CreateHud))]
        [HarmonyPostfix]
        private static bool RiskUIPlugin_CreateHud(ref GameObject __result)
        {
            __result = HudAssets.mainAssetBundle.LoadAsset<GameObject>("RiskUIHunk");
            return false;
        }
    }
}