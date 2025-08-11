using UnityEngine;
using RoR2.UI;
using HunkHud.Components;
using MaterialHud;
using RoR2;
using System.Linq;

namespace HunkHud.Modules
{
    public static class HudAssets
    {
        public static AssetBundle mainAssetBundle;

        internal static void UpdateRiskUI()
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

        internal static void ModifyRiskUI()
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
        internal static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (!go)
                return null;

            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        internal static T GetOrAddComponent<T>(this Component co) where T : Component
        {
            if (!co)
                return null;

            return co.GetComponent<T>() ?? co.gameObject.AddComponent<T>();
        }

        internal static void SetInactiveSafe(this Component co, bool active = false)
        {
            if (co)
                co.gameObject.SetActive(active);
        }

        internal static GameObject FindOrInstantiate(this Transform transform, string assetName)
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
