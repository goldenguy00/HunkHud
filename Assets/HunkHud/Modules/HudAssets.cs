using UnityEngine;
using RoR2.UI;
using HunkHud.Components.UI;
using HunkHud.Components;
using MaterialHud;
using RoR2;
using System.Linq;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System;

namespace HunkHud.Modules
{
    public static class HudAssets
    {
        public static AssetBundle mainAssetBundle;

        private static Hook onLoadHook, notifHook, crosshairHook, hudAwakeHook;
        internal static void Init()
        {
            crosshairHook = new Hook(
                typeof(CrosshairController).GetMethod(nameof(CrosshairController.Awake), ~BindingFlags.Default),
                typeof(HudAssets).GetMethod(nameof(HudAssets.CrosshairController_Awake), ~BindingFlags.Default)
            );

            hudAwakeHook = new Hook(
                typeof(HUD).GetMethod(nameof(HUD.Awake), ~BindingFlags.Default),
                typeof(HudAssets).GetMethod(nameof(HudAssets.HUD_Awake), ~BindingFlags.Default)
            );

            onLoadHook = new Hook(
                typeof(RiskUIPlugin).GetMethod("OnLoad", ~BindingFlags.Default),
                typeof(HudAssets).GetMethod(nameof(HudAssets.RiskUIPlugin_onLoad), ~BindingFlags.Default)
            );

            notifHook = new Hook(
                typeof(NotificationAreaLoader).GetMethod("get_genericNotificationPrefab", ~BindingFlags.Default),
                typeof(HudAssets).GetMethod(nameof(HudAssets.NotificationAreaLoader_get_genericNotificationPrefab), ~BindingFlags.Default)
            );
        }

        private static void CrosshairController_Awake(Action<CrosshairController> orig, CrosshairController self)
        {
            orig(self);

            self.GetOrAddComponent<DynamicCrosshair>();
        }

        private static void HUD_Awake(Action<HUD> orig, HUD self)
        {
            orig(self);

            var childLoc = self.GetComponent<ChildLocator>();

            childLoc.FindChild("TopCenterCluster").FindOrInstantiate("ObjectiveGauge");
            childLoc.FindChild("CrosshairExtras").FindOrInstantiate("LuminousGauge");

            var hpBar = childLoc.FindChild("BottomLeftCluster").FindOrInstantiate("CustomHealthBar").transform.GetChild(0).GetComponent<CustomHealthBar>();

            self.healthBar = hpBar;
            self.levelText = hpBar.GetComponentInChildren<LevelText>();
            self.expBar = hpBar.GetComponentInChildren<ExpBar>();
        }

        private static GameObject NotificationAreaLoader_get_genericNotificationPrefab(Func<GameObject> _) => mainAssetBundle.LoadAsset<GameObject>("ItemNotification");

        private static void RiskUIPlugin_onLoad(Action<RiskUIPlugin> orig, RiskUIPlugin self)
        {
            orig(self);

            UpdateRiskUI();

            ModifyRiskUI();
        }

        private static void UpdateRiskUI()
        {
            var hud = RiskUIPlugin.CreateHud().GetComponent<HUD>();
            var childLoc = RiskUIPlugin.CreateHud().GetComponent<ChildLocator>();

            var newChildLoc = childLoc.transformPairs.ToList();
            var springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas");

            newChildLoc.AddRange(new ChildLocator.NameTransformPair[]
            {
                new ChildLocator.NameTransformPair
                {
                    name = "SpringCanvas",
                    transform = springCanvas
                },
                new ChildLocator.NameTransformPair
                {
                    name = "UpperRightCluster",
                    transform = springCanvas.Find("UpperRightCluster")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "BottomRightCluster",
                    transform = springCanvas.Find("BottomRightCluster")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "UpperLeftCluster",
                    transform = springCanvas.Find("UpperLeftCluster")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "BottomCenterCluster",
                    transform = springCanvas.Find("BottomCenterCluster")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "LeftCluster",
                    transform = springCanvas.Find("LeftCluster")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "RightCluster",
                    transform = springCanvas.Find("RightCluster")
                },

                new ChildLocator.NameTransformPair
                {
                    name = "NotificationArea",
                    transform = hud.mainContainer.transform.Find("NotificationArea")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "ScoreboardPanel",
                    transform = springCanvas.Find("ScoreboardPanel")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "SkillDisplayRoot",
                    transform = springCanvas.Find("BottomRightCluster/Scaler")
                },
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
                            name = "InventoryDisplayRoot",
                            transform = configObject.transform
                        });
                        break;
                }
            }

            childLoc.transformPairs = newChildLoc.ToArray();
        }

        private static void ModifyRiskUI()
        {
            var hud = RiskUIPlugin.CreateHud().GetComponent<HUD>();
            var childLoc = RiskUIPlugin.CreateHud().GetComponent<ChildLocator>();

            RiskUIPlugin.CreateClassicRunHud().AddComponent<ObjectiveDisplayMover>();
            RiskUIPlugin.CreateSimulcrum().AddComponent<ObjectiveDisplayMover>();

            RiskUIPlugin.CreateAllyCard().AddComponent<CanvasGroup>();
            RiskUIPlugin.CreateAllyCard().AddComponent<AllyHealthBarMover>();
            RiskUIPlugin.CreateAllyCard().transform.Find("Healthbar/HealthTextContainer/CurrentHealthText")?.SetInactiveSafe();
            RiskUIPlugin.CreateAllyCard().transform.Find("Healthbar/HealthTextContainer/Slash")?.SetInactiveSafe();
            RiskUIPlugin.CreateAllyCard().transform.Find("Healthbar/HealthTextContainer/MaxHealthText")?.SetInactiveSafe();

            childLoc.FindChild("InventoryDisplayRoot").gameObject.AddComponent<ItemDisplayMover>();
            childLoc.FindChild("SkillDisplayRoot").gameObject.AddComponent<SkillIconMover>();

            childLoc.FindChild("UpperLeftCluster").GetChild(0).gameObject.AddComponent<MoneyDisplayMover>().costType = CostTypeIndex.Money;
            childLoc.FindChild("UpperLeftCluster").GetChild(1).gameObject.AddComponent<MoneyDisplayMover>().costType = CostTypeIndex.VoidCoin;
            childLoc.FindChild("UpperLeftCluster").GetChild(2).gameObject.AddComponent<MoneyDisplayMover>().costType = CostTypeIndex.LunarCoin;

            childLoc.FindChild("BottomLeftCluster").Find("BarRoots/HealthText")?.SetInactiveSafe();
            childLoc.FindChild("BottomLeftCluster").Find("BarRoots/Seperator")?.SetInactiveSafe();

            hud.healthBar?.SetInactiveSafe();
            hud.expBar?.SetInactiveSafe();
            hud.levelText?.SetInactiveSafe();
        }

        #region Extension Methods
        private static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (!go)
                return null;

            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        private static T GetOrAddComponent<T>(this Component co) where T : Component
        {
            if (!co)
                return null;

            return co.GetComponent<T>() ?? co.gameObject.AddComponent<T>();
        }

        private static void SetInactiveSafe(this Component co, bool active = false)
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
                assetObject.transform.SetParent(transform);
            }

            return assetObject;
        }
        #endregion
    }
}
