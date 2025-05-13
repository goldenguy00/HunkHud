using UnityEngine;
using RoR2.UI;
using HunkHud.Components.UI;
using HunkHud.Components;
using MaterialHud;
using System.Collections.Generic;
using RoR2;
using System.Linq;

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
            CustomHealthBar.Hook();

            SetRiskUISettings(RiskUIPlugin._newHud.GetComponent<HUD>());
        }

        private static void CrosshairController_Awake(On.RoR2.UI.CrosshairController.orig_Awake orig, CrosshairController self)
        {
            orig(self);

            if (self && !self.GetComponent<DynamicCrosshair>())
            {
                self.gameObject.AddComponent<DynamicCrosshair>();
            }
        }

        private static void SetRiskUISettings(HUD hud)
        {
            var springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas");

            var childLoc = hud.GetComponent<ChildLocator>();
            var newChildLoc = childLoc.transformPairs.ToList();
            
            newChildLoc.AddRange(new ChildLocator.NameTransformPair[]
            {
                new ChildLocator.NameTransformPair
                {
                    name = "NotificationArea",
                    transform = hud.mainContainer.transform.Find("NotificationArea")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "UpperLeftCluster",
                    transform = springCanvas.Find("UpperLeftCluster")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "SkillIconContainer",
                    transform = springCanvas.Find("BottomRightCluster/Scaler")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "AllyCardContainer",
                    transform = springCanvas.Find("LeftCluster/AllyCardContainer")
                }
            });

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

                        newChildLoc.Add(new ChildLocator.NameTransformPair
                        {
                            name = "BuffDisplayRoot",
                            transform = configObject.transform
                        });
                        break;

                    case "InventoryContainer":
                        configObject.choices[0] = configObject.choices[1];
                        configObject.transform.SetParent(configObject.choices[1], false);

                        newChildLoc.Add(new ChildLocator.NameTransformPair
                        {
                            name = "InventoryContainer",
                            transform = configObject.transform
                        });
                        break;
                }
            }

            RiskUIPlugin._allyCard.transform.Find("Healthbar/HealthTextContainer/CurrentHealthText").gameObject.SetActive(false);
            RiskUIPlugin._allyCard.transform.Find("Healthbar/HealthTextContainer/Slash").gameObject.SetActive(false);
            RiskUIPlugin._allyCard.transform.Find("Healthbar/HealthTextContainer/MaxHealthText").gameObject.SetActive(false);

            childLoc.transformPairs = newChildLoc.ToArray();
            childLoc.FindChild("AllyCardContainer").GetOrAddComponent<AllyHealthBarMover>();

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

            if (hud.gameModeUiInstance)
            {
                hud.gameModeUiInstance.GetOrAddComponent<ObjectiveDisplayMover>().UpdateReferences(hud, targetBody);
            }

            var childLoc = hud.GetComponent<ChildLocator>();

            var notification = childLoc.FindChildComponent<NotificationUIController>("NotificationArea");
            if (notification)
            {
                notification.genericNotificationPrefab = mainAssetBundle.LoadAsset<GameObject>("ItemNotification");
            }

            var topCenterCluster = childLoc.FindChild("TopCenterCluster");
            if (topCenterCluster)
            {
                topCenterCluster.FindOrInstantiate("ObjectiveGauge");
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

            var crosshair = childLoc.FindChild("CrosshairExtras");
            if (crosshair)
            {
                crosshair.FindOrInstantiate<LuminousDisplay>("LuminousGauge").UpdateReferences(hud, targetBody);
            }

            var bottomLeftCluster = childLoc.FindChild("BottomLeftCluster");
            if (bottomLeftCluster)
            {
                var healthBar = bottomLeftCluster.FindOrInstantiate("CustomHealthBar").transform.GetChild(0).GetComponent<CustomHealthBar>();
                healthBar.SetCharacterIcon();
                healthBar.hpBarMover.UpdateReferences(hud, targetBody);
                healthBar.bandDisplayController.UpdateReferences(hud, targetBody);

                FuckinLee(bottomLeftCluster, targetBody);
            }
        }

        private static void FuckinLee(Transform hud, CharacterBody targetBody)
        {
            var chatbox = hud.Find("ChatBoxPos1");
            var chatbox2 = hud.Find("ChatBoxPos2");

            if (chatbox && chatbox2)
            {
                var box = chatbox.Find("ChatBoxRoot");
                if (!box)
                    chatbox2.Find("ChatBoxRoot");

                var configManager = box ? box.GetComponent<BepinConfigParentManager>() : null;

                if (configManager)
                {
                    configManager.choices[0] = targetBody && targetBody.name == "LeeHyperrealBody(Clone)" ? chatbox2 : chatbox;
                    configManager.OnEnable();
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
