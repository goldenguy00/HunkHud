using UnityEngine;
using RoR2.UI;
using HunkHud.Components.UI;
using HunkHud.Components;
using MaterialHud;
using System.Runtime.CompilerServices;

namespace HunkHud.Modules
{
    public static class HudAssets
    {
        public static AssetBundle mainAssetBundle;
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
            On.RoR2.UI.CrosshairController.Awake += CrosshairController_Awake;
            HUD.onHudTargetChangedGlobal += HudAssets.HandleHud;

            HandleHud(RiskUIPlugin._newHud.GetComponent<HUD>());
        }

        private static void CrosshairController_Awake(On.RoR2.UI.CrosshairController.orig_Awake orig, CrosshairController self)
        {
            orig(self);

            if (self && !self.name.Contains("SprintCrosshair"))
            {
                self.GetOrAddComponent<DynamicCrosshair>();
            }
        }

        private static void SetRiskUISettings(HUD hud)
        {
            foreach (var configObject in hud.GetComponentsInChildren<BepinConfigParentManager>())
            {
                switch (configObject.gameObject.name)
                {
                    case "BuffDisplayRoot":
                        var springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas/BottomLeftCluster/BuffContainerPos3");
                        configObject.choices[0] = springCanvas;
                        configObject.choices[1] = springCanvas;
                        configObject.choices[2] = springCanvas;
                        configObject.choices[3] = springCanvas;

                        configObject.transform.SetParent(springCanvas, false);
                        configObject.transform.localPosition = Vector3.zero;

                        break;
                    case "InventoryContainer":
                        configObject.choices[0] = configObject.choices[1];

                        configObject.transform.SetParent(configObject.choices[1], false);

                        break;
                    case "ChatBoxRoot":
                        configObject.choices[0] = configObject.choices[1];

                        configObject.transform.SetParent(configObject.choices[1], false);

                        break;
                }
            }
        }

        private static void HandleHud(HUD hud)
        {
            if (!PluginConfig.customHUD.Value || !hud)
                return;

            SetRiskUISettings(hud);

            var targetBody = hud.targetMaster ? hud.targetMaster.GetBody() : null;

            var springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas");
            if (!springCanvas)
                return;

            var notification = hud.mainContainer.transform.Find("NotificationArea").GetComponent<NotificationUIController>();
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
                var transformParent = hud.itemInventoryDisplay.transform.parent.parent;
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
                crosshair.FindOrInstantiate<LuminousDisplay>("LuminousGauge").targetBody = targetBody;
            }

            var bottomLeftCluster = springCanvas.Find("BottomLeftCluster");
            if (bottomLeftCluster)
            {
                SetActiveSafe(bottomLeftCluster.Find("BarRoots/HealthText"), false);
                SetActiveSafe(bottomLeftCluster.Find("BarRoots/Seperator"), false);
                SetActiveSafe(hud.healthBar, false);
                SetActiveSafe(hud.expBar, false);
                SetActiveSafe(hud.levelText, false);

                var healthBar = bottomLeftCluster.FindOrInstantiate("CustomHealthBar").transform.GetChild(0).GetComponent<CustomHealthBar>();
                healthBar.SetCharacterIcon(targetBody);
                healthBar.hpBarMover.UpdateReferences(hud, targetBody);
                healthBar.bandDisplayController.UpdateReferences(hud.targetMaster?.inventory);
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
        }

        #region Extension Methods
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

        private static GameObject FindOrInstantiate(this Transform transform, string assetName)
        {
            var assetObject = transform.Find(assetName)?.gameObject;
            if (!assetObject)
            {
                assetObject = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>(assetName), transform);
                assetObject.name = assetName;
            }

            return assetObject;
        }

        private static T FindOrInstantiate<T>(this Transform transform, string assetName) where T : MonoBehaviour
        {
            var assetObject = transform.Find(assetName)?.gameObject;
            if (!assetObject)
            {
                assetObject = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>(assetName), transform);
                assetObject.name = assetName;
            }

            return assetObject.GetComponent<T>();
        }
        #endregion
    }
}
