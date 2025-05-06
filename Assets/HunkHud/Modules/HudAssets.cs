using UnityEngine;
using RoR2.UI;
using HunkHud.Components.UI;
using RoR2;
using HunkHud.Components;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace HunkHud.Modules
{
    public static class HudAssets
    {
        private static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }
        private static T GetOrAddComponent<T>(this Component co) where T : Component
        {
            return co.GetComponent<T>() ?? co.gameObject.AddComponent<T>();
        }
        private static void SetActiveSafe(GameObject go, bool active)
        {
            if (go)
                go.SetActive(active);
        }

        private static void SetActiveSafe(Component co, bool active)
        {
            if (co)
                co.gameObject.SetActive(active);
        }

        public static AssetBundle mainAssetBundle;
        private static Hook hook;
        /*
- Assets/hunk/hud/CustomHealthBar.prefab
- Assets/hunk/hud/ItemGetPopup.prefab
- Assets/hunk/hud/ItemNotification.prefab
- Assets/hunk/hud/Vignette.prefab
- Assets/hunk/hud/BandCooldownTracker.prefab
- Assets/hunk/hud/ObjectiveGauge.prefab
- Assets/hunk/hud/LuminousGauge.prefab


- Assets/hunk/hud/BiomassHolder.prefab
- Assets/hunk/hud/AmmoPanel.prefab
- Assets/hunk/hud/DodgeFlash.prefab
- Assets/hunk/hud/WeaponChargeBar.prefab
- Assets/hunk/hud/WeaponRadial.prefab
- Assets/hunk/hud/StaticOverlay.prefab
- Assets/hunk/hud/StaminaBar.prefab
- Assets/hunk/hud/JacketFullGauge.prefab
- Assets/hunk/hud/OldComboGauge.prefab
- Assets/hunk/hud/FlashbangOverlay.prefab
- Assets/hunk/hud/ChargeBar.prefab
- Assets/hunk/hud/ComboGauge2.prefab
- Assets/hunk/hud/ChargeRing.prefab
- Assets/hunk/hud/CounterFlash.prefab
         * */

        /*
         childLoc
[Error  :   HunkHud] RightInfoBar
[Error  :   HunkHud] TopCenterCluster
[Error  :   HunkHud] ScopeContainer
[Error  :   HunkHud] CrosshairExtras
[Error  :   HunkHud] BottomLeftCluster
[Error  :   HunkHud] BossHealthBar
[Error  :   HunkHud] RightUtilityArea
         */

        internal static void Init()
        {
            HUD.onHudTargetChangedGlobal += HudAssets.HandleHud;
            var all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            hook = new Hook(typeof(HUD).GetMethod(nameof(HUD.Awake), all), typeof(HudAssets).GetMethod(nameof(HUD_Awake), all));
        }

        private static void HUD_Awake(System.Action<HUD> orig, HUD self)
        {
            orig(self);

            HandleHud(self);
        }

        private static void HandleHud(HUD hud)
        {
            if (!PluginConfig.customHUD.Value || !hud)
                return;

            var targetBody = hud.targetMaster ? hud.targetMaster.GetBody() : null;

            var springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas");
            if (!springCanvas)
                return;

            var notification = hud.mainContainer.GetComponentInChildren<NotificationUIController>();
            if (notification)
            {
                notification.genericNotificationPrefab = mainAssetBundle.LoadAsset<GameObject>("ItemNotification");
            }

            if (hud.gameModeUiInstance)
            {
                hud.gameModeUiInstance.GetOrAddComponent<ObjectiveDisplayMover>().UpdateReferences(hud, targetBody);
            }

            if (hud.itemInventoryDisplay)
            {
                var transformParent = hud.itemInventoryDisplay.transform.parent;
                if (HunkHudMain.RiskUIEnabled)
                    transformParent = transformParent ? transformParent.parent : null;

                if (transformParent)
                    transformParent.GetOrAddComponent<ItemDisplayMover>().UpdateReferences(hud, targetBody);
            }

            if (PluginConfig.vignetteStrength.Value > 0f && !hud.mainContainer.transform.Find("Vignette"))
            {
                var component = Object.Instantiate(mainAssetBundle.LoadAsset<GameObject>("Vignette"), hud.mainContainer.transform).GetComponent<RectTransform>();
                component.gameObject.name = "Vignette";
                component.sizeDelta = Vector2.one;
                component.localPosition = Vector3.zero;
            }

            var upperLeftCluster = springCanvas.Find("UpperLeftCluster");
            if (upperLeftCluster)
            {
                upperLeftCluster.GetOrAddComponent<MoneyDisplayMover>().UpdateReferences(hud, targetBody);
            }

            var childLoc = hud.GetComponent<ChildLocator>();
            var topCenterCluster = childLoc.FindChild("TopCenterCluster");
            if (topCenterCluster)
            {
                if (!topCenterCluster.Find("ObjectiveGauge"))
                {
                    var objGauge = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("ObjectiveGauge"), topCenterCluster);
                    objGauge.name = "ObjectiveGauge";
                    objGauge.transform.SetSiblingIndex(1);
                }
            }

            var crosshair = childLoc.FindChild("CrosshairExtras");
            if (crosshair)
            {
                var luminousDisplay = crosshair.Find("LuminousGauge")?.gameObject;
                if (!luminousDisplay)
                {
                    luminousDisplay = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("LuminousGauge"), crosshair);
                    luminousDisplay.name = "LuminousGauge";
                }
                luminousDisplay.GetComponent<LuminousDisplay>().targetBody = targetBody;
            }

            var bottomLeftCluster = springCanvas.Find("BottomLeftCluster");
            if (bottomLeftCluster)
            {
                SetActiveSafe(bottomLeftCluster.Find("BarRoots/HealthText"), false);
                SetActiveSafe(bottomLeftCluster.Find("BarRoots/Seperator"), false);
                SetActiveSafe(hud.healthBar, false);
                SetActiveSafe(hud.expBar, false);
                SetActiveSafe(hud.levelText, false);
                
                if (!HunkHudMain.RiskUIEnabled && hud.buffDisplay)
                {
                    hud.buffDisplay.transform.localPosition = new Vector3(0f, -30f, 0f);
                }

                var healthBar = bottomLeftCluster.Find("CustomHealthBar")?.gameObject;
                if (!healthBar)
                {
                    healthBar = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("CustomHealthBar"), bottomLeftCluster);
                    healthBar.name = "CustomHealthBar";
                }
                healthBar.GetComponent<HealthBarMover>().UpdateReferences(hud, targetBody);

                var customHealthBar = healthBar.transform.Find("Center").GetComponent<CustomHealthBar>();
                customHealthBar.targetBody = targetBody;
                customHealthBar.SetCharacterIcon();

                healthBar.transform.Find("BandCooldownTracker").GetComponent<BandDisplayController>().UpdateReferences(hud.targetMaster?.inventory);
            }

            var bottomRightCluster = springCanvas.Find("BottomRightCluster");
            if (bottomRightCluster)
            {
                var bottomRightScaler = bottomRightCluster.Find("Scaler");
                if (bottomRightScaler)
                {
                    bottomRightScaler.GetOrAddComponent<SkillIconMover>().UpdateReferences(hud, targetBody);
                }
            }

            var bottomCenterCluster = springCanvas.Find("BottomCenterCluster");
            if (bottomCenterCluster)
            {
            }
        }
    }
}
