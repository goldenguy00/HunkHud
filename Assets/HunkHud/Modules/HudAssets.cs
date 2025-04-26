using TMPro;
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
            HUD.onHudTargetChangedGlobal += HudAssets.HUD_OnHudTargetChangedGlobal;
            var all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            hook = new Hook(typeof(HUD).GetMethod(nameof(HUD.Awake), all), typeof(HudAssets).GetMethod(nameof(HUD_Awake), all));
        }

        internal static void HUD_Awake(System.Action<HUD> orig, HUD self)
        {
            orig(self);
            HUD_OnHudTargetChangedGlobal(self);
        }

        internal static void HUD_OnHudTargetChangedGlobal(HUD hud)
        {
            if (!PluginConfig.customHUD.Value)
                return;

            var targetBody = hud.targetMaster ? hud.targetMaster.GetBody() : null;
            if (targetBody && (targetBody.name.Contains("RobNemesis") || targetBody.name.Contains("RobHunk")))
                return;

            HandleHud(hud, targetBody);
        }
        
        private static void HandleHud(HUD hud, CharacterBody targetBody)
        {
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

            SetActiveSafe(hud.healthBar, false);
            SetActiveSafe(hud.expBar, false);
            SetActiveSafe(hud.levelText, false);

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
                var luminousDisplay = crosshair.Find("LuminousGauge")?.gameObject;//.GetComponent<BandDisplay>();
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

                if (hud.buffDisplay)
                {
                    hud.buffDisplay.transform.SetParent(bottomLeftCluster);

                    var rect = hud.buffDisplay.GetComponent<RectTransform>();
                    rect.pivot = Vector2.up;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.right;
                    rect.anchoredPosition = Vector2.zero;
                    rect.anchoredPosition3D = Vector3.zero;
                }

                var healthBar = bottomLeftCluster.Find("CustomHealthBar")?.gameObject;//?.GetComponent<CustomHealthBar>();
                if (!healthBar)
                {
                    healthBar = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("CustomHealthBar"), bottomLeftCluster);
                    healthBar.name = "CustomHealthBar";
                }
                healthBar.GetComponent<HealthBarMover>().UpdateReferences(hud, targetBody);

                var customHealthBar = healthBar.transform.Find("Center").GetComponent<CustomHealthBar>();
                customHealthBar.targetBody = targetBody;
                customHealthBar.SetCharacterIcon();
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
                var bandDisplay = bottomCenterCluster.Find("BandCooldownTracker")?.gameObject;//.GetComponent<BandDisplay>();
                if (!bandDisplay)
                {
                    bandDisplay = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("BandCooldownTracker"), bottomCenterCluster);
                    bandDisplay.name = "BandCooldownTracker";
                }
                bandDisplay.GetComponent<BandDisplayMover>().UpdateReferences(hud, targetBody);
            }
        }
        /*
        internal static void RiskUIHudSetup(RoR2.UI.HUD hud)
        {
            bool flag = hud.targetMaster.bodyPrefab.GetComponent<HunkController>();
            bool flag2 = hud.targetMaster.bodyPrefab.GetComponent<NemesisController>();
            if (!flag && !Config.globalCustomHUD.Value)
            {
                return;
            }
            Transform transform = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas")
                .Find("BottomRightCluster")
                .Find("Scaler");
            if ((bool)transform.Find("WeaponSlot"))
            {
                return;
            }
            RectTransform rectTransform = null;
            Transform transform2 = hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas")
                .Find("CrosshairExtras");
            if (Config.showWeaponIcon.Value && !Config.customHUD.Value && flag)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(transform.Find("EquipmentSlotPos1").Find("EquipIcon").gameObject, transform);
                gameObject.name = "WeaponSlot";
                RoR2.UI.EquipmentIcon component = gameObject.GetComponent<RoR2.UI.EquipmentIcon>();
                WeaponIcon weaponIcon = gameObject.AddComponent<WeaponIcon>();
                weaponIcon.iconImage = component.iconImage;
                weaponIcon.displayRoot = component.displayRoot;
                weaponIcon.flashPanelObject = component.stockFlashPanelObject;
                weaponIcon.reminderFlashPanelObject = component.reminderFlashPanelObject;
                weaponIcon.isReadyPanelObject = component.isReadyPanelObject;
                weaponIcon.tooltipProvider = component.tooltipProvider;
                weaponIcon.targetHUD = hud;
                MaterialEquipmentIcon component2 = gameObject.GetComponent<MaterialEquipmentIcon>();
                MaterialWeaponIcon materialWeaponIcon = gameObject.AddComponent<MaterialWeaponIcon>();
                materialWeaponIcon.icon = weaponIcon;
                materialWeaponIcon.onCooldown = component2.onCooldown;
                materialWeaponIcon.mask = component2.mask;
                materialWeaponIcon.stockText = component2.stockText;
                RectTransform component3 = gameObject.GetComponent<RectTransform>();
                component3.localScale = new Vector3(2f, 2f, 2f);
                component3.anchoredPosition = new Vector2(-128f, 60f);
                RoR2.UI.HGTextMeshProUGUI component4 = gameObject.transform.Find("DisplayRoot").Find("BottomContainer").Find("SkillBackgroundPanel")
                    .Find("SkillKeyText")
                    .gameObject.GetComponent<RoR2.UI.HGTextMeshProUGUI>();
                component4.gameObject.GetComponent<RoR2.InputBindingDisplayController>().enabled = false;
                component4.text = "Weapon";
                gameObject.transform.Find("DisplayRoot").Find("BottomContainer").Find("StockTextContainer")
                    .gameObject.SetActive(value: false);
                gameObject.transform.Find("DisplayRoot").Find("CooldownText").gameObject.SetActive(value: false);
                UnityEngine.Object.Destroy(component);
                UnityEngine.Object.Destroy(component2);
            }
            if (flag)
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate(hud.transform.Find("MainContainer").Find("NotificationArea").gameObject);
                gameObject2.transform.SetParent(hud.transform.Find("MainContainer"), worldPositionStays: true);
                gameObject2.GetComponent<RectTransform>().localPosition = new Vector3(0f, -210f, -50f);
                gameObject2.transform.localScale = Vector3.one;
                RoR2.UI.NotificationUIController component5 = gameObject2.GetComponent<RoR2.UI.NotificationUIController>();
                HunkNotificationUIController hunkNotificationUIController = gameObject2.AddComponent<HunkNotificationUIController>();
                hunkNotificationUIController.hud = component5.hud;
                hunkNotificationUIController.genericNotificationPrefab = HunkAssets.customNotificationPrefab;
                hunkNotificationUIController.notificationQueue = hud.targetMaster.gameObject.AddComponent<HunkNotificationQueue>();
                component5.enabled = false;
                if (!transform2.Find("AmmoDisplay"))
                {
                    GameObject gameObject3 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("AmmoPanel"), transform2);
                    gameObject3.name = "AmmoDisplay";
                    gameObject3.transform.SetParent(transform2);
                    AmmoDisplay2 ammoDisplay = gameObject3.AddComponent<AmmoDisplay2>();
                    ammoDisplay.targetHUD = hud;
                    ammoDisplay.currentText = gameObject3.transform.Find("Current").gameObject.GetComponent<TextMeshProUGUI>();
                    ammoDisplay.totalText = gameObject3.transform.Find("Total").gameObject.GetComponent<TextMeshProUGUI>();
                    ammoDisplay.bonusText = gameObject3.transform.Find("Bonus").gameObject.GetComponent<TextMeshProUGUI>();
                    rectTransform = gameObject3.GetComponent<RectTransform>();
                    rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    rectTransform.anchoredPosition = new Vector2(50f, 0f);
                    rectTransform.localPosition = new Vector3(100f, -150f, 0f);
                }
            }
            if (flag2)
            {
                GameObject gameObject4 = UnityEngine.Object.Instantiate(hud.transform.Find("MainContainer").Find("NotificationArea").gameObject);
                gameObject4.transform.SetParent(hud.transform.Find("MainContainer"), worldPositionStays: true);
                gameObject4.GetComponent<RectTransform>().localPosition = new Vector3(0f, -210f, -50f);
                gameObject4.transform.localScale = Vector3.one;
                RoR2.UI.NotificationUIController component6 = gameObject4.GetComponent<RoR2.UI.NotificationUIController>();
                HunkNotificationUIController hunkNotificationUIController2 = gameObject4.AddComponent<HunkNotificationUIController>();
                hunkNotificationUIController2.hud = component6.hud;
                hunkNotificationUIController2.genericNotificationPrefab = HunkAssets.customNotificationPrefab;
                hunkNotificationUIController2.notificationQueue = hud.targetMaster.gameObject.AddComponent<HunkNotificationQueue>();
                component6.enabled = false;
            }
            if (!transform2.Find("NotificationPanel"))
            {
                GameObject gameObject5 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("GenericTextPanel"), transform2);
                gameObject5.name = "NotificationPanel";
                gameObject5.transform.SetParent(transform2);
                gameObject5.AddComponent<HunkNotificationHandler>().targetHUD = hud;
                rectTransform = gameObject5.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(1f, 1f, 1f);
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.anchoredPosition = new Vector2(50f, 0f);
                rectTransform.localPosition = new Vector3(0f, -350f, 0f);
            }
            if (Config.customHUD.Value)
            {
                GameObject gameObject6 = hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster").GetComponentInChildren<RoR2.UI.EquipmentIcon>().gameObject;
                gameObject6.transform.Find("DisplayRoot/GameObject").gameObject.SetActive(value: false);
                gameObject6.transform.Find("DisplayRoot/Mask").gameObject.SetActive(value: false);
                gameObject6.transform.Find("DisplayRoot/BgImage").gameObject.GetComponent<Image>().enabled = false;
                gameObject6.transform.Find("DisplayRoot/BgImage/IconPanel/OnCooldown").gameObject.GetComponent<Image>().sprite = HunkAssets.mainAssetBundle.LoadAsset<Sprite>("texRadialInner");
                gameObject6.transform.Find("DisplayRoot/BgImage/IconPanel/OnCooldown").gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 1.5f;
                if (flag || flag2)
                {
                    hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler/SkillIconContainer").gameObject.SetActive(value: false);
                }
                else
                {
                    hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler").transform.localPosition = new Vector3(-705f, 0f, 0f);
                }
                hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/UpperLeftCluster").gameObject.AddComponent<HunkMoneyDisplay>();
                hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/TopCenterCluster/InventoryPosition2").gameObject.AddComponent<HunkItemDisplay>();
                hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/UpperRightCluster/MaterialClassicRunInfoHudPanel(Clone)").gameObject.AddComponent<HunkObjectiveDisplay>();
                if (flag)
                {
                    GameObject gameObject7 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("StaminaBar"), transform2);
                    gameObject7.name = "RailgunChargeBar";
                    gameObject7.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                    gameObject7.AddComponent<RailgunChargeBar>().targetHUD = hud;
                    RectTransform component7 = gameObject7.GetComponent<RectTransform>();
                    component7.localScale = new Vector3(1f, 1f, 1f);
                    component7.anchorMin = new Vector2(0f, 0f);
                    component7.anchorMax = new Vector2(0f, 0f);
                    component7.offsetMin = new Vector2(-150f, 0f);
                    component7.offsetMax = new Vector2(150f, 0f);
                    component7.pivot = new Vector2(0f, 0f);
                    component7.anchoredPosition = new Vector2(0f, 0f);
                    component7.localPosition = new Vector3(-50f, -50f, 0f);
                    component7.localRotation = Quaternion.identity;
                    component7.sizeDelta = new Vector2(100f, 10f);
                    GameObject gameObject8 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("StaminaBar"), transform2);
                    gameObject8.name = "StaminaBar";
                    gameObject8.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                    gameObject8.AddComponent<StaminaBar>().targetHUD = hud;
                    rectTransform = gameObject8.GetComponent<RectTransform>();
                    rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.offsetMin = new Vector2(-150f, 0f);
                    rectTransform.offsetMax = new Vector2(150f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    rectTransform.anchoredPosition = new Vector2(0f, 0f);
                    rectTransform.localPosition = new Vector3(-150f, -300f, 0f);
                    rectTransform.localRotation = Quaternion.identity;
                    rectTransform.sizeDelta = new Vector2(300f, 10f);
                    GameObject gameObject9 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("LuminousGauge"), transform2);
                    gameObject9.name = "LuminousGauge";
                    gameObject9.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                    gameObject9.AddComponent<LuminousDisplay>().targetHUD = hud;
                    rectTransform = gameObject9.GetComponent<RectTransform>();
                    rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.offsetMin = new Vector2(-150f, 0f);
                    rectTransform.offsetMax = new Vector2(150f, 0f);
                    rectTransform.pivot = new Vector2(0f, 0f);
                    rectTransform.anchoredPosition = new Vector2(0f, 0f);
                    rectTransform.localPosition = new Vector3(10f, -16f, 0f);
                    rectTransform.localRotation = Quaternion.identity;
                    rectTransform.sizeDelta = new Vector2(300f, 10f);
                }
                RoR2.UI.HealthBar healthBar = null;
                healthBar = hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomLeftCluster").gameObject.GetComponentInChildren<RoR2.UI.HealthBar>();
                if ((bool)healthBar)
                {
                    healthBar.transform.parent.gameObject.SetActive(value: false);
                }
                healthBar = hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomCenterCluster").gameObject.GetComponentInChildren<RoR2.UI.HealthBar>();
                if ((bool)healthBar)
                {
                    healthBar.transform.parent.gameObject.SetActive(value: false);
                }
                GameObject gameObject10 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("CustomHealthBar"), transform2);
                gameObject10.name = "CustomHealthBar";
                gameObject10.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster"));
                gameObject10.AddComponent<CustomHealthBar>().targetHUD = hud;
                rectTransform = gameObject10.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(1f, 1f, 1f);
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.anchoredPosition = new Vector2(0f, 0f);
                rectTransform.localPosition = new Vector3(-200f, 40f, 0f);
                rectTransform.localRotation = Quaternion.identity;
                gameObject10.transform.GetChild(0).gameObject.AddComponent<RectMover>().pos = new Vector3(-300f, 80f, 0f);
                gameObject6.transform.parent.SetParent(gameObject10.transform);
                gameObject6.AddComponent<RectMover>().pos = new Vector3(-180f, 90f, 0f);
                GameObject gameObject11 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("BandCooldownTracker"), transform2);
                gameObject11.name = "BandTracker";
                gameObject11.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                gameObject11.AddComponent<BandTracker>().targetHUD = hud;
                rectTransform = gameObject11.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(1f, 1f, 1f);
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.offsetMin = new Vector2(-150f, 0f);
                rectTransform.offsetMax = new Vector2(150f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.anchoredPosition = new Vector2(0f, 0f);
                rectTransform.localPosition = new Vector3(0f, 0f, 0f);
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.sizeDelta = new Vector2(100f, 100f);
                GameObject gameObject12 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("ObjectiveGauge"), transform2);
                gameObject12.name = "ObjectiveGauge";
                gameObject12.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                gameObject12.AddComponent<ObjectiveDisplay>().targetHUD = hud;
                rectTransform = gameObject12.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(1f, 1f, 1f);
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.offsetMin = new Vector2(-400f, 0f);
                rectTransform.offsetMax = new Vector2(400f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.anchoredPosition = new Vector2(0f, 0f);
                rectTransform.localPosition = new Vector3(-150f, 340f, 0f);
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.sizeDelta = new Vector2(300f, 10f);
            }
        }*/
    }
}
