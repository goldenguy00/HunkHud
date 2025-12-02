using HarmonyLib;
using MaterialHud;
using RoR2.UI;
using UnityEngine;
using HunkHud.Components;
using HG;

namespace HunkHud.Modules
{
    [HarmonyPatch]
    public class Hooks
    {
        [HarmonyPatch(typeof(CrosshairController), nameof(CrosshairController.Awake))]
        [HarmonyPostfix]
        private static void CrosshairController_Awake(CrosshairController __instance)
        {
            __instance.EnsureComponent<DynamicCrosshair>();
        }
    }
}