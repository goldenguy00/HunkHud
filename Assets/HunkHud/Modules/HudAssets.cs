using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.UI;
using HunkHud.Components.UI;
using RoR2;
using HunkHud.Components;

namespace HunkHud
{
    public static class HudAssets
    {
        public static AssetBundle mainAssetBundle;

        public static TMP_FontAsset hgFont;
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

        public static void Init()
        {
            //HUD.onHudTargetChangedGlobal += HUD_OnHudTargetChangedGlobal;
        }

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
        private static void HUD_OnHudTargetChangedGlobal(HUD hud)
        {
            //HandleHud(hud, targetBody);
        }

        private static void HandleHud(HUD hud, CharacterBody targetBody)
        {
            var childLoc = hud.GetComponent<ChildLocator>();
            var springCanvas = childLoc.FindChild("SpringCanvas");
            if (!springCanvas)
                return;

            var bottomRightScaler = springCanvas.Find("BottomRightCluster/Scaler");
            if (bottomRightScaler)
            {
                var skillDisplay = bottomRightScaler.GetComponent<SkillIconMover>() ?? bottomRightScaler.gameObject.AddComponent<SkillIconMover>();
                //skillDisplay.targetBody = targetBody;

                bottomRightScaler.Find("SprintCluster")?.gameObject.SetActive(false);
                bottomRightScaler.Find("InventoryCluster")?.gameObject.SetActive(false);
            }

            var upperLeftCluster = springCanvas.Find("UpperLeftCluster");
            if (upperLeftCluster)
            {
                var moneyDisplay = upperLeftCluster.GetComponent<MoneyDisplayMover>() ?? upperLeftCluster.gameObject.AddComponent<MoneyDisplayMover>();
                //moneyDisplay.targetMaster = hud.targetMaster;
                //moneyDisplay.cachedMoney = hud.targetMaster.money;
            }

            var topCenterCluster = childLoc.FindChild("TopCenterCluster");
            if (topCenterCluster)
            {
                // fuckin disgusting
                foreach (Transform child in topCenterCluster)
                {
                    if (child.GetComponentInChildren<ItemInventoryDisplay>())
                    {
                        var itemDisplay = child.GetComponent<ItemDisplayMover>();
                        if (!itemDisplay)
                            itemDisplay = child.gameObject.AddComponent<ItemDisplayMover>();

                        //itemDisplay.targetMaster = hud.targetMaster;
                        break;
                    }
                }

                if (!topCenterCluster.Find("ObjectiveGauge"))
                {
                    var objGauge = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("ObjectiveGauge"), topCenterCluster);
                    objGauge.name = "ObjectiveGauge";
                    objGauge.transform.SetSiblingIndex(1);
                    objGauge.AddComponent<ObjectiveChargeDisplay>();
                }
            }

            if (hud.gameModeUiInstance && !hud.gameModeUiInstance.GetComponent<ObjectiveDisplayMover>())
                hud.gameModeUiInstance.AddComponent<ObjectiveDisplayMover>();

            var crosshair = childLoc.FindChild("CrosshairExtras");
            if (crosshair)
            {
                var tracker = crosshair.Find("BandTracker")?.GetComponent<BandDisplay>();
                if (!tracker)
                {
                    var bandTracker = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("BandCooldownTracker"), crosshair);
                    bandTracker.name = "BandTracker";
                    tracker = bandTracker.AddComponent<BandDisplay>();
                }
                //tracker.targetBody = targetBody;
            }

            var bottomRightCluster = springCanvas.Find("BottomRightCluster");
            if (bottomRightCluster)
            {
                springCanvas.Find("BottomLeftCluster/BarRoots")?.gameObject.SetActive(false);

                var customHealthBar = bottomRightCluster.Find("CustomHealthBar")?.GetComponent<CustomHealthBar>();
                if (!customHealthBar)
                {
                    var healthBar = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("CustomHealthBar"), bottomRightCluster);
                    healthBar.name = "CustomHealthBar";
                    customHealthBar = healthBar.AddComponent<CustomHealthBar>();
                    /*
                    for (var i = 0; i < hud.equipmentIcons.Length; i++)
                    {
                        var equipmentIcon = hud.equipmentIcons[i];
                        if (!equipmentIcon)
                            continue;

                        equipmentIcon.transform.Find("DisplayRoot/BGPanel")?.gameObject.SetActive(false);
                        equipmentIcon.transform.Find("DisplayRoot/EquipmentTextBackgroundPanel")?.gameObject.SetActive(false);
                        equipmentIcon.transform.SetParent(customHealthBar.transform.Find("Center/EquipmentContainer"));

                        var img = equipmentIcon.isReadyPanelObject?.GetComponent<Image>();
                        if (img)
                            img.enabled = false;
                    }*/
                }
                //customHealthBar.targetBody = targetBody;
                //customHealthBar.activeTimer = 8f;

                if (hud.contextManager)
                {
                    var rect = hud.contextManager.GetComponent<RectTransform>();
                    rect.SetParent(bottomRightCluster);
                    rect.localScale = 0.75f * Vector3.one;
                    rect.localPosition = new Vector3(-550, 50f, 0f);
                }
            }

            //if (RoR2.RoR2Application.isInSinglePlayer)
            //    springCanvas.Find("BottomLeftCluster/ChatBoxRoot").localPosition = new Vector3(-2000f, -135.48f, 0f);
        }
    }
}
