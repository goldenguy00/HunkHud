using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HG;
using HunkMod.Modules;
using HunkMod.Modules.Components;
using HunkMod.Modules.Components.UI;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class CompatHandler : CustomHudElement
    {
        public TextMeshProUGUI currentText;
        public TextMeshProUGUI totalText;
        public TextMeshProUGUI bonusText;

        public GameObject ammoPanel;
        public GameObject textPanel;
        public RawImage gunIcon;

        public GameObject gunDisplay;
        public GameObject characterDisplay;
        public GameObject biomassDisplay;

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            base.HUD_onHudTargetChangedGlobal(newHud);
            CheckCompat();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void CheckCompat()
        {
            if (!this.targetMaster || !targetMaster.bodyPrefab || !this.hud)
                return;

            if (Chainloader.PluginInfos.ContainsKey("com.rob.Hunk"))
            {
                HunkCompat();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void HunkCompat()
        {
            SetupHealthbarWeaponIcon();
            MainContainer();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void MainContainer()
        {
            Transform transform2 = hud.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomLeftCluster/BarRoots/LevelDisplayCluster");
            if (!hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras/NotificationPanel"))
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("GenericTextPanel"), transform2);
                gameObject2.name = "NotificationPanel";
                gameObject2.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas")
                    .Find("CrosshairExtras"));
                HunkNotificationHandler hunkNotificationHandler = gameObject2.AddComponent<HunkNotificationHandler>();
                hunkNotificationHandler.targetHUD = hud;
                var rectTransform = gameObject2.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(1f, 1f, 1f);
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0f, 0f);
                rectTransform.pivot = new Vector2(0f, 0f);
                rectTransform.anchoredPosition = new Vector2(50f, 0f);
                rectTransform.localPosition = new Vector3(0f, -350f, 0f);
            }

            if (!hud.transform.Find("MainContainer/NotificationPanel"))
            {
                GameObject gameObject3 = UnityEngine.Object.Instantiate(hud.transform.Find("MainContainer/NotificationArea").gameObject);
                gameObject3.name = "NotificationPanel";
                gameObject3.transform.SetParent(hud.transform.Find("MainContainer"), worldPositionStays: true);
                gameObject3.GetComponent<RectTransform>().localPosition = new Vector3(0f, -265f, -150f);
                gameObject3.transform.localScale = Vector3.one;
                NotificationUIController component2 = gameObject3.GetComponent<NotificationUIController>();
                HunkNotificationUIController hunkNotificationUIController = gameObject3.AddComponent<HunkNotificationUIController>();
                hunkNotificationUIController.hud = component2.hud;
                hunkNotificationUIController.genericNotificationPrefab = HunkAssets.customNotificationPrefab;
                hunkNotificationUIController.notificationQueue = hud.targetMaster.gameObject.AddComponent<HunkNotificationQueue>();
                component2.enabled = false;
            }

            hud.transform.Find("MainContainer/NotificationArea").gameObject.AddComponent<AmmoNotificationHandler>();

            if (!hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras/AmmoTracker"))
            {
                if (Config.fancyAmmoDisplay.Value)
                {
                    GameObject gameObject7 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("AmmoPanel"), hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                    gameObject7.name = "AmmoDisplay";
                    gameObject7.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                    AmmoDisplay2 ammoDisplay = gameObject7.AddComponent<AmmoDisplay2>();
                    ammoDisplay.targetHUD = hud;
                    ammoDisplay.currentText = gameObject7.transform.Find("Current").gameObject.GetComponent<TextMeshProUGUI>();
                    ammoDisplay.totalText = gameObject7.transform.Find("Total").gameObject.GetComponent<TextMeshProUGUI>();
                    ammoDisplay.bonusText = gameObject7.transform.Find("Bonus").gameObject.GetComponent<TextMeshProUGUI>();
                    ammoDisplay.fontOverride = HunkAssets.hgFont;
                    var rectTransform = gameObject7.GetComponent<RectTransform>();
                    rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    rectTransform.anchoredPosition = new Vector2(50f, 0f);
                    rectTransform.localPosition = new Vector3(100f, -150f, 0f);
                }
                else
                {
                    GameObject gameObject8 = UnityEngine.Object.Instantiate(transform2.gameObject, hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas")
                        .Find("BottomLeftCluster"));
                    gameObject8.name = "AmmoTracker";
                    gameObject8.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
                    UnityEngine.Object.DestroyImmediate(gameObject8.transform.GetChild(0).gameObject);
                    UnityEngine.Object.Destroy(gameObject8.GetComponentInChildren<LevelText>());
                    UnityEngine.Object.Destroy(gameObject8.GetComponentInChildren<ExpBar>());
                    AmmoDisplay ammoDisplay2 = gameObject8.AddComponent<AmmoDisplay>();
                    ammoDisplay2.targetHUD = hud;
                    ammoDisplay2.targetText = gameObject8.transform.Find("LevelDisplayRoot").Find("PrefixText").gameObject.GetComponent<LanguageTextMeshController>();
                    gameObject8.transform.Find("LevelDisplayRoot").Find("ValueText").gameObject.SetActive(value: false);
                    gameObject8.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);
                    var rectTransform = gameObject8.GetComponent<RectTransform>();
                    rectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
                    rectTransform.anchorMin = new Vector2(0f, 0f);
                    rectTransform.anchorMax = new Vector2(0f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    rectTransform.anchoredPosition = new Vector2(50f, 0f);
                    rectTransform.localPosition = new Vector3(50f, -95f, 0f);
                }
            }

            GameObject gameObject9 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("StaminaBar"), transform2);
            gameObject9.name = "RailgunChargeBar";
            gameObject9.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
            gameObject9.AddComponent<RailgunChargeBar>().targetHUD = hud;
            RectTransform component6 = gameObject9.GetComponent<RectTransform>();
            component6.localScale = new Vector3(1f, 1f, 1f);
            component6.anchorMin = new Vector2(0f, 0f);
            component6.anchorMax = new Vector2(0f, 0f);
            component6.offsetMin = new Vector2(-150f, 0f);
            component6.offsetMax = new Vector2(150f, 0f);
            component6.pivot = new Vector2(0f, 0f);
            component6.anchoredPosition = new Vector2(0f, 0f);
            component6.localPosition = new Vector3(-50f, -50f, 0f);
            component6.localRotation = Quaternion.identity;
            component6.sizeDelta = new Vector2(100f, 10f);
            GameObject gameObject10 = UnityEngine.Object.Instantiate(HunkAssets.mainAssetBundle.LoadAsset<GameObject>("StaminaBar"), transform2);
            gameObject10.name = "MayhemChargeBar";
            gameObject10.transform.SetParent(hud.transform.Find("MainContainer/MainUIArea/CrosshairCanvas/CrosshairExtras"));
            gameObject10.AddComponent<MayhemGauge>().targetHUD = hud;
            component6 = gameObject10.GetComponent<RectTransform>();
            component6.localScale = new Vector3(1f, 1f, 1f);
            component6.anchorMin = new Vector2(0f, 0f);
            component6.anchorMax = new Vector2(0f, 0f);
            component6.offsetMin = new Vector2(-150f, 0f);
            component6.offsetMax = new Vector2(150f, 0f);
            component6.pivot = new Vector2(0f, 0f);
            component6.anchoredPosition = new Vector2(0f, 0f);
            component6.localPosition = new Vector3(-200f, -50f, 0f);
            component6.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            component6.sizeDelta = new Vector2(100f, 10f);
        }

        private void SetupHealthbarWeaponIcon()
        {
            if (!this.targetMaster.bodyPrefab.GetComponent<HunkController>())
            {
                characterDisplay.SetActive(value: true);
                characterDisplay.SetActive(value: false);
                gunDisplay.SetActive(value: false);
                return;
            }

            characterDisplay.SetActive(value: false);
            biomassDisplay.SetActive(value: false);
            gunDisplay.SetActive(value: true);
                textPanel.SetActive(value: false);
                ammoPanel.SetActive(value: true);
                gunIcon.gameObject.SetActive(true);

            var weaponIcon = gunDisplay.EnsureComponent<WeaponIcon>();
            weaponIcon.targetHUD = hud;
            weaponIcon.iconImage = gunIcon;
            weaponIcon.displayRoot = gunDisplay;

            var ammoDisplay = ammoPanel.EnsureComponent<AmmoDisplay2>();
            ammoDisplay.useDesiredAlpha = false;
            ammoDisplay.targetHUD = hud;
            ammoDisplay.currentText = currentText;
            ammoDisplay.totalText = totalText;
            ammoDisplay.bonusText = bonusText;
        }
    }
}