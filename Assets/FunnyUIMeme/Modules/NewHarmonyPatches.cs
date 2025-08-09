using System;
using System.Linq;
using HarmonyLib;
using MaterialHud;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace MaterialHud
{
    [HarmonyPatch]
    public static class NewHarmonyPatches
    {
        [HarmonyILManipulator, HarmonyPatch(typeof(CameraRigController), nameof(CameraRigController.UpdateHUD))]
        public static void ReplaceHUD(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(out _),
                x => x.MatchCallOrCallvirt(out _),
                x => x.MatchStsfld(typeof(CameraRigController), nameof(CameraRigController.HudPrefab))))
            {
                c.Emit(OpCodes.Call, AccessTools.Method(typeof(RiskUIPlugin), nameof(RiskUIPlugin.CreateHud)));
                c.RemoveRange(2);

                if (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdsfld(typeof(CameraRigController), nameof(CameraRigController.HudPrefab))))
                {
                    c.Emit(OpCodes.Call, AccessTools.Method(typeof(RiskUIPlugin), nameof(RiskUIPlugin.CreateHud)));
                    c.Remove();
                }
                else Log.Error("CameraRigController.UpdateHUD # 2");
            }
            else Log.Error("CameraRigController.UpdateHUD # 1");
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(HealthBar), nameof(HealthBar.UpdateBarInfos))]
        public static void FixInfusionColor(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchLdsfld(typeof(HealthBar), nameof(HealthBar.voidPanelColor))))
            {
                c.Remove();
                c.EmitDelegate<Func<Color>>(() =>
                {
                    var color = RiskUIPlugin.VoidColor.Value;//.Trim();
                    if (color == Color.clear)
                        return Color.HSVToRGB(Mathf.Sin(Time.time) * 0.5f + 0.5f, 1, 1);
                    return color; //ColorUtility.TryParseHtmlString(str, out var color) ? color : Color.magenta;
                });
            }
            else Log.Error("HealthBar.UpdateBarInfos # 1");

            if (c.TryGotoNext(x => x.MatchLdsfld(typeof(HealthBar), nameof(HealthBar.infusionPanelColor))))
            {
                c.Remove();
                c.EmitDelegate<Func<Color>>(() =>
                {
                    var color = RiskUIPlugin.InfusionColor.Value;//.Trim();
                    if (color == Color.clear)//"rainbow")
                        return Color.HSVToRGB(Mathf.Sin(Time.time) * 0.5f + 0.5f, 1, 1);
                    return color; //ColorUtility.TryParseHtmlString(str, out var color) ? color : Color.magenta;
                });
            }
            else Log.Error("HealthBar.UpdateBarInfos # 2");

            if (c.TryGotoNext(x => x.MatchLdsfld(typeof(HealthBar), nameof(HealthBar.voidShieldsColor))))
            {
                c.Remove();
                c.EmitDelegate<Func<Color>>(() =>
                {
                    var color = RiskUIPlugin.VoidShieldColor.Value;//.Trim();
                    if (color == Color.clear)//"rainbow")
                        return Color.HSVToRGB(Mathf.Sin(Time.time) * 0.5f + 0.5f, 1, 1);
                    return color; //ColorUtility.TryParseHtmlString(str, out var color) ? color : Color.magenta;
                });
            }
            else Log.Error("HealthBar.UpdateBarInfos # 3");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Run), nameof(Run.InstantiateUi))]
        public static bool OverwriteTracker(Run __instance, Transform uiRoot, ref GameObject __result)
        {
            if (!uiRoot) return true;
            switch (__instance.nameToken)
            {
                case "ECLIPSE_GAMEMODE_NAME":
                case "GAMEMODE_CLASSIC_RUN_NAME":
                    __result = UnityEngine.Object.Instantiate(RiskUIPlugin.CreateClassicRunHud(), uiRoot);
                    __instance.uiInstances.Add(__result);
                    return false;
                /*case "INFINITETOWER_GAMEMODE_NAME":
                    __result = GameObject.Instantiate(RiskUIPlugin.CreateSimulcrum(), uiRoot);
                    __instance.uiInstances.Add(__result);
                    return false;*/
                default:
                    return true;
            }
        }

        /*
		[HarmonyILManipulator,
		 HarmonyPatch(typeof(InfiniteTowerWaveController), nameof(InfiniteTowerWaveController.InstantiateUi))]
		public static void ReplaceWaveUI(ILContext il)
		{
			var c = new ILCursor(il);

			c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<InfiniteTowerWaveController>(nameof(InfiniteTowerWaveController.uiPrefab)));

			c.RemoveRange(2);
			c.EmitDelegate<Func<InfiniteTowerWaveController, Transform, GameObject>>((controller, transform) =>
			{
				var obj = GameObject.Instantiate(RiskUIPlugin.BaseWaveUI, transform);
				obj.transform.SetSiblingIndex(1);
				return obj;
			});
		}*/

        [HarmonyPrefix, HarmonyPatch(typeof(DifficultyDef), nameof(DifficultyDef.GetIconSprite))]
        public static bool SwapIcon(DifficultyDef __instance, ref Sprite __result)
        {
            if (__instance.nameToken == null || !RiskUIPlugin.DifficultyIconMap.ContainsKey(__instance.nameToken))
                return true;

            __result = RiskUIPlugin.DifficultyIconMap[__instance.nameToken];
            return false;
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(AllyCardManager), nameof(AllyCardManager.Awake))]
        public static void ReplaceAllyCards(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchLdstr("Prefabs/UI/AllyCard")))
            {
                c.RemoveRange(2);
                c.Emit(OpCodes.Call, AccessTools.Method(typeof(RiskUIPlugin), nameof(RiskUIPlugin.CreateAllyCard)));
            }
            else Log.Error("AllyCardManager.Awake");
        }

        //[HarmonyILManipulator, HarmonyPatch(typeof(VoidSurvivorController), nameof(VoidSurvivorController.OnEnable))]
        public static void MoveVoidSurvivorController(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchLdarg(0), x => x.MatchLdfld<VoidSurvivorController>("overlayChildLocatorEntry")))
            {
                c.RemoveRange(2);
                c.Emit(OpCodes.Ldstr, "BottomLeftCluster");
            }
            else Log.Error("VoidSurvivorController.OnEnable");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnemyInfoPanel), nameof(EnemyInfoPanel.Init))]
        public static void ChangeMonsterInventory() => EnemyInfoPanel.panelPrefab = RiskUIPlugin.EnemyInfoPanel;

        [HarmonyPrefix, HarmonyPatch(typeof(ScoreboardStrip), nameof(ScoreboardStrip.ResetNavIndicators))]
        public static bool ResetNav() => false;

        [HarmonyPostfix, HarmonyPatch(typeof(HUD), nameof(HUD.DeactivateScoreboard))]
        public static void Unselect(HUD __instance)
        {
            if (__instance.scoreboardPanel && __instance.scoreboardPanel.TryGetComponent<ScoreboardController>(out var ctrl) && ctrl.eventSystem)
            {
                ctrl.eventSystem.SetSelectedGameObject(null);
                ctrl.eventSystem.currentSelectedButton = null;
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(ItemIcon), nameof(ItemIcon.SetItemIndex))]
        public static void ItemCount(ILContext il)
        {
            ILCursor[] c = null;
            if (new ILCursor(il).TryFindNext(out c,
                    x => x.MatchLdfld(typeof(ItemIcon), nameof(ItemIcon.spriteAsNumberManager)),
                    x => x.MatchCallOrCallvirt<SpriteAsNumberManager>(nameof(SpriteAsNumberManager.SetItemCount)),
                    x => x.MatchLdfld(typeof(ItemIcon), nameof(ItemIcon.spriteAsNumberManager)),
                    x => x.MatchCallOrCallvirt<SpriteAsNumberManager>(nameof(SpriteAsNumberManager.SetVisible))
                ))
            {
                c[0].Remove();
                c[0].Emit<ItemIcon>(OpCodes.Ldfld, nameof(ItemIcon.stackText));
                c[1].Remove();
                c[1].EmitDelegate<Action<TextMeshProUGUI, int>>((image, num) =>
                {
                    image.text = "x" + num.ToString();
                    image.enabled = true;
                });
                c[2].Remove();
                c[2].Emit<ItemIcon>(OpCodes.Ldfld, nameof(ItemIcon.stackText));
                c[3].Remove();
                c[3].EmitDelegate<Action<TextMeshProUGUI, bool>>((image, num) =>
                {
                    image.text = "";
                    image.enabled = false;
                });
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ScoreboardStrip), nameof(ScoreboardStrip.UpdateItemCountText))]
        public static bool NoCount() => false;

        [HarmonyPrefix, HarmonyPatch(typeof(InspectPanelLocator), nameof(InspectPanelLocator.Awake))]
        public static bool NoPanel() => false;

        [HarmonyILManipulator, HarmonyPatch(typeof(ContextManager), nameof(ContextManager.Update))]
        public static void NoInspect(ILContext il)
        {
            var c = new ILCursor(il);
            c.Index = c.Instrs.Count - 1;

            if (c.TryGotoPrev(x => x.MatchCallOrCallvirt(typeof(GameObject), nameof(GameObject.SetActive))))
            {
                c.Remove();
                c.EmitDelegate<Action<GameObject, bool>>(Slurp2);
            }
            if (c.TryGotoPrev(x => x.MatchCallOrCallvirt(AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text)))))
            {
                c.Remove();
                c.EmitDelegate<Action<TMP_Text, string>>(Slurp);
            }
            if (c.TryGotoPrev(x => x.MatchCallOrCallvirt(AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text)))))
            {
                c.Remove();
                c.EmitDelegate<Action<TMP_Text, string>>(Slurp);
            }
            if (c.TryGotoPrev(x => x.MatchCallOrCallvirt(typeof(GameObject), nameof(GameObject.SetActive))))
            {
                c.Remove();
                c.EmitDelegate<Action<GameObject, bool>>(Slurp2);
            }
            if (c.TryGotoPrev(x => x.MatchCallOrCallvirt(AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text)))))
            {
                c.Remove();
                c.EmitDelegate<Action<TMP_Text, string>>(Slurp);
            }
            if (c.TryGotoPrev(x => x.MatchCallOrCallvirt(AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text)))))
            {
                c.Remove();
                c.EmitDelegate<Action<TMP_Text, string>>(Slurp);
            }

            void Slurp(TMP_Text _, string __)
            {
                if (_)
                    _.text = __;
            }
            void Slurp2(GameObject _, bool __)
            {
                if (_)
                    _.SetActive(__);
            }
        }
    }
}