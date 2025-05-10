using UnityEngine;
using RoR2.UI;
using HunkHud.Components.UI;
using HunkHud.Components;
using MaterialHud;
using UnityEngine.UI;

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

            SetRiskUISettings(RiskUIPlugin._newHud.GetComponent<HUD>());
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
            var childLoc = hud.GetComponent<ChildLocator>();
            var springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas");
            RiskUIPlugin._allyCard.AddComponent<AllyHealthBarMover>();
            RiskUIPlugin._allyCard.transform.Find("Healthbar/HealthTextContainer/CurrentHealthText").gameObject.SetActive(false);
            RiskUIPlugin._allyCard.transform.Find("Healthbar/HealthTextContainer/Slash").gameObject.SetActive(false);
            RiskUIPlugin._allyCard.transform.Find("Healthbar/HealthTextContainer/MaxHealthText").gameObject.SetActive(false);
            foreach (var configObject in hud.GetComponentsInChildren<BepinConfigParentManager>())
            {
                switch (configObject.gameObject.name)
                {
                    case "BuffDisplayRoot":
                        var buffPos3 = configObject.choices[2];
                        buffPos3.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0f, -40f, 0f);

                        configObject.choices[0] = buffPos3;
                        configObject.choices[1] = buffPos3;
                        configObject.choices[2] = buffPos3;
                        configObject.choices[3] = buffPos3;
                        configObject.transform.SetParent(buffPos3, false);

                        var rect = configObject.GetComponent<RectTransform>();
                        rect.pivot = new Vector2(0.5f, 1f);
                        rect.anchoredPosition3D = Vector3.zero;
                        break;

                    case "InventoryContainer":
                        configObject.choices[0] = configObject.choices[1];
                        configObject.transform.SetParent(configObject.choices[1], false);

                        HG.ArrayUtils.ArrayAppend(ref childLoc.transformPairs,
                            new ChildLocator.NameTransformPair
                            {
                                name = "InventoryContainer",
                                transform = configObject.transform
                            });
                        break;

                    case "ChatBoxRoot":
                        //configObject.choices[0] = configObject.choices[1];
                        //configObject.transform.SetParent(configObject.choices[1], false);
                        break;
                }
            }

            HG.ArrayUtils.ArrayAppend(ref childLoc.transformPairs,
                new ChildLocator.NameTransformPair
                {
                    name = "NotificationArea",
                    transform = hud.mainContainer.transform.Find("NotificationArea")
                });
            HG.ArrayUtils.ArrayAppend(ref childLoc.transformPairs,
                new ChildLocator.NameTransformPair
                {
                    name = "UpperLeftCluster",
                    transform = springCanvas.Find("UpperLeftCluster")
                });
            HG.ArrayUtils.ArrayAppend(ref childLoc.transformPairs,
                new ChildLocator.NameTransformPair
                {
                    name = "SkillIconContainer",
                    transform = springCanvas.Find("BottomRightCluster/Scaler")
                });

            SetActiveSafe(childLoc.FindChild("BottomLeftCluster").Find("BarRoots/HealthText"), false);
            SetActiveSafe(childLoc.FindChild("BottomLeftCluster").Find("BarRoots/Seperator"), false);
            SetActiveSafe(hud.healthBar, false);
            SetActiveSafe(hud.expBar, false);
            SetActiveSafe(hud.levelText, false);

            HandleHud(hud);
        }

        private static void HandleHud(HUD hud)
        {
            /*
            if (PluginConfig.vignetteStrength.Value > 0f && !hud.mainContainer.transform.Find("Vignette"))
            {
                var component = Object.Instantiate(mainAssetBundle.LoadAsset<GameObject>("Vignette"), hud.mainContainer.transform).GetComponent<RectTransform>();
                component.gameObject.name = "Vignette";
                component.sizeDelta = Vector2.one;
                component.localPosition = Vector3.zero;
            }*/

            var targetBody = hud.targetMaster ? hud.targetMaster.GetBody() : null;
            var childLoc = hud.GetComponent<ChildLocator>();

            var notification = childLoc.FindChildComponent<NotificationUIController>("NotificationArea");
            if (notification)
            {
                notification.genericNotificationPrefab = mainAssetBundle.LoadAsset<GameObject>("ItemNotification");
            }

            if (hud.gameModeUiInstance)
            {
                hud.gameModeUiInstance.GetOrAddComponent<ObjectiveDisplayMover>().UpdateReferences(hud, targetBody);
            }

            var inventoryContainer = childLoc.FindChild("InventoryContainer");
            if (inventoryContainer)
            {
                inventoryContainer.GetOrAddComponent<ItemDisplayMover>().UpdateReferences(hud, targetBody);
            }

            var skillIconContainer = childLoc.FindChild("SkillIconContainer");
            if (skillIconContainer)
            {
                skillIconContainer.GetOrAddComponent<SkillIconMover>().UpdateReferences(hud, targetBody);
            }

            var upperLeftCluster = childLoc.FindChild("UpperLeftCluster");
            if (upperLeftCluster)
            {
                upperLeftCluster.GetOrAddComponent<MoneyDisplayMover>().UpdateReferences(hud, targetBody);
            }

            var topCenterCluster = childLoc.FindChild("TopCenterCluster");
            if (topCenterCluster)
            {
                topCenterCluster.FindOrInstantiate("ObjectiveGauge");
            }

            var crosshair = childLoc.FindChild("CrosshairExtras");
            if (crosshair)
            {
                crosshair.FindOrInstantiate<LuminousDisplay>("LuminousGauge").targetBody = targetBody;
            }

            var bottomLeftCluster = childLoc.FindChild("BottomLeftCluster");
            if (bottomLeftCluster)
            {
                var healthBar = bottomLeftCluster.FindOrInstantiate("CustomHealthBar").transform.GetChild(0).GetComponent<CustomHealthBar>();
                healthBar.SetCharacterIcon(targetBody);
                healthBar.hpBarMover.UpdateReferences(hud, targetBody);
                healthBar.bandDisplayController.UpdateReferences(hud.targetMaster?.inventory);

                if (targetBody && targetBody.name == "LeeHyperrealBody(Clone)")
                {
                    var chatbox = bottomLeftCluster.Find("ChatBoxPos1/ChatBoxRoot");
                    if (chatbox && bottomLeftCluster.Find("ChatBoxPos2"))
                    {
                        chatbox.SetParent(bottomLeftCluster.Find("ChatBoxPos2"), false);
                    }
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
