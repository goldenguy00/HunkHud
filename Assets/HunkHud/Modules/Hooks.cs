using HarmonyLib;
using HunkHud.Components.UI;
using MaterialHud;
using RoR2.UI;
using UnityEngine;
using HunkHud.Components;

namespace HunkHud.Modules
{
    [HarmonyPatch]
    public class Hooks
    {
        [HarmonyPatch(typeof(RiskUIPlugin), "OnLoad")]
        [HarmonyPostfix]
        private static void RiskUIPlugin_OnLoad()
        {
            HudAssets.UpdateRiskUI();

            HudAssets.ModifyRiskUI();
        }

        [HarmonyPatch(typeof(CrosshairController), nameof(CrosshairController.Awake))]
        [HarmonyPostfix]
        private static void CrosshairController_Awake(CrosshairController __instance)
        {
            __instance.GetOrAddComponent<DynamicCrosshair>();
        }

        [HarmonyPatch(typeof(HUD), nameof(HUD.Awake))]
        [HarmonyPostfix]
        private static void HUD_Awake(HUD __instance)
        {
            var childLoc = __instance.GetComponent<ChildLocator>();

            childLoc.FindChild("TopCenterCluster").FindOrInstantiate("ObjectiveGauge");
            childLoc.FindChild("CrosshairExtras").FindOrInstantiate("LuminousGauge");

            var hpBar = childLoc.FindChild("BottomLeftCluster").FindOrInstantiate("CustomHealthBar").transform.GetChild(0).GetComponent<CustomHealthBar>();

            __instance.healthBar = hpBar;
            __instance.levelText = hpBar.GetComponentInChildren<LevelText>();
            __instance.expBar = hpBar.GetComponentInChildren<ExpBar>();
        }

        [HarmonyPatch(typeof(NotificationAreaLoader), "genericNotificationPrefab", MethodType.Getter)]
        [HarmonyPostfix]
        private static bool NotificationAreaLoader_get_genericNotificationPrefab(ref GameObject __result)
        {
            __result = HudAssets.mainAssetBundle.LoadAsset<GameObject>("ItemNotification");
            return false;
        }
    }
}